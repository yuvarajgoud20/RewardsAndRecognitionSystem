using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RewardsAndRecognitionSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IUserRepo _userRepo;
        private readonly UserManager<User> _userManager;
        private readonly ITeamRepo _teamRepo;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(
            IUserRepo userRepo,
            UserManager<User> userManager,
            ITeamRepo teamRepo,
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager)
        {
            _userRepo = userRepo;
            _userManager = userManager;
            _teamRepo = teamRepo;
            _context = context;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string role, string name)
        {
            var users = await _context.Users
                .Include(u => u.Team)
                .ThenInclude(t => t.Manager)
                .ToListAsync();

            var userRoles = new Dictionary<string, string>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "No Role";
            }

            if (!string.IsNullOrEmpty(role))
                users = users.Where(u => userRoles[u.Id] == role).ToList();

            if (!string.IsNullOrEmpty(name))
                users = users.Where(u => u.Name != null && u.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();

            ViewBag.UserRoles = userRoles;
            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.SelectedRole = role;
            ViewBag.NameQuery = name;

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(string role, string name)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var usersQuery = _context.Users
                .Include(u => u.Team)
                .ThenInclude(t => t.Manager)
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
                usersQuery = usersQuery.Where(u => u.Name.Contains(name));

            var users = await usersQuery.ToListAsync();

            var userRoles = new Dictionary<string, string>();
            foreach (var user in users)
            {
                var rolesList = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = rolesList.FirstOrDefault() ?? "None";
            }

            if (!string.IsNullOrEmpty(role))
                users = users.Where(u => userRoles[u.Id] == role).ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Users");

                worksheet.Cells[1, 1].Value = "Name";
                worksheet.Cells[1, 2].Value = "Email";
                worksheet.Cells[1, 3].Value = "Team";
                worksheet.Cells[1, 4].Value = "Manager";
                worksheet.Cells[1, 5].Value = "Role";

                int row = 2;
                foreach (var user in users)
                {
                    worksheet.Cells[row, 1].Value = user.Name;
                    worksheet.Cells[row, 2].Value = user.Email;
                    worksheet.Cells[row, 3].Value = user.Team?.Name ?? "Not Assigned";
                    worksheet.Cells[row, 4].Value = user.Team?.Manager?.Name ?? "No Manager";
                    worksheet.Cells[row, 5].Value = userRoles[user.Id];
                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");
            }
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropDowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string password, string SelectedRole)
        {
            if (ModelState.IsValid)
            {
                user.UserName = user.Email;
                user.NormalizedEmail = user.Email.ToUpper();
                user.NormalizedUserName = user.Email.ToUpper();
                user.EmailConfirmed = true;
                user.CreatedAt = DateTime.UtcNow;

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    var roleToAssign = string.IsNullOrEmpty(SelectedRole) ? "Employee" : SelectedRole;
                    var roleResult = await _userManager.AddToRoleAsync(user, roleToAssign);

                    if (!roleResult.Succeeded)
                    {
                        foreach (var error in roleResult.Errors)
                            ModelState.AddModelError("", error.Description);

                        await PopulateDropDowns();
                        return View(user);
                    }

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }

            await PopulateDropDowns();
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> EditPartial(string id)
        {
            var user = await _userManager.Users
                .Include(u => u.Team)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.SelectedRole = userRoles.FirstOrDefault();

            await PopulateDropDowns();
            return PartialView("_EditPartial", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User updatedUser, string SelectedRole)
        {
            var user = await _userManager.FindByIdAsync(updatedUser.Id);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                user.Name = updatedUser.Name;
                user.Email = updatedUser.Email;
                user.UserName = updatedUser.Email;
                user.NormalizedEmail = updatedUser.Email.ToUpper();
                user.NormalizedUserName = updatedUser.Email.ToUpper();
                user.TeamId = updatedUser.TeamId;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    var existingRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, existingRoles);
                    if (!string.IsNullOrEmpty(SelectedRole))
                        await _userManager.AddToRoleAsync(user, SelectedRole);

                    return Json(new { success = true });
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }

            await PopulateDropDowns();
            ViewBag.SelectedRole = SelectedRole;
            return PartialView("_EditPartial", updatedUser);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Error deleting user.");
            return RedirectToAction(nameof(Index));
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
