using Microsoft.AspNetCore.Authorization;
//Fixed Respective Director View and No nominations found message
using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public DashboardController(UserManager<User> userManager, INominationRepo nominationRepo, ApplicationDbContext context)
    {
        _userManager = userManager;
        _nominationRepo = nominationRepo;
        _context = context;
    }
[HttpGet]
public IActionResult GetYears()
{
    var years = _context.YearQuarters
        .Where(yq => !yq.IsDeleted) // Show only non-deleted
        .Select(yq => yq.Year)
        .Distinct()
        .OrderByDescending(y => y)
        .ToList();

    return Json(years);
}

[HttpGet]
public IActionResult GetQuarters(int year)
{
    var quarters = _context.YearQuarters
        .Where(yq => yq.Year == year && !yq.IsDeleted) // Show only non-deleted
        .Select(yq => new { id = yq.Id, name = $"{yq.Quarter}" })
        .ToList();

    return Json(quarters);
}

    [HttpGet]
    public async Task<IActionResult> GetTeamNominations(Guid teamId, int year, Guid quarterId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        // Determine user role
        var roles = await _userManager.GetRolesAsync(user);
        string role = roles.Contains("Director") ? "Director"
                     : roles.Contains("Manager") ? "Manager"
                     : string.Empty;

        if (string.IsNullOrEmpty(role))
            return Forbid();

        // Get team based on role
        IQueryable<Team> query = _context.Teams.Include(t => t.Users);
        Team team = null;

        if (role == "Manager")
            team = await query.FirstOrDefaultAsync(t => t.Id == teamId && t.ManagerId == user.Id);
        else if (role == "Director")
            team = await query.FirstOrDefaultAsync(t => t.Id == teamId && t.DirectorId == user.Id);

        if (team == null)
            return NotFound();

        // Filter nominations for team + year + quarter
        var nomineeIds = team.Users.Select(u => u.Id).ToList();
        ViewBag.SelectedTeamId = teamId.ToString();
        var nominations = await _context.Nominations
            .Include(n => n.Nominee)
            .Include(n => n.Category)
            .Include(n => n.Nominator)
            .Include(n => n.YearQuarter)
            .Where(n => nomineeIds.Contains(n.NomineeId)
                        && n.YearQuarter.Year == year
                        && n.YearQuarter.Id == quarterId)
            .ToListAsync();

        // Pass role to partial
        ViewBag.Role = role;
        ViewBag.TeamName = team.Name;
        ViewBag.CurrentYear = year;
        ViewBag.CurrentQuarterId = quarterId;

        return PartialView("_TeamNominationsModal", nominations);
    }

    public async Task<IActionResult> Index(Guid? yearQuarterId = null, Guid? teamId = null)


    {
        if (yearQuarterId == null || yearQuarterId == Guid.Empty)
        {
            var currentYQ = await _context.YearQuarters
                .Where(yq => yq.IsActive)
                .OrderByDescending(yq => yq.Year)
                .ThenByDescending(yq => yq.Quarter)
                .FirstOrDefaultAsync();
                 

            if (currentYQ == null)
            {
                TempData["Error"] = GeneralMessages.No_Active_Quarter;
                return View(new List<int>()); // fallback just to load dropdowns
            }


            return RedirectToAction(nameof(Index), new { yearQuarterId = currentYQ.Id });
        }
        var selectedQuarter = await _context.YearQuarters.FindAsync(yearQuarterId);
        if (selectedQuarter == null)
        {
            TempData["Error"] = GeneralMessages.No_Valid_Quarter;
            return View(new List<int>());
        }

        int currentYear = selectedQuarter.Year;
        var years = Enumerable.Range(currentYear - 10, 11).OrderByDescending(y => y).ToList();

        ViewBag.CurrentYear = currentYear;
        ViewBag.CurrentQuarterId = yearQuarterId;
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");
        var roles = await _userManager.GetRolesAsync(user);
        var nominations = (await _nominationRepo.GetAllNominationsAsync()).ToList();
       if (roles.Contains(nameof(Roles.Manager)))
       {
            // Get all teams under this manager
            var teams = await _context.Teams
                .Include(t => t.Users)
                .Include(t => t.TeamLead)
                .Where(t => t.ManagerId == user.Id)
                .ToListAsync();

            // Flatten all user IDs under these teams
            var teamUserIds = teams.SelectMany(t => t.Users).Select(u => u.Id).ToList();

            // Fetch nominations filtered by YearQuarterId where nominee belongs to these teams
            var nominationsList = await _context.Nominations
                .Include(n => n.Nominee)
                .Include(n => n.Category)
                .Include(n => n.Approvals)
                .Include(n => n.Nominator)
                .Where(n => !n.IsDeleted && teamUserIds.Contains(n.NomineeId) && n.YearQuarterId == yearQuarterId)
                .ToListAsync();

            // Breakdown nominations per team (for pie/bar chart)
            var teamStatusData = teams.Select(team =>
            {
                var nomineeIds = (team.Users ?? new List<User>()).Select(u => u.Id).ToList();
                var teamNoms = nominationsList.Where(n => nomineeIds.Contains(n.NomineeId)).ToList();

                var approved = teamNoms.Count(n => n.Status == NominationStatus.DirectorApproved);
                var rejected = teamNoms.Count(n => n.Status == NominationStatus.DirectorRejected);
                var total = teamNoms.Count;
                var pending = teamNoms.Count(n => n.Status == NominationStatus.PendingManager);
                return new
                {
                    TeamName = team.Name,
                    Approved = approved,
                    Rejected = rejected,
                    Pending = pending,
                    Total = total
                };
            }).ToList();

            // Summary: Team Lead + count of nominations they submitted
            var teamSummaries = teams.Select(t => new
            {
                TeamId = t.Id,
                TeamName = t.Name,
                TeamLeadName = t.TeamLead?.Name ?? GeneralMessages.NotAvailable_Error,
                NominatedCount = nominationsList.Count(n => n.NominatorId == t.TeamLeadId)
            }).ToList<dynamic>();

            // ViewBags
            ViewBag.TeamStatusData = teamStatusData;
            ViewBag.TeamsUnderManager = teamSummaries;
            ViewBag.SelectedQuarterId = yearQuarterId?.ToString();
            ViewBag.SelectedQuarterNominations = yearQuarterId != null && yearQuarterId != Guid.Empty
                ? nominationsList
                : new List<Nomination>();

            ViewBag.TotalNominations = nominationsList.Count;
            ViewBag.PendingNominations = nominationsList.Count(n => n.Status == NominationStatus.PendingManager);
            ViewBag.FinalApprovedNominations = nominationsList.Count(n => n.Status == NominationStatus.DirectorApproved);
            ViewBag.FinalRejectedNominations = nominationsList.Count(n => n.Status == NominationStatus.DirectorRejected);

            return View("ManagerDashboard", nominations);
       }      
      if (roles.Contains(nameof(Roles.Director)))
       {
            //Fetch ALl Nominations for the Director
            // Get teams under this Director
            var teams = await _context.Teams
                .Include(t => t.Users)
                .Include(t => t.TeamLead)
                .Where(t => t.DirectorId == user.Id)
                .ToListAsync();

            // Get nominee user IDs for this director's teams
            var nomineeIds = teams.SelectMany(t => t.Users).Select(u => u.Id).ToList();

            // Now filter nominations ONLY for these nominee IDs
            var NominationsList = await _context.Nominations
                .Include(n => n.Nominee)
                .Include(n => n.Category)
                .Include(n => n.Approvals)
                .Include(n => n.Nominator)
                .Where(n =>
                    !n.IsDeleted&&
                    nomineeIds.Contains(n.NomineeId) && // 🟢 This line restricts data to this Director only
                    n.YearQuarterId == yearQuarterId &&
                    n.Status != NominationStatus.PendingManager)
                .ToListAsync();
            // Breakdown nominations per team
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
            // Summary per team lead
            var teamSummaries = teams.Select(t => new
            {
                TeamId = t.Id,
                TeamName = t.Name,
                TeamLeadName = t.TeamLead?.Name ?? GeneralMessages.NotAvailable_Error,
                NominatedCount = NominationsList.Count(n => n.NominatorId == t.TeamLeadId)
            }).ToList<dynamic>();
            // ViewBags
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
        // For Team Lead Role
        if (roles.Contains(nameof(Roles.TeamLead)))
        {
            // Get the team where this user is the team lead
            var team = await _context.Teams
                .Include(t => t.Users)
                .FirstOrDefaultAsync(t => t.TeamLeadId == user.Id);

            if (team == null)
            {
                TempData["Error"] = GeneralMessages.No_Team_Assigned_TeamLead;
                // Set default ViewBag values to avoid runtime binding errors
                ViewBag.TotalNominations = 0;
                ViewBag.PendingNominations = 0;
                ViewBag.FinalApprovedNominations = 0;
                ViewBag.RejectedNominations = 0;
                ViewBag.ActiveQuarterId = yearQuarterId ?? Guid.Empty;
                ViewBag.ActiveQuarterName = selectedQuarter != null ? "Q" + selectedQuarter.Quarter : "";
                ViewBag.ActiveYearName = selectedQuarter?.Year ?? DateTime.Now.Year;

                return View("TeamLeadDashboard", new List<Nomination>());
            }

            var nomineeIds = team.Users.Select(u => u.Id).ToList();

            var teamLeadNominations = nominations
                .Where(n =>!n.IsDeleted && nomineeIds.Contains(n.NomineeId) && n.YearQuarterId == yearQuarterId)
                .ToList();

            ViewBag.TotalNominations = teamLeadNominations.Count;
            ViewBag.PendingNominations = teamLeadNominations.Count(n =>
                n.Status == NominationStatus.PendingManager ||
                n.Status == NominationStatus.ManagerApproved ||
                n.Status == NominationStatus.ManagerRejected);
            ViewBag.FinalApprovedNominations = teamLeadNominations.Count(n => n.Status == NominationStatus.DirectorApproved);
            ViewBag.RejectedNominations = teamLeadNominations.Count(n => n.Status == NominationStatus.DirectorRejected);

            ViewBag.ActiveQuarterId = yearQuarterId;
            ViewBag.ActiveQuarterName = "Q" + selectedQuarter.Quarter;
            ViewBag.ActiveYearName = selectedQuarter.Year;

            return View("TeamLeadDashboard", teamLeadNominations);
        }
        // For Admin Role (Optional)
        if (roles.Contains(nameof(Roles.Admin)))
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





