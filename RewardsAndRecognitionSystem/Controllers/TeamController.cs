using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;

namespace RewardsAndRecognitionSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TeamController : Controller
    {
        private readonly ITeamRepo _teamRepo;
        private readonly IUserRepo _userRepo;
        private readonly UserManager<User> _userManager;

        public TeamController(ITeamRepo teamRepo, IUserRepo userRepo,UserManager<User> userManager)
        {
            _teamRepo = teamRepo;
            _userRepo = userRepo;
            _userManager = userManager;
        }

        // GET: TeamController
        public async Task<IActionResult> Index()
        {
            var teams = await _teamRepo.GetAllAsync();
            return View(teams);
        }

        // GET: TeamController/Create
        public async Task<IActionResult> Create()
        {
            //var managers = await _userRepo.GetAllManagersAsync();
            //var leads = await _userRepo.GetLeadsAsync(); // ⬅️ Only unassigned team leads

            //ViewBag.Managers = new SelectList(managers, "Id", "Name");
            //ViewBag.TeamLeads = new SelectList(leads, "Id", "Name");

            await LoadDropdownsAsync();

            return View();
        }


        // POST: TeamController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Team team)
        {
            if (ModelState.IsValid)
            {
                await _teamRepo.AddAsync(team);
                User user = await _userRepo.GetByIdAsync(team.TeamLeadId);
                user.TeamId = team.Id;
                user.Team=team;
                await _userManager.UpdateAsync(user);
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownsAsync();
            return View(team);
        }

        // GET: TeamController/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var team = await _teamRepo.GetByIdAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            var managers = await _userRepo.GetAllManagersAsync(); // ✅ Required

            var leads = await _userRepo.GetLeadsAsync(team.TeamLeadId);          // ✅ Required
            var directors = await _userRepo.GetAllDirectorsAsync();

            ViewBag.Managers = new SelectList(managers, "Id", "Name", team.ManagerId);
            ViewBag.TeamLeads = new SelectList(leads, "Id", "Name", team.TeamLeadId);
            ViewBag.Directors = new SelectList(directors, "Id", "Name", team.DirectorId);

            return View(team);
        }


        // POST: TeamController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Team team)
        {
            if (id != team.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                await _teamRepo.UpdateAsync(team);
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownsAsync();
            return View(team);
        }

        // GET: TeamController/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var team = await _teamRepo.GetByIdAsync(id);
            if (team == null)
                return NotFound();

            return View(team);
        }

        // GET: TeamController/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var team = await _teamRepo.GetByIdAsync(id);
            if (team == null)
                return NotFound();

            return View(team);
        }

        // POST: TeamController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var team = await _teamRepo.GetByIdAsync(id);
            if (team == null)
                return NotFound();

            await _teamRepo.DeleteAsync(team);
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdownsAsync()
        {
            var managers = await _userRepo.GetAllManagersAsync();
            var leads = await _userRepo.GetLeadsAsync();
            var directors = await _userRepo.GetAllDirectorsAsync();

            ViewBag.Managers = new SelectList(managers, "Id", "Name");
            ViewBag.TeamLeads = new SelectList(leads, "Id", "Name");
            ViewBag.Directors = new SelectList(directors, "Id", "Name");
        }
    }
}