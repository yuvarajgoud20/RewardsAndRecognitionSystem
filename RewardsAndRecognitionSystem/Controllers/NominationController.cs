using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RewardsAndRecognitionRepository.Enums;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionRepository.Repos;

namespace RewardsAndRecognitionSystem.Controllers
{
    [Authorize(Roles = "Manager,TeamLead,Admin,Director")]
    public class NominationController : Controller
    {
        private readonly INominationRepo _nominationRepo;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public NominationController(INominationRepo nominationRepo, ApplicationDbContext context, UserManager<User> userManager)
        {
            _nominationRepo = nominationRepo;
            _context = context;
            _userManager = userManager;
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

                return View(nominationsToShow);
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

                return View(nominationsToShow);
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

                return View(nominationsToShow);
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
            return View(nominationsToShow);
        }


        // GET: Nomination/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var nomination = await _nominationRepo.GetNominationByIdAsync(id);
            if (nomination == null)
            {
                return NotFound();
            }
            return View(nomination);
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
            //ViewBag.NominatorId = currentUser.Id;
            //ViewBag.NominatorName = currentUser.Name;
            Nomination nomination = new Nomination();
            if (activeQuarter != null)
            {
                nomination.YearQuarterId= activeQuarter.Id;
            }
            ViewData["ActiveQuarterDisplay"] = activeQuarter.Quarter + " - " + activeQuarter.Year;
            ViewBag.NominatorId = currentUser.Id;
            ViewBag.Status = NominationStatus.PendingManager;
            return View(nomination);
        }

        // POST: Nomination/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Nomination nomination)
        {

            if (ModelState.IsValid)
            {
                nomination.Id = Guid.NewGuid();
                nomination.CreatedAt = DateTime.UtcNow;
                await _nominationRepo.AddNominationAsync(nomination);
                return RedirectToAction(nameof(Index));
            }
            return View(nomination);
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

            return View(nomination);
        }


        // POST: Nomination/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Nomination nomination)
        {
            await _nominationRepo.UpdateNominationAsync(nomination);
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

            return RedirectToAction(nameof(Index));
        }
    }
}