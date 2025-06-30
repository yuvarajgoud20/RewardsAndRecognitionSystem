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
    [Authorize]
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
                return Unauthorized();

            var nominations = await _nominationRepo
                .GetAllNominationsAsync();

            // Filter nominations created by current user
            var myNominations = nominations
                .Where(n => n.NominatorId == currentUser.Id)
                .ToList();

            return View(myNominations);
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
                return RedirectToAction("Login", "Account");

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var nominees = await _context.Users
           .Where(u => u.TeamId == currentUser.TeamId && u.Id != currentUser.Id && u.Name != null)
           .ToListAsync();

            ViewBag.Nominees = new SelectList(nominees, "Id", "Name");


            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            ViewBag.YearQuarters = new SelectList(await _context.YearQuarters.ToListAsync(), "Id", "Quarter");
            //ViewBag.NominatorId = currentUser.Id;
            //ViewBag.NominatorName = currentUser.Name;
            Nomination nomination = new Nomination();
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
            {
                return NotFound();
            }
            return View(nomination);
        }

        // POST: Nomination/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Nomination nomination)
        {
            if (id != nomination.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _nominationRepo.UpdateNominationAsync(nomination);
                return RedirectToAction(nameof(Index));
            }
            return View(nomination);
        }

        // GET: Nomination/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var nomination = await _nominationRepo.GetNominationByIdAsync(id);
            if (nomination == null)
            {
                return NotFound();
            }
            return View(nomination);
        }

        // POST: Nomination/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _nominationRepo.DeleteNominationAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
