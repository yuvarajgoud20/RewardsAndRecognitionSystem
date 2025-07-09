﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RewardsAndRecognitionRepository;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionRepository.Repos;

namespace RewardsAndRecognitionSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IUserRepo _userRepo;
        private readonly UserManager<User> _userManager;
        private readonly ITeamRepo _teamRepo;
        private readonly ApplicationDbContext _context;

        public UserController(IUserRepo userRepo, UserManager<User> userManager, ITeamRepo teamRepo, ApplicationDbContext context)
        {
            _userRepo = userRepo;
            _userManager = userManager;
            _teamRepo = teamRepo;
            _context = context;
        }

        // GET: User
        public async Task<IActionResult> Index()
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

            

            ViewBag.UserRoles = userRoles;

            return View(users);
        }

        // GET: User/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropDowns();
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string password, string SelectedRole)
        {
            if (ModelState.IsValid)
            {
                user.UserName = user.Email;
                user.NormalizedEmail = user.Email.ToUpper();
                user.EmailConfirmed = true;
                user.CreatedAt = DateTime.UtcNow;

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    // Add user to selected role or default to Employee
                    var roleToAssign = string.IsNullOrEmpty(SelectedRole) ? "Employee" : SelectedRole;
                    //if (roleToAssign == "Employee" && user.Team == null)
                    //    throw new RnRException("We cannot Create Employee without Team");
                    //if (roleToAssign == "Manager" && user.Team != null)
                    //    throw new RnRException($"A {roleToAssign} should not assigned a team");
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

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await PopulateDropDowns();
            return View(user);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, User updatedUser)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

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
                    return RedirectToAction(nameof(Index));

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }

            await PopulateDropDowns();
            return View(updatedUser);
        }

        // GET: User/Delete/5
        //public async Task<IActionResult> Delete(string id)
        //{
        //    var user = await _userRepo.GetByIdAsync(id);
        //    if (user == null) return NotFound();

        //    return View(user);
        //}

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            //    var user = await _userManager.FindByIdAsync(id);
            //    if (user == null) return NotFound();

            //    var result = await _userManager.DeleteAsync(user);
            //    if (result.Succeeded)
            //        return RedirectToAction(nameof(Index));

            //    ModelState.AddModelError("", "Error deleting user.");
            //    return View(user);
            /* var user = await _userManager.FindByIdAsync(id);
             await _userManager.DeleteAsync(user);
            return RedirectToAction(nameof(Index));*/
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
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
