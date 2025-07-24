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
using RewardsAndRecognitionRepository.Enums;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionRepository.Repos;
using RewardsAndRecognitionRepository.Service;
using RewardsAndRecognitionSystem.Utilities;
using RewardsAndRecognitionSystem.ViewModels;

namespace RewardsAndRecognitionSystem.Controllers
{
    [Authorize(Roles = "Manager,TeamLead,Admin,Director")]
    public class NominationController : Controller
    {
        private readonly IMapper _mapper;
        private readonly INominationRepo _nominationRepo;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;

        public NominationController(
            IMapper mapper,
            INominationRepo nominationRepo, 
            ApplicationDbContext context,
            UserManager<User> userManager,
            IEmailService emailService)

        {
            _nominationRepo = nominationRepo;
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _emailService = emailService;
        }

        // GET: Nomination
        public async Task<IActionResult> Index(string filter = "all" ,string FilterForDelete = "active", int page = 1)
        {
            int pageSize = 10;
            ViewBag.filter = filter;
            
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            List<Nomination> allNominations = new();
            List<Nomination> deletednominations = new();

            var activeQuarter = await _context.YearQuarters.FirstOrDefaultAsync(yq => yq.IsActive);
            if (activeQuarter == null)
            {
                TempData["Message"] = "❌ No active quarter is set.";
                return View(new List<Nomination>());
            }

            ViewBag.currentUser = currentUser;
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            List<Nomination> nominationsToShow = new();

            if (userRoles.Contains("Director"))
            {
                nominationsToShow = await _context.Nominations
                    .Include(n => n.Nominee)
                        .ThenInclude(u => u.Team)
                    .Include(n => n.Category)
                    .Include(n => n.Approvals)
                    .Include(n => n.Nominator)
                          .Where(n => n.YearQuarterId == activeQuarter.Id)
                    .Where(n => n.Nominee.Team.DirectorId == currentUser.Id)
                    .Where(n => n.Status != NominationStatus.PendingManager)
                    //.Where(n => n.Approvals != null)
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

            if (userRoles.Contains("Manager"))
            {
                nominationsToShow = await _context.Nominations
                    .Include(n => n.Nominee)
                        .ThenInclude(u => u.Team)
                    .Include(n => n.Category)
                    .Include(n => n.Approvals)
                    .Include(n => n.Nominator)
                     .Where(n => n.YearQuarterId == activeQuarter.Id)
                    .Where(n => n.Nominee.Team.ManagerId == currentUser.Id)
                    .ToListAsync();
                if (filter == "pending")
                {
                    nominationsToShow = nominationsToShow.Where(n => n.Status == NominationStatus.PendingManager ).ToList();
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
            if (userRoles.Contains("TeamLead"))
            {
                // For TeamLead or others — show their own nominations
                nominationsToShow = await _context.Nominations
                     .Include(n => n.Nominee)
                     .ThenInclude(u => u.Team)
                     .Include(n => n.Category)
                     .Include(n => n.Approvals)
                     .Include(n => n.Nominator)
                      .Where(n => n.YearQuarterId == activeQuarter.Id)
                     .Where(n => n.NominatorId == currentUser.Id)
                     .ToListAsync();
                if (filter == "pending")
                {
                    nominationsToShow = nominationsToShow.Where(n => n.Status == NominationStatus.PendingManager ||
                                                 n.Status == NominationStatus.ManagerRejected ||
                                                 n.Status == NominationStatus.ManagerApproved).ToList();
                }
                if (filter == "directorapproved")
                {
                    nominationsToShow = nominationsToShow.Where(n => n.Status == NominationStatus.DirectorApproved).ToList();
                }
                if (filter == "directorrejected")
                {
                    nominationsToShow = nominationsToShow.Where(n => n.Status == NominationStatus.DirectorRejected).ToList();
                }
                if (FilterForDelete == "deleted")
                {
                    allNominations =  nominationsToShow.Where(n => n.IsDeleted).ToList();
                }
                else
                {
                    allNominations =  nominationsToShow.Where(n => !n.IsDeleted).ToList();
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

            if (userRoles.Contains("Admin"))
            {
                nominationsToShow = await _context.Nominations
                    .Include(n => n.Nominee)
                    .Include(n => n.Nominator)
                    .ThenInclude(u => u.Team)
                    .Include(n => n.Category)
                     .Where(n => n.YearQuarterId == activeQuarter.Id)
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
            var viewModel=_mapper.Map<NominationViewModel>(nomination);
            return View(viewModel);
        }

        // GET: Nomination/Create
        public async Task<IActionResult> Create()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Identity/Account/Login");

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var nominees = await _context.Users
           .Where(u => u.TeamId == currentUser.TeamId && u.Id != currentUser.Id && u.Name != null)
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

            if (ModelState.IsValid)
            {
                var nomination = _mapper.Map<Nomination>(viewModel);
                nomination.Id = Guid.NewGuid();
                nomination.CreatedAt = DateTime.UtcNow;
                await _nominationRepo.AddNominationAsync(nomination);
                return RedirectToAction(nameof(Index));
            }
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var nominees = await _context.Users
           .Where(u => u.TeamId == currentUser.TeamId && u.Id != currentUser.Id && u.Name != null)
           .ToListAsync();

            ViewBag.Nominees = new SelectList(nominees, "Id", "Name");
            ViewBag.Categories = new SelectList(await _context.Categories.Where(ct => ct.IsDeleted==false && ct.isActive == true).ToListAsync(), "Id", "Name");
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
            var viewModel=_mapper.Map<NominationViewModel>(nomination); 
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
                var existingViewModel= _mapper.Map<NominationViewModel>(existing);
                return View(existingViewModel);
            }
            existing.YearQuarterId = viewModel.YearQuarterId;
            existing.NominatorId=viewModel.NominatorId;
           
            existing.Achievements=viewModel.Achievements;
            existing.CategoryId=viewModel.CategoryId;
            existing.Description=viewModel.Description;
            existing.NomineeId=viewModel.NomineeId;
            existing.Status=viewModel.Status;
           
            await _nominationRepo.UpdateNominationAsync(existing);
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
                ModelState.AddModelError("", "Invalid approval action.");
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
                        subject: "🎉 Your Nomination is Approved!",
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
                        subject: "🎖️ You Have Been Selected for an Award!",
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
                    subject: "Nomination Reverted",
                    isHtml:true,
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

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> ExportAllNominationsOpenXml()

        {

            var nominations = await _context.Nominations

                .Include(n => n.Nominee)

                .Include(n => n.Nominator)

                .Include(n => n.Category)

                .Include(n => n.YearQuarter)

                .ToListAsync();

            using var memStream = new MemoryStream();

            using (SpreadsheetDocument document = SpreadsheetDocument.Create(memStream, SpreadsheetDocumentType.Workbook))

            {

                // Workbook

                var workbookPart = document.AddWorkbookPart();

                workbookPart.Workbook = new Workbook();

                // Worksheet

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();

                var sheetData = new SheetData();

                worksheetPart.Worksheet = new Worksheet(sheetData);

                // Styles

                var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();

                stylesPart.Stylesheet = SheetClassesStyles.CreateStylesheet();

                stylesPart.Stylesheet.Save();

                // Headers for Nomination export

                var headers = new[]

                {

     "Nominee Name", "Nominator Name", "Category", "Description",

     "Achievements", "Status", "Quarter", "Created At"

};

                var headerRow = new Row();

                foreach (var header in headers)

                {

                    headerRow.Append(SheetClassesStyles.CreateStyledCell(header, 2)); // Header style

                }

                sheetData.Append(headerRow);

                // Data Rows

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
        [Route("Nomination/ ExportTeamNominationsToExcel/{teamId}")]
        public async Task<IActionResult> ExportTeamNominationsToExcel(Guid? teamId = null)

        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            var nominations = new List<Nomination>();
            string teamName = await _context.Teams
              .Where(t => t.Id == teamId)
              .Select(t => t.Name)
              .FirstOrDefaultAsync();
            if (userRoles.Contains("Manager"))
            {
                nominations = await _context.Nominations

                 .Include(n => n.Nominee)

                 .Include(n => n.Nominator)
                 .ThenInclude(n => n.Team)

                 .Include(n => n.Category)

                 .Include(n => n.YearQuarter)
                 .Where(n => n.Nominator.Team.Manager.Id == currentUser.Id && n.Nominator.TeamId == teamId)
                 .ToListAsync();

            }
            if (userRoles.Contains("Director"))
            {
                nominations = await _context.Nominations

                  .Include(n => n.Nominee)

                  .Include(n => n.Nominator)
                  .ThenInclude(n => n.Team)

                  .Include(n => n.Category)

                  .Include(n => n.YearQuarter)
                  .Where(n => n.Nominator.Team.Director.Id == currentUser.Id && n.Nominator.TeamId == teamId)
                  .ToListAsync();
                nominations = nominations.Where(n => n.Status != NominationStatus.PendingManager).ToList();

            }


            using var memStream = new MemoryStream();

            using (SpreadsheetDocument document = SpreadsheetDocument.Create(memStream, SpreadsheetDocumentType.Workbook))

            {

                // Workbook

                var workbookPart = document.AddWorkbookPart();

                workbookPart.Workbook = new Workbook();

                // Worksheet

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();

                var sheetData = new SheetData();

                worksheetPart.Worksheet = new Worksheet(sheetData);

                // Styles

                var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();

                stylesPart.Stylesheet = SheetClassesStyles.CreateStylesheet();

                stylesPart.Stylesheet.Save();

                // Headers for Nomination export

                var headers = new[]

                {

     "Nominee Name","Nominated By", "Category", "Description",

     "Achievements", "Status", "Created At"

};

                var headerRow = new Row();

                foreach (var header in headers)

                {

                    headerRow.Append(SheetClassesStyles.CreateStyledCell(header, 2)); // Header style

                }

                sheetData.Append(headerRow);

                // Data Rows

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

                 $"{teamName}Nominations.xlsx");

        }

    }
}