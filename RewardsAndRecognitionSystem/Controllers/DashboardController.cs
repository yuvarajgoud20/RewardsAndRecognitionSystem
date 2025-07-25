using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RewardsAndRecognitionRepository.Enums;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionSystem.Common;

[Authorize]
public class DashboardController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly INominationRepo _nominationRepo;
    private readonly ApplicationDbContext _context;
    private readonly IYearQuarterRepo _yearQuarterRepo;

    public DashboardController(UserManager<User> userManager, INominationRepo nominationRepo, ApplicationDbContext context, IYearQuarterRepo yearQuarterRepo)
    {
        _userManager = userManager;
        _nominationRepo = nominationRepo;
        _context = context;
        _yearQuarterRepo = yearQuarterRepo;
    }

    public async Task<IActionResult> Index(Guid? teamId = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var roles = await _userManager.GetRolesAsync(user);
        var activeYQ = (await _yearQuarterRepo.GetAllAsync()).FirstOrDefault(yq => yq.IsActive);

        if (activeYQ != null)
        {
            ViewBag.ActiveQuarterName = activeYQ.Quarter.ToString();
            ViewBag.ActiveYearName = activeYQ.Year.ToString();
        }
        else
        {
            ViewBag.ActiveQuarterName = GeneralMessages.NotAvailable_Error;
            ViewBag.ActiveYearName = GeneralMessages.NotAvailable_Error;
            ViewBag.ActiveQuarterCloseDate = GeneralMessages.NotAvailable_Error;
        }

        var startDate = activeYQ.StartDate;
        var endDate = activeYQ.EndDate;

        var nominations = (await _nominationRepo.GetAllNominationsAsync())
            .Where(n => n.YearQuarterId == activeYQ.Id)
            .ToList();

        if (roles.Contains(nameof(Roles.Manager)))
        {
            var teams = await _context.Teams
                .Include(t => t.Users)
                .Include(t => t.TeamLead)
                .Where(t => t.ManagerId == user.Id)
                .ToListAsync();

            var teamUserIds = teams.SelectMany(t => t.Users).Select(u => u.Id).ToList();

            var NominationsList = await _context.Nominations
                .Include(n => n.Nominee)
                .Include(n => n.Category)
                .Include(n => n.Approvals)
                .Include(n => n.Nominator)
                .Where(n => teamUserIds.Contains(n.NomineeId) && n.YearQuarterId == activeYQ.Id)
                .ToListAsync();

            var teamStatusData = teams.Select(team =>
            {
                var nomineeIds = (team.Users ?? new List<User>()).Select(u => u.Id).ToList();
                var teamNoms = NominationsList.Where(n => nomineeIds.Contains(n.NomineeId)).ToList();
                var approved = teamNoms.Count(n => n.Status == NominationStatus.DirectorApproved);
                var rejected = teamNoms.Count(n => n.Status == NominationStatus.DirectorRejected);
                var total = teamNoms.Count;
                var pending = total - approved - rejected;

                return new
                {
                    TeamName = team.Name,
                    Approved = approved,
                    Rejected = rejected,
                    Pending = pending,
                    Total = total
                };
            }).ToList();

            var teamSummaries = teams.Select(t => new
            {
                TeamId = t.Id,
                TeamName = t.Name,
                TeamLeadName = t.TeamLead?.Name ?? GeneralMessages.NotAvailable_Error,
                NominatedCount = NominationsList.Count(n => n.NominatorId == t.TeamLeadId)
            }).ToList<dynamic>();

            ViewBag.TeamStatusData = teamStatusData;
            ViewBag.TeamsUnderManager = teamSummaries;
            ViewBag.SelectedTeamId = teamId?.ToString();
            ViewBag.SelectedTeamNominations = teamId != null && teamId != Guid.Empty
                ? NominationsList.Where(n => n.Nominee?.TeamId == teamId).ToList()
                : new List<Nomination>();

            ViewBag.TotalNominations = NominationsList.Count;
            ViewBag.PendingNominations = NominationsList.Count(n => n.Status == NominationStatus.PendingManager);
            ViewBag.FinalApprovedNominations = NominationsList.Count(n => n.Status == NominationStatus.DirectorApproved);
            ViewBag.FinalRejectedNominations = NominationsList.Count(n => n.Status == NominationStatus.DirectorRejected);
            ViewBag.QuarterName = activeYQ.Quarter.ToString();
            ViewBag.YearName = activeYQ.Year.ToString();

            return View("ManagerDashboard", NominationsList);
        }

        if (roles.Contains(nameof(Roles.Director)))
        {
            var teams = await _context.Teams
                .Include(t => t.Users)
                .Include(t => t.TeamLead)
                .Where(t => t.DirectorId == user.Id)
                .ToListAsync();

            var NominationsList = await _context.Nominations
                .Include(n => n.Nominee)
                .Include(n => n.Category)
                .Include(n => n.Approvals)
                .Include(n => n.Nominator)
                .Where(n => n.Status != NominationStatus.PendingManager && n.YearQuarterId == activeYQ.Id)
                .ToListAsync();

            var teamStatusData = teams.Select(team =>
            {
                var nomineeIds = (team.Users ?? new List<User>()).Select(u => u.Id).ToList();
                var teamNoms = NominationsList.Where(n => nomineeIds.Contains(n.NomineeId)).ToList();
                var approved = teamNoms.Count(n => n.Status == NominationStatus.DirectorApproved);
                var rejected = teamNoms.Count(n => n.Status == NominationStatus.DirectorRejected);
                var total = teamNoms.Count;
                var pending = total - approved - rejected;

                return new
                {
                    TeamName = team.Name,
                    Approved = approved,
                    Rejected = rejected,
                    Pending = pending,
                    Total = total
                };
            }).ToList();

            var teamSummaries = teams.Select(t => new
            {
                TeamId = t.Id,
                TeamName = t.Name,
                TeamLeadName = t.TeamLead?.Name ?? GeneralMessages.NotAvailable_Error,
                NominatedCount = NominationsList.Count(n => n.NominatorId == t.TeamLeadId)
            }).ToList<dynamic>();

            ViewBag.TeamStatusData = teamStatusData;
            ViewBag.Teams = teamSummaries;
            ViewBag.SelectedTeamId = teamId?.ToString();
            ViewBag.SelectedTeamNominations = teamId != null && teamId != Guid.Empty
                ? NominationsList.Where(n => n.Nominee?.TeamId == teamId).ToList()
                : new List<Nomination>();

            ViewBag.TotalNominations = NominationsList.Count;
            ViewBag.PendingNominations = NominationsList.Count(n => n.Status == NominationStatus.ManagerApproved || n.Status == NominationStatus.ManagerRejected);
            ViewBag.FinalApprovedNominations = NominationsList.Count(n => n.Status == NominationStatus.DirectorApproved);
            ViewBag.FinalRejectedNominations = NominationsList.Count(n => n.Status == NominationStatus.DirectorRejected);

            return View("DirectorDashboard", NominationsList);
        }

        if (roles.Contains(nameof(Roles.TeamLead)))
        {
            var teamLeadNominations = nominations
                .Where(n => n.NominatorId == user.Id && n.YearQuarterId == activeYQ.Id)
                .ToList();

            ViewBag.TotalNominations = teamLeadNominations.Count;
            ViewBag.PendingNominations = teamLeadNominations.Count(n =>
                n.Status == NominationStatus.PendingManager ||
                n.Status == NominationStatus.ManagerApproved ||
                n.Status == NominationStatus.ManagerRejected);
            ViewBag.FinalApprovedNominations = teamLeadNominations.Count(n => n.Status == NominationStatus.DirectorApproved);
            ViewBag.RejectedNominations = teamLeadNominations.Count(n => n.Status == NominationStatus.DirectorRejected);
            ViewBag.ActiveQuarterId = activeYQ.Id;

            return View("TeamLeadDashboard", teamLeadNominations);
        }

        if (roles.Contains(nameof(Roles.Admin)))
        {
            var adminNominations = nominations
                .Where(n => n.YearQuarterId == activeYQ.Id)
                .ToList();

            ViewBag.TotalNominations = adminNominations.Count;
            ViewBag.PendingNominations = adminNominations.Count(n => n.Status == NominationStatus.PendingManager);
            ViewBag.FinalApprovedNominations = adminNominations.Count(n => n.Status == NominationStatus.DirectorApproved);

            return View("AdminDashboard", adminNominations);
        }

        var employeeNominations = nominations
            .Where(n => n.YearQuarterId == activeYQ.Id)
            .ToList();

        return View("EmployeeDashboard", employeeNominations);
    }
}