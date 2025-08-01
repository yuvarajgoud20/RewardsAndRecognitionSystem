﻿using AutoMapper;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RewardsAndRecognitionRepository;
using RewardsAndRecognitionRepository.Enums;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionRepository.Service;
using RewardsAndRecognitionSystem.Common;
using RewardsAndRecognitionSystem.Utilities;
using RewardsAndRecognitionSystem.ViewModels;

namespace RewardsAndRecognitionSystem.Controllers
{
    [Authorize(Roles = nameof(Roles.Admin))]
    public class UserController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IUserRepo _userRepo;
        private readonly UserManager<User> _userManager;
        private readonly ITeamRepo _teamRepo;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public UserController(
            IMapper mapper,
            IUserRepo userRepo,
            UserManager<User> userManager,
            ITeamRepo teamRepo,
            ApplicationDbContext context,
            IEmailService emailService)
        {
            _mapper = mapper;
            _userRepo = userRepo;
            _userManager = userManager;
            _teamRepo = teamRepo;
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index(string filter = "active",int page = 1)
        {
            int pageSize = 25;
            IQueryable<User> usersQuery = _context.Users
                    .Include(u => u.Team)
                    .ThenInclude(t => t.Manager)
                    .OrderBy(u => u.Name);
            if (filter == "deleted")
            {
                usersQuery = usersQuery.Where(u => u.IsDeleted == true);
            }
            else
            {
                usersQuery = usersQuery.Where(u => u.IsDeleted == false || u.IsDeleted == null);
            }
            var totalRecords = await usersQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var users = await usersQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userRoles = new Dictionary<string, string>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? GeneralMessages_User.NoRoleError;
            }

            ViewBag.UserRoles = userRoles;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.ActionName = nameof(Index);

            var usersList = _mapper.Map<List<UserViewModel>>(users);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (Request.Query.ContainsKey("paginationOnly"))
                {
                    return PartialView("_PaginationPartial", (page, totalPages, nameof(Index)));
                }
                return PartialView("_UserListPartial", usersList);
            }

            return View(usersList);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropDowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropDowns();
                return View(viewModel);
            }

            var user = _mapper.Map<User>(viewModel);
            if (ModelState.IsValid)
            {
                user.UserName = user.Email;
                user.NormalizedEmail = user.Email.ToUpper();
                user.EmailConfirmed = true;
                user.CreatedAt = DateTime.UtcNow;

                var result = await _userManager.CreateAsync(user, viewModel.PasswordHash);

                if (result.Succeeded)
                {
                    var roleToAssign = viewModel.SelectedRole;
                    var roleResult = await _userManager.AddToRoleAsync(user, roleToAssign);
                    if (!roleResult.Succeeded)
                    {
                        foreach (var error in roleResult.Errors)
                            ModelState.AddModelError("", error.Description);
                        await PopulateDropDowns();
                        return View(user);
                    }

                    await _emailService.SendEmailAsync(
                        subject:GeneralMessages_User.EmailSubjectWelcome,
                        to: user.Email,
                        isHtml: true,
                        body: $@"
                        <body style=""font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #ffffff;"">
                          <div style=""background-color: #ffffff; padding: 10px 20px; max-width: 600px; margin: auto; color: #000;"">
                            <img src=""cid:bannerImage"" alt=""Zelis Banner"" style=""width: 100%; max-width: 600px;"">
                            <p>Dear <strong>{user.Name}</strong>,</p>
                            <p>Welcome to the <strong>Rewards and Recognition</strong> platform!</p>
                            <p>Your account has been successfully created. Below are your login credentials:</p>
                            <p style=""font-size: 16px;"">
                              <strong>Email:</strong> {user.Email}<br>
                              <strong>Temporary Password:</strong> <span style=""color: #black;"">{viewModel.PasswordHash}</span>
                            </p>
                            <p>Please log in and <strong>change your password immediately</strong> to secure your account.</p>
                            <p>If you have any questions or need help accessing your account, feel free to contact our support team.</p>
                            <p style=""font-size: 13px; color: #999; text-align: center;"">
                              — This email was sent from the <strong>Rewards & Recognition</strong> system
                            </p>
                          </div>
                        </body>"
                    );
                    TempData["message"] = "Successfully created User";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }

            await PopulateDropDowns();
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userWithTeam = await _context.Users
                .Include(u => u.Team)
                .FirstOrDefaultAsync(u => u.Id == user.Id);


            var viewModel = new EditUserViewModel
            {
                Id = userWithTeam.Id,
                Name = userWithTeam.Name,
                Email = userWithTeam.Email,
                TeamId = userWithTeam.TeamId,
                Team = userWithTeam.Team
            };

            var roles = await _userManager.GetRolesAsync(user);
            bool isRestrictedRole = roles.Contains(nameof(Roles.TeamLead))
                       || roles.Contains(nameof(Roles.Manager))
                       || roles.Contains(nameof(Roles.Director));

            ViewBag.CanEditTeam = !isRestrictedRole;

            await PopulateDropDowns();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel updatedUserViewModel)
        {
            if (!ModelState.IsValid)
            {
                var usermodel = await _userManager.FindByIdAsync(id);
                var userViewmodel = _mapper.Map<EditUserViewModel>(usermodel);
                ModelState.Clear();
                await PopulateDropDowns();
                return View(userViewmodel);
            }

            var updatedUser = _mapper.Map<User>(updatedUserViewModel);
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            bool isTeamLeadOrManager = roles.Contains(nameof(Roles.TeamLead))
                          || roles.Contains(nameof(Roles.Manager));
            if (isTeamLeadOrManager && updatedUser.TeamId != user.TeamId)
            {
                ModelState.AddModelError("", GeneralMessages_User.TeamChangeError);
                await PopulateDropDowns();
                return View(updatedUserViewModel);
            }

            if (ModelState.IsValid)
            {
                user.NormalizedUserName = updatedUser.Email.ToUpper();
                user.UserName = updatedUser.Email;
                user.Name = updatedUser.Name;
                user.Email = updatedUser.Email;
                user.NormalizedEmail = updatedUser.Email.ToUpper();
                user.TeamId = updatedUser.TeamId;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["message"] = "Successfully updated User";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }

            ModelState.Clear();
            await PopulateDropDowns();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsDeleted = true;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                // handle failure (optional)
                ModelState.AddModelError("", "Unable to soft delete the user.");
            }
            TempData["message"] = "Successfully deleted User";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ExportAllUsersOpenXml()
        {
            var users = await _context.Users
                .Include(u => u.Team)
                .Include(u => u.Team.Manager)
                .OrderBy(n => n.Name)
                .ToListAsync();

            var userRoles = new Dictionary<string, string>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? GeneralMessages_User.UnknownRole;
            }

            using var memStream = new MemoryStream();
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(memStream, SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                worksheetPart.Worksheet = new Worksheet(sheetData);

                var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = SheetClassesStyles.CreateStylesheet();
                stylesPart.Stylesheet.Save();

                var headers = new[] { "Name", "Email", "Team", "Manager", "Role" };
                var headerRow = new Row();
                foreach (var header in headers)
                {
                    headerRow.Append(SheetClassesStyles.CreateStyledCell(header, 2));
                }
                sheetData.Append(headerRow);

                foreach (var user in users)
                {
                    var row = new Row();
                    row.Append(SheetClassesStyles.CreateStyledCell(user.Name, 1));
                    row.Append(SheetClassesStyles.CreateStyledCell(user.Email, 1));
                    row.Append(SheetClassesStyles.CreateStyledCell(user.Team?.Name ?? "Not Assigned", 1));
                    row.Append(SheetClassesStyles.CreateStyledCell(user.Team?.Manager?.Name ?? "No Manager", 1));
                    row.Append(SheetClassesStyles.CreateStyledCell(userRoles.ContainsKey(user.Id) ? userRoles[user.Id] : "Unknown", 1));
                    sheetData.Append(row);
                }

                var columns = new Columns(new Column { Min = 1, Max = 5, Width = 25, CustomWidth = true });
                worksheetPart.Worksheet.InsertAt(columns, 0);

                Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                Sheet sheet = new Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Users"
                };
                sheets.Append(sheet);
                workbookPart.Workbook.Save();
            }

            memStream.Seek(0, SeekOrigin.Begin);
            return File(memStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");
        }

        private async Task PopulateDropDowns()
        {
            var teams = await _teamRepo.GetAllAsync();
            var managers = await _userRepo.GetAllManagersAsync();
            var roles = new List<string> { "Admin", "TeamLead", "Manager", "Director", "Employee" };

            ViewBag.Teams = new SelectList(teams, "Id", "Name");
            ViewBag.Managers = new SelectList(managers, "Id", "Name");
            ViewBag.Roles = roles;
        }
    }
}
