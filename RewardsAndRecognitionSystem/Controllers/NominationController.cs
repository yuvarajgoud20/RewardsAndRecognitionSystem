using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RewardsAndRecognitionRepository.Enums;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionRepository.Repos;
using RewardsAndRecognitionSystem.ViewModels;
using RewardsAndRecognitionRepository.Service;

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
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });


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
                    .Where(n => n.Nominee.Team.DirectorId == currentUser.Id)
                    .Where(n => n.Status != NominationStatus.PendingManager)
                    //.Where(n => n.Approvals != null)
                    .ToListAsync();

                // Track reviewed nominations
                var alreadyReviewedIds = nominationsToShow
                    .Where(n => n.Approvals.Any(a => a.ApproverId == currentUser.Id))
                    .Select(n => n.Id)
                    .ToList();

                ViewBag.ReviewedNominationIds = alreadyReviewedIds;
                var directorList = _mapper.Map<List<NominationViewModel>>(nominationsToShow);
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
                    .Where(n => n.Nominee.Team.ManagerId == currentUser.Id)
                    .ToListAsync();

                // Track reviewed nominations
                var alreadyReviewedIds = nominationsToShow
                    .Where(n => n.Approvals.Any(a => a.ApproverId == currentUser.Id))
                    .Select(n => n.Id)
                    .ToList();

                ViewBag.ReviewedNominationIds = alreadyReviewedIds;
                var managerList = _mapper.Map<List<NominationViewModel>>(nominationsToShow);
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
                     .Where(n => n.NominatorId == currentUser.Id)
                     .ToListAsync();
                                var alreadyReviewedIds = nominationsToShow
                                    .Where(n => n.Approvals.Any(a => a.ApproverId == currentUser.Team.DirectorId))
                                    .Select(n => n.Id)
                                    .ToList();

                ViewBag.ReviewedNominationIds = alreadyReviewedIds;

                var teamList = _mapper.Map<List<NominationViewModel>>(nominationsToShow);
                return View(teamList);
            }

            if (userRoles.Contains("Admin"))
            {
                nominationsToShow = await _context.Nominations
                    .Include(n => n.Nominee)
                    .Include(n => n.Nominator)
                    .ThenInclude(u => u.Team)
                    .Include(n => n.Category)
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

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            var activeQuarter = _context.YearQuarters.FirstOrDefault(yq => yq.IsActive);
            Nomination nomination = new Nomination();
            var viewModel = _mapper.Map<NominationViewModel>(nomination);
            if (activeQuarter != null)
            {
                nomination.YearQuarterId= activeQuarter.Id;
            }
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
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
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

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", nomination.CategoryId);
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
                ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", existing.CategoryId);
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

            await _nominationRepo.DeleteNominationAsync(id);
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
    }
}