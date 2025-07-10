using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RewardsAndRecognitionRepository.Enums;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;

[Authorize]
public class DashboardController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly INominationRepo _nominationRepo;
    private readonly ApplicationDbContext _context;

    public DashboardController(UserManager<User> userManager, INominationRepo nominationRepo, ApplicationDbContext context)
    {
        _userManager = userManager;
        _nominationRepo = nominationRepo;
        _context = context;
    }

    //public async Task<IActionResult> Index(Guid? teamId = null)
    //{
    //    var user = await _userManager.GetUserAsync(User);
    //    if (user == null) return RedirectToAction("Login", "Account");

    //    var roles = await _userManager.GetRolesAsync(user);
    //    var nominations = (await _nominationRepo.GetAllNominationsAsync()).ToList();

    //    if (roles.Contains("Manager"))
    //    {
    //        var managerNominations = nominations
    //            .Where(n => n.Nominee?.Team?.ManagerId == user.Id)
    //            .ToList();

    //        var teams = await _context.Teams
    //            .Include(t => t.Users)
    //            .Include(t => t.TeamLead)
    //            .Where(t => t.ManagerId == user.Id)
    //            .ToListAsync();

    //        var teamSummaries = teams.Select(t => new
    //        {
    //            TeamId = t.Id,
    //            TeamName = t.Name,
    //            TeamLeadName = t.TeamLead?.Name ?? "N/A",
    //            NominatedCount = nominations.Count(n => n.NominatorId == t.TeamLeadId)
    //        }).ToList<dynamic>();

    //        ViewBag.TeamsUnderManager = teamSummaries;
    //        ViewBag.SelectedTeamId = teamId?.ToString();

    //        ViewBag.SelectedTeamNominations = teamId != null && teamId != Guid.Empty
    //            ? managerNominations.Where(n => n.Nominee?.TeamId == teamId).ToList()
    //            : null;

    //        return View("ManagerDashboard", managerNominations);
    //    }

    //    if (roles.Contains("TeamLead"))
    //    {
    //        var teamLeadNominations = nominations
    //            .Where(n => n.NominatorId == user.Id)
    //            .ToList();
    //        return View("TeamLeadDashboard", teamLeadNominations);
    //    }

    //    if (roles.Contains("Admin"))
    //    {
    //        return View("AdminDashboard", nominations);
    //    }

    //    return View("EmployeeDashboard", nominations);
    //}
    public async Task<IActionResult> Index(Guid? teamId = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var roles = await _userManager.GetRolesAsync(user);
        var nominations = (await _nominationRepo.GetAllNominationsAsync()).ToList();

        // For Manager Role
        if (roles.Contains("Manager"))
        {
            var managerNominations = nominations
                .Where(n => n.Nominee?.Team?.ManagerId == user.Id)
                .ToList();

            var teams = await _context.Teams
                .Include(t => t.Users)
                .Include(t => t.TeamLead)
                .Where(t => t.ManagerId == user.Id)
                .ToListAsync();

            var teamSummaries = teams.Select(t => new
            {
                TeamId = t.Id,
                TeamName = t.Name,
                TeamLeadName = t.TeamLead?.Name ?? "N/A",
                NominatedCount = nominations.Count(n => n.NominatorId == t.TeamLeadId)
            }).ToList<dynamic>();

            ViewBag.TeamsUnderManager = teamSummaries;
            ViewBag.SelectedTeamId = teamId?.ToString();

            ViewBag.SelectedTeamNominations = teamId != null && teamId != Guid.Empty
                ? managerNominations.Where(n => n.Nominee?.TeamId == teamId).ToList()
                : new List<Nomination>();

            ViewBag.TotalNominations = managerNominations.Count;
            ViewBag.PendingNominations = managerNominations.Count(n => n.Status == NominationStatus.PendingManager);
            ViewBag.FinalApprovedNominations = managerNominations.Count(n => n.Status == NominationStatus.DirectorApproved);

            return View("ManagerDashboard", managerNominations);
        }

        // For Team Lead Role
        if (roles.Contains("TeamLead"))
        {
            var teamLeadNominations = nominations
                .Where(n => n.NominatorId == user.Id)
                .ToList();

            ViewBag.TotalNominations = teamLeadNominations.Count;
            ViewBag.PendingNominations = teamLeadNominations.Count(n => n.Status == NominationStatus.PendingManager);
            ViewBag.FinalApprovedNominations = teamLeadNominations.Count(n => n.Status == NominationStatus.DirectorApproved);

            return View("TeamLeadDashboard", teamLeadNominations);
        }

        // For Admin Role (Optional)
        if (roles.Contains("Admin"))
        {
            ViewBag.TotalNominations = nominations.Count;
            ViewBag.PendingNominations = nominations.Count(n => n.Status == NominationStatus.PendingManager);
            ViewBag.FinalApprovedNominations = nominations.Count(n => n.Status == NominationStatus.DirectorApproved);

            return View("AdminDashboard", nominations);
        }

        // For Regular Employee
        return View("EmployeeDashboard", nominations);
    }
}
