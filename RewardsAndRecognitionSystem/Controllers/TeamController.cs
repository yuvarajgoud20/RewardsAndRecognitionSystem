﻿using System.IO;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RewardsAndRecognitionRepository.Enums;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionSystem.Common;
using RewardsAndRecognitionSystem.ViewModels;

namespace RewardsAndRecognitionSystem.Controllers
{
    [Authorize(Roles = nameof(Roles.Admin))]
    public class TeamController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ITeamRepo _teamRepo;
        private readonly ApplicationDbContext _context;
        private readonly IUserRepo _userRepo;
        private readonly UserManager<User> _userManager;

        public TeamController(IMapper mapper, ITeamRepo teamRepo, IUserRepo userRepo, UserManager<User> userManager, ApplicationDbContext context)

        {
            _mapper = mapper;
            _teamRepo = teamRepo;
            _userRepo = userRepo;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int page = 1, bool showDeleted = false)
        {
            int pageSize = 25;

            var teamsQuery = (await _teamRepo.GetAllAsync(showDeleted))
                             .OrderBy(t => t.Name)
                              .ToList();
            var totalRecords = teamsQuery.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var pagedTeams = teamsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var userRoles = new Dictionary<string, string>();
            foreach (var team in pagedTeams)
            {
                foreach (var user in team.Users ?? new List<User>())
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userRoles[user.Id] = roles.FirstOrDefault() ?? GeneralMessages.NotAvailable_Error;
                }
            }

            var grouped = pagedTeams.Select(team => new GroupedTeam
            {
                Team = team,
                Users = team.Users?.ToList() ?? new List<User>()
            }).ToList();

            ViewBag.UserRoles = userRoles;
            ViewBag.ShowDeleted = showDeleted;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.ActionName = nameof(Index); // for pagination

            return View(grouped);
        }


        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeamViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var team = _mapper.Map<Team>(viewModel);
                await _teamRepo.AddAsync(team);
                User user = await _userRepo.GetByIdAsync(team.TeamLeadId);
                user.TeamId = team.Id;
                await _userManager.UpdateAsync(user);
                TempData["message"] = "Successfully created Team";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownsAsync();
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var existingteam = await _teamRepo.GetByIdAsync(id);
            var team = _mapper.Map<TeamViewModel>(existingteam);

            if (team == null)
            {
                return NotFound();
            }

            var managers = await _userRepo.GetAllManagersAsync();
            var leads = await _userRepo.GetLeadsAsync(team.TeamLeadId);
            var directors = await _userRepo.GetAllDirectorsAsync();

            ViewBag.Managers = new SelectList(managers, "Id", "Name", team.ManagerId);
            ViewBag.TeamLeads = new SelectList(leads, "Id", "Name", team.TeamLeadId);
            ViewBag.Directors = new SelectList(directors, "Id", "Name", team.DirectorId);

            return View(team);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, TeamViewModel updatedTeam)
        {
            if (id != updatedTeam.Id)
                return BadRequest();

            var existingTeam = await _teamRepo.GetByIdAsync(id);
            if (existingTeam == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                ModelState.Clear();
                var teamViewModel = _mapper.Map<TeamViewModel>(existingTeam);
                var managers = await _userRepo.GetAllManagersAsync();

                var leads = await _userRepo.GetLeadsAsync(existingTeam.TeamLeadId);
                var directors = await _userRepo.GetAllDirectorsAsync();
                ViewBag.Managers = new SelectList(managers, "Id", "Name", existingTeam.ManagerId);
                ViewBag.TeamLeads = new SelectList(leads, "Id", "Name", existingTeam.TeamLeadId);
                ViewBag.Directors = new SelectList(directors, "Id", "Name", existingTeam.DirectorId);
                return View(teamViewModel);
            }

            if (existingTeam.TeamLeadId != updatedTeam.TeamLeadId)
            {
                if (!string.IsNullOrEmpty(existingTeam.TeamLeadId))
                {
                    var oldLead = await _userRepo.GetByIdAsync(existingTeam.TeamLeadId);
                    if (oldLead != null)
                    {
                        oldLead.TeamId = null;
                        await _userRepo.UpdateAsync(oldLead);
                    }
                }

                if (!string.IsNullOrEmpty(updatedTeam.TeamLeadId))
                {
                    var newLead = await _userRepo.GetByIdAsync(updatedTeam.TeamLeadId);
                    if (newLead != null)
                    {
                        newLead.TeamId = existingTeam.Id;
                        await _userRepo.UpdateAsync(newLead);
                    }
                }
            }

            existingTeam.Name = updatedTeam.Name;
            existingTeam.TeamLeadId = updatedTeam.TeamLeadId;
            existingTeam.ManagerId = updatedTeam.ManagerId;
            existingTeam.DirectorId = updatedTeam.DirectorId;

            await _teamRepo.UpdateAsync(existingTeam);
            TempData["message"] = "Successfully updated Team";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var team = await _teamRepo.GetByIdAsync(id);

            if (team == null)
                return NotFound();

            return View(team);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var team = await _teamRepo.GetByIdAsync(id);

            if (team == null)
                return NotFound();

            return View(team);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var team = await _teamRepo.GetByIdAsync(id);
            if (team == null)
                return NotFound();


            await _teamRepo.SoftDeleteAsync(id); 

            TempData["message"] = "Successfully deleted Team";
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
