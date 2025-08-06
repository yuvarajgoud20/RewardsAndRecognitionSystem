using System.Drawing.Printing;
using AutoMapper;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RewardsAndRecognitionRepository.Enums;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionRepository.Repos;
using RewardsAndRecognitionRepository.Repositories;
using RewardsAndRecognitionRepository.Service;
using RewardsAndRecognitionSystem.Common;
using RewardsAndRecognitionSystem.Utilities;
using RewardsAndRecognitionSystem.ViewModels;
//Export only for all filter
namespace RewardsAndRecognitionSystem.Controllers
{
   
    [Authorize(Roles = nameof(Roles.Manager) + "," + nameof(Roles.TeamLead) + "," + nameof(Roles.Director) + "," + nameof(Roles.Admin))]
    public class NominationController : Controller
    {
        private readonly IMapper _mapper;
        private readonly INominationRepo _nominationRepo;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly ICategoryRepo _categoryRepo;
        private readonly PaginationSettings _paginationSettings;

        public NominationController(
            IMapper mapper,
            INominationRepo nominationRepo,
            ApplicationDbContext context,
            UserManager<User> userManager,
            IEmailService emailService,
            ICategoryRepo categoryRepo,
             IOptions<PaginationSettings> paginationOptions)

        {
            _nominationRepo = nominationRepo;
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _emailService = emailService;
            _categoryRepo = categoryRepo;
            _paginationSettings = paginationOptions.Value;
        }

        // GET: Nomination
        public async Task<IActionResult> Index(Guid? yearQuarterId, string filter = "all", string FilterForDelete = "active", int page = 1)
        {
            ViewBag.filter = filter;
            int pageSize = _paginationSettings.DefaultPageSize;
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            List<Nomination> allNominations = new();
            List<Nomination> deletednominations = new();
            var selectedQuarter = yearQuarterId.HasValue
             ? await _context.YearQuarters.FirstOrDefaultAsync(yq => yq.Id == yearQuarterId.Value)
             : await _context.YearQuarters.FirstOrDefaultAsync(yq => yq.IsActive);
            if (selectedQuarter == null)
            {
                TempData["Message"] = GeneralMessages.No_Valid_Quarter;
              
                return View(new List<Nomination>());
            }
            ViewBag.IsQuarterActive = selectedQuarter.IsActive;

            ViewBag.currentUser = currentUser;
            ViewBag.SelectedYearQuarterId = selectedQuarter.Id;
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            List<Nomination> nominationsToShow = new();
            if (userRoles.Contains(nameof(Roles.Director)))
            {
                nominationsToShow = await _context.Nominations
                 .Include(n => n.Nominee).ThenInclude(u => u.Team)
                 .Include(n => n.Category)
                 .Include(n => n.Approvals)
                 .Include(n => n.Nominator)
                 .Where(n => n.YearQuarterId == selectedQuarter.Id)
                 .Where(n => n.Nominee.Team.DirectorId == currentUser.Id)
                 .Where(n => n.Status != NominationStatus.PendingManager)
                 .Where(n => n.Approvals != null)
                 .Where(n => n.IsDeleted == false || n.IsDeleted == null)
                 .ToListAsync();
                if (filter == "pending")
                {
                    nominationsToShow = nominationsToShow.Where(n => n.Status == NominationStatus.ManagerApproved || n.Status == NominationStatus.ManagerRejected).ToList();
                }

                if (filter == "directorapproved")
                {
                    nominationsToShow = nominationsToShow.Where(n => n.Status == NominationStatus.DirectorApproved).ToList();
                }
                if (filter == "directorrejected")
                {
                    nominationsToShow = nominationsToShow.Where(n => n.Status == NominationStatus.DirectorRejected).ToList();
                }
                // Track reviewed nominations
                var alreadyReviewedIds = nominationsToShow
                        .Where(n => n.Approvals.Any(a => a.ApproverId == currentUser.Id))
                        .Select(n => n.Id)
                        .ToList();

                ViewBag.ReviewedNominationIds = alreadyReviewedIds;
                ViewBag.TotalPages = (int)Math.Ceiling(nominationsToShow.Count / (double)pageSize);
                ViewBag.CurrentPage = page;
                ViewBag.ReviewedNominationIds = alreadyReviewedIds;
               
                var pagedNominations = nominationsToShow
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                var directorList = _mapper.Map<List<NominationViewModel>>(pagedNominations);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_NominationListPartial", directorList);
                }

                return View(directorList);
            }           
           if (userRoles.Contains(nameof(Roles.Manager)))
           {

                nominationsToShow = await _context.Nominations
                   .Include(n => n.Nominee).ThenInclude(u => u.Team)
                   .Include(n => n.Category)
                   .Include(n => n.Approvals)
                   .Include(n => n.Nominator)
                   .Where(n => n.YearQuarterId == selectedQuarter.Id)
                   .Where(n => n.Nominee.Team.ManagerId == currentUser.Id)
                   .Where(n => n.IsDeleted == false || n.IsDeleted == null)
                   .ToListAsync();
                if (filter == "pending")
                {
                    nominationsToShow = nominationsToShow.Where(n => n.Status == NominationStatus.PendingManager).ToList();
                }

                if (filter == "directorapproved")
                {
                    nominationsToShow = nominationsToShow.Where(n => n.Status == NominationStatus.DirectorApproved).ToList();
                }
                if (filter == "directorrejected")
                {
                    nominationsToShow = nominationsToShow.Where(n => n.Status == NominationStatus.DirectorRejected).ToList();
                }
                // Track reviewed nominations
                var alreadyReviewedIds = nominationsToShow
                    .Where(n => n.Approvals.Any(a => a.ApproverId == currentUser.Id))
                    .Select(n => n.Id)
                    .ToList();

                ViewBag.ReviewedNominationIds = alreadyReviewedIds;
                ViewBag.TotalPages = (int)Math.Ceiling(nominationsToShow.Count / (double)pageSize);
                ViewBag.CurrentPage = page;
                var pagedNominations = nominationsToShow
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                var managerList = _mapper.Map<List<NominationViewModel>>(pagedNominations);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_NominationListPartial", managerList);
                }
                return View(managerList);
            }
            if (userRoles.Contains(nameof(Roles.TeamLead)))              
            {
                nominationsToShow = await _context.Nominations
                .Include(n => n.Nominee).ThenInclude(u => u.Team)
                .Include(n => n.Category)
                .Include(n => n.Approvals)
                .Include(n => n.Nominator)
                .Where(n => n.YearQuarterId == selectedQuarter.Id)
                .Where(n => n.NominatorId == currentUser.Id)
                .ToListAsync();
                if (filter == "pending")
                {
                    nominationsToShow = nominationsToShow.Where(n => n.Status == NominationStatus.PendingManager ||
                                                 n.Status == NominationStatus.ManagerRejected ||
                                                 n.Status == NominationStatus.ManagerApproved).
                                                  Where(n=>n.IsDeleted==false || n.IsDeleted==null).ToList();
                }
                if (filter == "directorapproved")
                {
                    nominationsToShow = nominationsToShow.Where(n => n.Status == NominationStatus.DirectorApproved).Where(n => n.IsDeleted == false || n.IsDeleted == null).ToList();
                }
                if (filter == "directorrejected")
                {
                    nominationsToShow = nominationsToShow.Where(n => n.Status == NominationStatus.DirectorRejected).Where(n => n.IsDeleted == false || n.IsDeleted == null).ToList();
                }
                if (FilterForDelete == "deleted")
                {
                    allNominations = nominationsToShow.Where(n => n.IsDeleted).ToList();
                }
                else
                {
                    allNominations = nominationsToShow.Where(n => !n.IsDeleted).ToList();
                }
                ViewBag.FilterForDelete = FilterForDelete;
                var alreadyReviewedIds = nominationsToShow
                                    .Where(n => n.Approvals.Any(a => a.ApproverId == currentUser.Team.DirectorId))
                                    .Select(n => n.Id)
                                    .ToList();
                ViewBag.ReviewedNominationIds = alreadyReviewedIds;
                ViewBag.TotalPages = (int)Math.Ceiling(allNominations.Count / (double)pageSize);
                ViewBag.CurrentPage = page;
                ViewBag.ReviewedNominationIds = alreadyReviewedIds;
                var pagedNominations = allNominations
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                var teamList = _mapper.Map<List<NominationViewModel>>(pagedNominations);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_NominationListPartial", teamList);
                }
                return View(teamList);
            }
            if (userRoles.Contains(nameof(Roles.Admin)))
               
            {
                nominationsToShow = await _context.Nominations
                .Include(n => n.Nominee)
                .Include(n => n.Nominator).ThenInclude(u => u.Team)
                .Include(n => n.Category)
                .Where(n => n.YearQuarterId == selectedQuarter.Id)
                .Where(n => n.IsDeleted == false || n.IsDeleted == null)
                .ToListAsync();
            }
            var viewModelList = _mapper.Map<List<NominationViewModel>>(nominationsToShow);
            return View(viewModelList);
        }
        // GET: Nomination/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var nomination = await _nominationRepo.GetNominationByIdAsync(id);
            if (nomination == null)
            {
                return NotFound();
            }
            var viewModel = _mapper.Map<NominationViewModel>(nomination);
            return View(viewModel);
        }
        // GET: Nomination/Create
        public async Task<IActionResult> Create()
        {
            if (!User.Identity.IsAuthenticated)
                //return RedirectToAction("Identity/Account/Login");
                return RedirectToAction("/Login");

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var nominees = await _context.Users
           .Where(u => u.TeamId == currentUser.TeamId && u.Id != currentUser.Id && u.Name != null)
           .Where(n => n.IsDeleted == false || n.IsDeleted == null)
           .ToListAsync();

            ViewBag.Nominees = new SelectList(nominees, "Id", "Name");

            ViewBag.Categories = new SelectList(await _context.Categories.Where(ct => ct.IsDeleted == false && ct.isActive == true).ToListAsync(), "Id", "Name");
            var activeQuarter = _context.YearQuarters.FirstOrDefault(yq => yq.IsActive);
            Nomination nomination = new Nomination();

            if (activeQuarter != null)
            {
                nomination.YearQuarterId = activeQuarter.Id;
            }

            var viewModel = _mapper.Map<NominationViewModel>(nomination);

            ViewData["ActiveQuarterDisplay"] = activeQuarter.Quarter + " - " + activeQuarter.Year;
            ViewBag.NominatorId = currentUser.Id;
            ViewBag.Status = NominationStatus.PendingManager;
            return View(viewModel);
        }
        // POST: Nomination/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NominationViewModel viewModel)
        {
            var exists = await _context.Nominations
                   .AnyAsync(n => n.NomineeId == viewModel.NomineeId
                   && n.CategoryId == viewModel.CategoryId
                   && n.YearQuarterId == viewModel.YearQuarterId
                   && !n.IsDeleted);

           if (exists)
            {
                ModelState.AddModelError("CategoryId",GeneralMessages.DuplicateNomination);
            }
            if (ModelState.IsValid)
            {
                var nomination = _mapper.Map<Nomination>(viewModel);
                nomination.Id = Guid.NewGuid();
                nomination.CreatedAt = DateTime.UtcNow;
                await _nominationRepo.AddNominationAsync(nomination);
                TempData["message"] = ToastMessages_Nomination.CreateNomination;
                return RedirectToAction(nameof(Index));
            }
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var nominees = await _context.Users
           .Where(u => u.TeamId == currentUser.TeamId && u.Id != currentUser.Id && u.Name != null)
           .ToListAsync();

            ViewBag.Nominees = new SelectList(nominees, "Id", "Name");
            ViewBag.Categories = new SelectList(await _context.Categories.Where(ct => ct.IsDeleted == false && ct.isActive == true).ToListAsync(), "Id", "Name");
            var activeQuarter = _context.YearQuarters.FirstOrDefault(yq => yq.IsActive);

            if (activeQuarter != null)
            {
                viewModel.YearQuarterId = activeQuarter.Id;
            }
            ViewData["ActiveQuarterDisplay"] = activeQuarter.Quarter + " - " + activeQuarter.Year;
            ViewBag.NominatorId = currentUser.Id;
            ViewBag.Status = NominationStatus.PendingManager;
            return View(viewModel);
        }

        // GET: Nomination/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var nomination = await _nominationRepo.GetNominationByIdAsync(id);
            if (nomination == null)
                return NotFound();

            if (nomination.Status != NominationStatus.PendingManager)
                return Forbid(); // Disallow editing if not pending

            ViewBag.Categories = new SelectList(await _context.Categories.Where(ct => ct.IsDeleted == false && ct.isActive == true).ToListAsync(), "Id", "Name", nomination.CategoryId);
            ViewBag.YearQuarters = new SelectList(await _context.YearQuarters.ToListAsync(), "Id", "Quarter", nomination.YearQuarterId);
            var viewModel = _mapper.Map<NominationViewModel>(nomination);
            return View(viewModel);
        }
        // POST: Nomination/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, NominationViewModel viewModel)
        {
            var existing = await _nominationRepo.GetNominationByIdAsync(id);

            if (!ModelState.IsValid)
            {
                ModelState.Clear();
                ViewBag.Categories = new SelectList(await _context.Categories.Where(ct => ct.IsDeleted == false && ct.isActive == true).ToListAsync(), "Id", "Name", existing.CategoryId);
                ViewBag.YearQuarters = new SelectList(await _context.YearQuarters.ToListAsync(), "Id", "Quarter", existing.YearQuarterId);
                var existingViewModel = _mapper.Map<NominationViewModel>(existing);
                return View(existingViewModel);
            }
            existing.YearQuarterId = viewModel.YearQuarterId;
            existing.NominatorId = viewModel.NominatorId;
            existing.Achievements = viewModel.Achievements;
            existing.CategoryId = viewModel.CategoryId;
            existing.Description = viewModel.Description;
            existing.NomineeId = viewModel.NomineeId;
            existing.Status = viewModel.Status;

            await _nominationRepo.UpdateNominationAsync(existing);
            TempData["message"] = ToastMessages_Nomination.UpdateNomination;
            return RedirectToAction(nameof(Index));
        }


        // GET: Nomination/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var nomination = await _nominationRepo.GetNominationByIdAsync(id);
            if (nomination == null || nomination.Status != NominationStatus.PendingManager)
                return Forbid();

            await _nominationRepo.SoftDeleteNominationAsync(id);
            TempData["message"] = ToastMessages_Nomination.DeleteNomination;
            return RedirectToAction(nameof(Index));
        }

        // GET: Nomination/Review/{id}


        public async Task<IActionResult> Review(Guid id)
        {
            var nomination = await _nominationRepo.GetNominationByIdAsync(id);
            if (nomination == null)
                return NotFound();
            return View(nomination);
        }
        // POST: Nomination/Review/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(Guid id, string action, string remarks)
        {
            var nomination = await _nominationRepo.GetNominationByIdAsync(id);
            var currentUser = await _userManager.GetUserAsync(User);
            var userRole = await _userManager.GetRolesAsync(currentUser);
            if (nomination == null || currentUser == null)
                return NotFound();
            // Parse enum safely
            if (!Enum.TryParse<ApprovalAction>(action, out var parsedAction))
            {
                ModelState.AddModelError("", GeneralMessages.ApprovalError);
                return View(nomination); // Or redirect back with error
            }

            nomination.Status = parsedAction == ApprovalAction.Approved
                ? (userRole.Contains("Manager") ? NominationStatus.ManagerApproved : NominationStatus.DirectorApproved)
                : (userRole.Contains("Manager") ? NominationStatus.ManagerRejected : NominationStatus.DirectorRejected);

            await _nominationRepo.UpdateNominationAsync(nomination);
            var approval = new Approval
            {
                Id = Guid.NewGuid(),
                NominationId = id,
                ApproverId = currentUser.Id,
                Action = parsedAction,
                Level = (userRole.Contains("Manager") ? ApprovalLevel.Manager : ApprovalLevel.Director),
                ActionAt = DateTime.UtcNow,
                Remarks = remarks
            };
            _context.Approvals.Add(approval);
            await _context.SaveChangesAsync();
            // Fetch nominator and nominee
            var nominator = await _userManager.FindByIdAsync(nomination.NominatorId);
            var nominee = await _userManager.FindByIdAsync(nomination.NomineeId);
            if (nomination.Status == NominationStatus.DirectorApproved)
            {
                // Notify the nominator
                if (nominator != null)
                {
                    await _emailService.SendEmailAsync(
                          subject: GeneralMessages.Nomation_Approved,
                        isHtml: true,
                        body: $@"
                        <body style=""font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #ffffff;"">
                          <div style=""background-color: #ffffff; padding: 10px 20px; max-width: 600px; margin: auto; color: #000;"">
                            <img src=""cid:bannerImage"" alt=""Zelis Banner"" style=""width: 100%; max-width: 600px;"">
                            <h2 style='color: green;'>Congratulations!</h2>
                            <p>Your nomination for <strong>{nominee?.Name}</strong> has been <strong>approved by the Director</strong>.</p>
                            <p>Thank you for recognizing great work on our Rewards and Recognition platform.</p>
                            <p style='color: gray;'>Regards,<br/>Rewards & Recognition Team</p>
                            
                          </div>
                        </body>",
                        to: nominator.Email
                    );
                }

                // Notify the nominee
                if (nominee != null)
                {
                    await _emailService.SendEmailAsync(
                          subject: GeneralMessages.Selected_Award,
                        isHtml: true,
                        body: $@"<body style=""font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #ffffff;"">
                          <div style=""background-color: #ffffff; padding: 10px 20px; max-width: 600px; margin: auto; color: #000;"">
                            <img src=""cid:bannerImage"" alt=""Zelis Banner"" style=""width: 100%; max-width: 600px;"">
                            <h2 style='color: navy;'>Congratulations {nominee.Name}!</h2>
                            <p>You have been selected for an <strong>award</strong> in the category of <strong>{nomination.Category.Name}</strong> for <strong>{nomination.YearQuarter.Quarter}</strong>.</p>
                            <p>This recognition comes as part of our Rewards & Recognition initiative. Keep up the amazing work!</p>
                            <p style='color: gray;'>Cheers,<br/>Rewards & Recognition Team</p>
                          </div>
                        </body>",
                        to: nominee.Email
                    );
                }
            }

            TempData["message"] = ((nomination.Status == NominationStatus.ManagerApproved) || (nomination.Status == NominationStatus.DirectorApproved)) ? ToastMessages_Nomination.ApproveNomination : ToastMessages_Nomination.RejectNomination;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RevertBack(Guid id, string reason)
        {
            var nomination = await _nominationRepo.GetNominationByIdAsync(id);
            var currentUser = await _userManager.GetUserAsync(User);

            if (nomination == null || currentUser == null)
                return NotFound();

            // Change status
            nomination.Status = NominationStatus.PendingManager;
            await _nominationRepo.UpdateNominationAsync(nomination);

            // Delete existing approvals
            var approvals = _context.Approvals.Where(a => a.NominationId == id);
            _context.Approvals.RemoveRange(approvals);
            await _context.SaveChangesAsync();

            // Get team lead
            var teamLead = await _userManager.FindByIdAsync(nomination.Nominator.Id);
            if (teamLead != null)
            {
                await _emailService.SendEmailAsync(
                     subject: GeneralMessages.Nomination_Reverted,
                    isHtml: true,
                    body: $@"
                     <body style=""font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #ffffff;"">
                          <div style=""background-color: #ffffff; padding: 10px 20px; max-width: 600px; margin: auto; color: #000;"">
                            <img src=""cid:bannerImage"" alt=""Zelis Banner"" style=""width: 100%; max-width: 600px;"">
                            Hi {teamLead.Name},<br/><br/>
                            Your nomination <strong>for {nomination.Nominee.Name}</strong> has been reverted back for the following reason:<br/><blockquote>{reason}</blockquote>
                            <br/>Please review and resubmit if needed.<br/><br/>Regards,<br/>R&R Team
                          </div>
                        </body>",
                    to: teamLead.Email
                );
            }
            TempData["message"] = ToastMessages_Nomination.RevertNomination;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ExportAllNominationsOpenXml(Guid? yearQuarterId = null)
        {
            var user = await _userManager.GetUserAsync(User);

            IQueryable<Nomination> query = _context.Nominations
                .Include(n => n.Nominee)
                .Include(n => n.Nominator)
                .Include(n => n.Category)
                .Include(n => n.YearQuarter);

            // Filter by role (Manager)

            if (User.IsInRole(Roles.Manager.ToString()))

            {
                query = query.Where(n => n.Nominee.Team.ManagerId == user.Id);
            }
            else if (User.IsInRole(Roles.Director.ToString()))
            {
                query = query.Where(n =>
                    (n.Nominator.Team.DirectorId == user.Id || n.Nominee.Team.DirectorId == user.Id) &&
                    (n.Status == NominationStatus.DirectorApproved ||
                     n.Status == NominationStatus.DirectorRejected ||
                     n.Status == NominationStatus.ManagerApproved ||
                     n.Status == NominationStatus.ManagerRejected));
            }

            else if (User.IsInRole(Roles.TeamLead.ToString()))
            {
                // TeamLead sees nominations for teams they lead
                query = query.Where(n => n.Nominee.Team.TeamLeadId == user.Id);
            }
            // Filter by YearQuarter
            if (yearQuarterId.HasValue)
            {
                query = query.Where(n => n.YearQuarter.Id == yearQuarterId.Value);
            }

            var nominations = await query.ToListAsync();
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

                // Headers
                var headers = new[]{"Nominee Name", "Nominator Name", "Category", "Description","Achievements", "Status", "Quarter", "Created At"};
                var headerRow = new Row();
                foreach (var header in headers)
                    headerRow.Append(SheetClassesStyles.CreateStyledCell(header, 2));
                sheetData.Append(headerRow);
                // Data rows
                foreach (var nomination in nominations)
                {
                    var row = new Row();
                    row.Append(SheetClassesStyles.CreateStyledCell(nomination.Nominee?.Name ?? "N/A", 1));
                    row.Append(SheetClassesStyles.CreateStyledCell(nomination.Nominator?.Name ?? "N/A", 1));
                    row.Append(SheetClassesStyles.CreateStyledCell(nomination.Category?.Name ?? "N/A", 1));
                    row.Append(SheetClassesStyles.CreateStyledCell(nomination.Description, 1));
                    row.Append(SheetClassesStyles.CreateStyledCell(nomination.Achievements, 1));
                    row.Append(SheetClassesStyles.CreateStyledCell(nomination.Status.ToString(), 1));
                    row.Append(SheetClassesStyles.CreateStyledCell(nomination.YearQuarter?.Quarter.ToString() ?? "N/A", 1));
                    row.Append(SheetClassesStyles.CreateStyledCell(nomination.CreatedAt.ToString("dd-MM-yyyy"), 1));
                    sheetData.Append(row);
                }
                // Column width
                var columns = new Columns(
                    new Column { Min = 1, Max = 8, Width = 25, CustomWidth = true }
                );
                worksheetPart.Worksheet.InsertAt(columns, 0);
                // Sheets
                Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                Sheet sheet = new Sheet()
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Nominations"
                };
                sheets.Append(sheet);

                workbookPart.Workbook.Save();
            }
            memStream.Seek(0, SeekOrigin.Begin);
            return File(memStream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Nominations.xlsx");
        }

        [HttpGet]
        [Route("Nomination/ExportTeamNominationsToExcel/{teamId}")]
        public async Task<IActionResult> ExportTeamNominationsToExcel(Guid? teamId = null, int? year = null, Guid? quarterId = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            // Get team name
            string teamName = await _context.Teams
                .Where(t => t.Id == teamId)
                .Select(t => t.Name)
                .FirstOrDefaultAsync();

            // Base query
            var query = _context.Nominations
                .Include(n => n.Nominee)
                .Include(n => n.Nominator).ThenInclude(n => n.Team)
                .Include(n => n.Category)
                .Include(n => n.YearQuarter)
                .AsQueryable();
            // Role-based filtering          
            if (userRoles.Contains(nameof(Roles.Manager)))
            {
                query = query.Where(n =>
                    n.Nominator.Team.Manager.Id == currentUser.Id &&
                    n.Nominator.TeamId == teamId);
            }
            else if (userRoles.Contains(nameof(Roles.Director)))
            {
                query = query.Where(n =>
                    n.Nominator.Team.Director.Id == currentUser.Id &&
                    n.Nominator.TeamId == teamId);
            }
            // Year + Quarter filtering
            if (year.HasValue)
                query = query.Where(n => n.YearQuarter.Year == year.Value);

            if (quarterId.HasValue)
                query = query.Where(n => n.YearQuarter.Id == quarterId.Value);
            var nominations = await query.ToListAsync();
            // Generate Excel
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

                // Header row
                var headers = new[]{"Nominee Name","Nominated By", "Category", "Description","Achievements", "Status", "Created At"};
                var headerRow = new Row();
                foreach (var header in headers)
                    headerRow.Append(SheetClassesStyles.CreateStyledCell(header, 2));
                sheetData.Append(headerRow);
                // Data rows
                foreach (var nomination in nominations)
                {
                    var row = new Row();
                    row.Append(SheetClassesStyles.CreateStyledCell(nomination.Nominee?.Name ?? "N/A", 1));
                    row.Append(SheetClassesStyles.CreateStyledCell(nomination.Nominator?.Name ?? "N/A", 1));
                    row.Append(SheetClassesStyles.CreateStyledCell(nomination.Category?.Name ?? "N/A", 1));
                    row.Append(SheetClassesStyles.CreateStyledCell(nomination.Description, 1));
                    row.Append(SheetClassesStyles.CreateStyledCell(nomination.Achievements, 1));
                    row.Append(SheetClassesStyles.CreateStyledCell(nomination.Status.ToString(), 1));
                    row.Append(SheetClassesStyles.CreateStyledCell(nomination.CreatedAt.ToString("dd-MM-yyyy"), 1));
                    sheetData.Append(row);
                }
                // Column width
                var columns = new Columns(
                    new Column { Min = 1, Max = 8, Width = 25, CustomWidth = true }
                );
                worksheetPart.Worksheet.InsertAt(columns, 0);
                // Sheets
                Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                Sheet sheet = new Sheet()
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Nominations"
                };
                sheets.Append(sheet);
                workbookPart.Workbook.Save();
            }
            memStream.Seek(0, SeekOrigin.Begin);
            return File(memStream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{teamName}_Nominations.xlsx");
        }

        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            // Generate Excel
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

                // Header row
                string[] headers = { "NomineeEmail", "CategoryName", "YearQuarterName", "Description", "Achievements" };

                var headerRow = new Row();
                foreach (var header in headers)
                    headerRow.Append(SheetClassesStyles.CreateStyledCell(header, 2));

                sheetData.Append(headerRow);

                string[] data = { "nominee@zelis.com", "star of the quarter", "Q3,2025", "Good job", "Excellent Job" };

                // Data rows
                var row = new Row();
                foreach (var d in data)
                {

                    row.Append(SheetClassesStyles.CreateStyledCell(d, 1));


                }
                sheetData.Append(row);

                // Column width
                var columns = new Columns(
                    new Column { Min = 1, Max = 8, Width = 25, CustomWidth = true }
                );
                worksheetPart.Worksheet.InsertAt(columns, 0);

                // Sheets
                Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                Sheet sheet = new Sheet()
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Template"
                };
                sheets.Append(sheet);

                workbookPart.Workbook.Save();
            }
            memStream.Seek(0, SeekOrigin.Begin);
            return File(memStream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"NominationsTemplate.xlsx");
        }


        [HttpGet]
        public IActionResult UploadNomination()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadNomination(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = Excel_Messages.ValidExcel;
                return View("UploadNomination");
            }

            var previewList = new List<NominationPreviewViewModel>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using var document = SpreadsheetDocument.Open(stream, false);
            var workbookPart = document.WorkbookPart;
            var sheet = workbookPart.Workbook.Sheets.Elements<Sheet>().First();
            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
            var rows = worksheetPart.Worksheet.Descendants<Row>().Skip(1); // Skip header

            foreach (var row in rows)
            {
                var cells = row.Elements<Cell>().ToList();
                var nomineeEmail = GetCellValue(workbookPart, cells[0]);
                var categoryName = GetCellValue(workbookPart, cells[1]);
                var yearQuarterName = GetCellValue(workbookPart, cells[2]);
                var description = GetCellValue(workbookPart, cells[3]);
                var achievements = GetCellValue(workbookPart, cells[4]);

                previewList.Add(new NominationPreviewViewModel
                {
                    NomineeEmail = nomineeEmail,
                    CategoryName = categoryName,
                    YearQuarterName = yearQuarterName,
                    Description = description,
                    Achievements = achievements
                });
            }

            TempData["PreviewData"] = System.Text.Json.JsonSerializer.Serialize(previewList);
            return View("UploadNomination", previewList);  // show preview grid
        }

        private string GetCellValue(WorkbookPart wbPart, Cell cell)
        {
            string value = cell.InnerText;
            if (cell.DataType == null) return value;

            if (cell.DataType == CellValues.SharedString)
            {
                return wbPart.SharedStringTablePart.SharedStringTable
                    .Elements<SharedStringItem>().ElementAt(int.Parse(value)).InnerText;
            }
            return value;
        }

        [HttpPost]
        public async Task<IActionResult> SaveNominations()
        {
            if (!TempData.ContainsKey("PreviewData"))
                return RedirectToAction("UploadNomination");

            var previewJson = TempData["PreviewData"]?.ToString();
            var previewList = System.Text.Json.JsonSerializer.Deserialize<List<NominationPreviewViewModel>>(previewJson);

            var nominations = new List<Nomination>();
            var nominator = await _userManager.GetUserAsync(User);

            foreach (var item in previewList)
            {
                var nominee = await _context.Users.Include(x => x.Team)
                    .FirstOrDefaultAsync(x => (x.Email == item.NomineeEmail) && (x.Team.TeamLeadId == nominator.Id));
                var category = await _context.Categories.FirstOrDefaultAsync(x => x.Name == item.CategoryName);

                var yq = item.YearQuarterName.Split(',');
                if (yq.Length != 2) throw new Exception(GeneralMessages.InvalidQuarterError);

                var quarterString = yq[0];
                var year = int.Parse(yq[1]);

                if (!Enum.TryParse<Quarter>(quarterString, true, out var quarterEnum))
                    throw new Exception($"Invalid Quarter '{quarterString}'");
                var yearQuarter = await _context.YearQuarters.FirstOrDefaultAsync(x => x.Year == year && x.Quarter == quarterEnum);
                if (nominee == null || category == null || yearQuarter == null)
                {
                    throw new Exception(GeneralMessages.InvalidExcelData);
                }

                var ns = await _nominationRepo.GetAllNominationsAsync();
                bool exists = ns.Any(n => n.NomineeId == nominee.Id && n.CategoryId == category.Id);

                if (exists)
                    throw new Exception(GeneralMessages.ExcelDuplicateData);

                nominations.Add(new Nomination
                {
                    Id = Guid.NewGuid(),
                    NominatorId = nominator.Id,
                    NomineeId = nominee.Id,
                    CategoryId = category.Id,
                    YearQuarterId = yearQuarter.Id,
                    Description = item.Description,
                    Achievements = item.Achievements,
                    Status = NominationStatus.PendingManager,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Nominations.AddRange(nominations);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Message"] = Excel_Messages.SaveExcelData;
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public IActionResult CancelUpload()
        {
            TempData.Remove("PreviewData");
            return RedirectToAction("UploadNomination");
        }

    }
}
