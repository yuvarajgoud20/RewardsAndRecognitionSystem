using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using RewardsAndRecognitionRepository.Enums;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionSystem.ViewModels;

namespace RewardsAndRecognitionSystem.Controllers
{
    [Authorize(Roles = nameof(Roles.Admin))]
    public class UserTeamController : Controller
    {
        private readonly IUserRepo _userRepo;
        private readonly ITeamRepo _teamRepo;
        private readonly UserManager<User> _userManager;

        public UserTeamController(IUserRepo userRepo, ITeamRepo teamRepo, UserManager<User> userManager)
        {
            _userRepo = userRepo;
            _teamRepo = teamRepo;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Map()
        {
            var allUsers = await _userRepo.GetUnassignedUsersAsync();

            // filter out Admin, Manager, Director
            var filteredUsers = new List<User>();
            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains(Roles.Admin.ToString()) &&
                    !roles.Contains(Roles.TeamLead.ToString()) &&
                    !roles.Contains(Roles.Manager.ToString()) &&
                    !roles.Contains(Roles.Director.ToString()))
                {
                    filteredUsers.Add(user);
                }
            }

            var vm = new UserTeamMappingViewModel
            {
                Teams = (await _teamRepo.GetAllAsync())
                            .Where(t => t.IsDeleted == false)
                            .OrderBy(t => t.Name)
                            .ToList(),
                Users = filteredUsers
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(Guid selectedTeamId, List<string> selectedUserIds)
        {
            if (selectedTeamId == Guid.Empty || selectedUserIds == null || selectedUserIds.Count == 0)
            {
                TempData["message"] = "Please select a team and at least one user.";
                return RedirectToAction(nameof(Map));
            }

            foreach (var userId in selectedUserIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    user.TeamId = selectedTeamId;   // reassigns if they had a team (rare for this page)
                    await _userManager.UpdateAsync(user);
                }
            }

            TempData["message"] = "Users assigned to team successfully.";
            return RedirectToAction(nameof(Map));
        }
    }
}
