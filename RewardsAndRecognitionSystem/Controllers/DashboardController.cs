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
    private readonly IYearQuarterRepo _yearQuarterRepo;

    public DashboardController(UserManager<User> userManager, INominationRepo nominationRepo, ApplicationDbContext context, IYearQuarterRepo yearQuarterRepo)
    {
        _userManager = userManager;
        _nominationRepo = nominationRepo;
        _context = context;
        _yearQuarterRepo = yearQuarterRepo;  // add this
    }


    public async Task<IActionResult> Index(Guid? teamId = null)

    {

        var user = await _userManager.GetUserAsync(User);

        if (user == null) return RedirectToAction("Login", "Account");

        var roles = await _userManager.GetRolesAsync(user);
        //var activeYQ = (await _yearQuarterRepo.GetAllAsync())
        //        .FirstOrDefault(yq => yq.IsActive);

        //if (activeYQ == null)
        //{
        //    // No active YearQuarter, return a message or empty view
        //    ViewBag.Message = "No active Year and Quarter configured.";
        //    return View("NoActiveYearQuarter");
        //}
        var activeYQ = (await _yearQuarterRepo.GetAllAsync()).FirstOrDefault(yq => yq.IsActive);

        if (activeYQ != null)
        {
            ViewBag.ActiveQuarterName = activeYQ.Quarter.ToString();
            ViewBag.ActiveYearName = activeYQ.Year.ToString();
          //  ViewBag.ActiveQuarterCloseDate = activeYQ.EndDate.ToShortDateString();

        }
        else
        {
            ViewBag.ActiveQuarterName = "N/A";
            ViewBag.ActiveYearName = "N/A";
            ViewBag.ActiveQuarterCloseDate = "N/A";
        }
        var startDate = activeYQ.StartDate;
        var endDate = activeYQ.EndDate;


        var nominations = (await _nominationRepo.GetAllNominationsAsync())
      .Where(n => n.YearQuarterId == activeYQ.Id)
      .ToList();


        if (roles.Contains("Manager"))

        {

            // Get all teams under this manager

            var teams = await _context.Teams

                .Include(t => t.Users)

                .Include(t => t.TeamLead)

                .Where(t => t.ManagerId == user.Id)

                .ToListAsync();

            // Flatten all user IDs under these teams

            var teamUserIds = teams.SelectMany(t => t.Users).Select(u => u.Id).ToList();

            // Fetch nominations where the nominee belongs to any of those teams

            var NominationsList = await _context.Nominations
      .Include(n => n.Nominee)
      .Include(n => n.Category)
      .Include(n => n.Approvals)
      .Include(n => n.Nominator)
      .Where(n => teamUserIds.Contains(n.NomineeId) && n.YearQuarterId == activeYQ.Id)
      .ToListAsync();


            // Breakdown nominations per team (for pie/bar chart)

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

            // Summary: Team Lead + count of nominations they submitted

            var teamSummaries = teams.Select(t => new

            {

                TeamId = t.Id,

                TeamName = t.Name,

                TeamLeadName = t.TeamLead?.Name ?? "N/A",

                NominatedCount = NominationsList.Count(n => n.NominatorId == t.TeamLeadId)

            }).ToList<dynamic>();

            // ViewBags

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
            ViewBag.QuarterName = activeYQ.Quarter.ToString();     // e.g. "Q1"
            ViewBag.YearName = activeYQ.Year.ToString();           // e.g. "2025"
         //   ViewBag.ClosesAt = activeYQ.EndDate.ToString("MMMM dd, yyyy");  // e.g. "September 30, 2025"


            return View("ManagerDashboard", NominationsList);

        }


        //For Director 

        if (roles.Contains("Director"))

        {

            // Get all teams under this Director

            var teams = await _context.Teams

                .Include(t => t.Users)

                .Include(t => t.TeamLead)

                .Where(t => t.DirectorId == user.Id)

                .ToListAsync();

            //Fetch ALl Nominations for the Director

            var NominationsList = await _context.Nominations
      .Include(n => n.Nominee)
      .Include(n => n.Category)
      .Include(n => n.Approvals)
      .Include(n => n.Nominator)
      .Where(n => n.Status != NominationStatus.PendingManager && n.YearQuarterId == activeYQ.Id)
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

                TeamLeadName = t.TeamLead?.Name ?? "N/A",

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

        // Assume activeYQ was retrieved earlier and is not null

        if (roles.Contains("TeamLead"))
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

        if (roles.Contains("Admin"))
        {
            var adminNominations = nominations
                .Where(n => n.YearQuarterId == activeYQ.Id)
                .ToList();

            ViewBag.TotalNominations = adminNominations.Count;
            ViewBag.PendingNominations = adminNominations.Count(n => n.Status == NominationStatus.PendingManager);
            ViewBag.FinalApprovedNominations = adminNominations.Count(n => n.Status == NominationStatus.DirectorApproved);
            return View("AdminDashboard", adminNominations);
        }

        // Regular Employee
        var employeeNominations = nominations
            .Where(n => n.YearQuarterId == activeYQ.Id)
            .ToList();

        return View("EmployeeDashboard", employeeNominations);


    }

}





//using Microsoft.AspNetCore.Authorization;

//using Microsoft.AspNetCore.Identity;

//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;

//using RewardsAndRecognitionRepository.Enums;

//using RewardsAndRecognitionRepository.Interfaces;

//using RewardsAndRecognitionRepository.Models;

//[Authorize]

//public class DashboardController : Controller

//{

//    private readonly UserManager<User> _userManager;

//    private readonly INominationRepo _nominationRepo;

//    private readonly ApplicationDbContext _context;

//    public DashboardController(UserManager<User> userManager, INominationRepo nominationRepo, ApplicationDbContext context)

//    {

//        _userManager = userManager;

//        _nominationRepo = nominationRepo;

//        _context = context;

//    }
//    [HttpGet]
//    public IActionResult GetYears()
//    {
//        var years = _context.YearQuarters.Select(yq => yq.Year).Distinct().OrderByDescending(y => y).ToList();
//        return Json(years);
//    }

//    [HttpGet]
//    public IActionResult GetQuarters(int year)
//    {
//        var quarters = _context.YearQuarters
//            .Where(yq => yq.Year == year)
//            .Select(yq => new { id = yq.Id, name = $"Q{yq.Quarter}" })
//            .ToList();
//        return Json(quarters);
//    }

//    [HttpGet]
//    public async Task<IActionResult> GetTeamNominations(Guid teamId)
//    {
//        var user = await _userManager.GetUserAsync(User);
//        if (user == null)
//            return Unauthorized();

//        // Get roles
//        var roles = await _userManager.GetRolesAsync(user);

//        IQueryable<Team> query = _context.Teams.Include(t => t.Users);

//        Team team = null;

//        if (roles.Contains("Manager"))
//        {
//            team = await query.FirstOrDefaultAsync(t => t.Id == teamId && t.ManagerId == user.Id);
//        }
//        else if (roles.Contains("Director"))
//        {
//            team = await query.FirstOrDefaultAsync(t => t.Id == teamId && t.DirectorId == user.Id);
//        }
//        else
//        {
//            // User not authorized to view teams
//            return Forbid();
//        }

//        if (team == null)
//            return NotFound();

//        var nomineeIds = team.Users.Select(u => u.Id).ToList();

//        var nominations = await _context.Nominations
//            .Include(n => n.Nominee)
//            .Include(n => n.Category)
//            .Where(n => nomineeIds.Contains(n.NomineeId))
//            .ToListAsync();

//        ViewBag.TeamName = team.Name;

//        return PartialView("_TeamNominationsModal", nominations);
//    }



//    public async Task<IActionResult> Index(Guid? yearQuarterId = null, Guid? teamId = null)


//    {
//        if (yearQuarterId == null || yearQuarterId == Guid.Empty)
//        {
//            var currentYQ = await _context.YearQuarters
//                .Where(yq => yq.IsActive)
//                .OrderByDescending(yq => yq.Year)
//                .ThenByDescending(yq => yq.Quarter)
//                .FirstOrDefaultAsync();

//            if (currentYQ == null)
//            {
//                TempData["Error"] = "No active Year-Quarter found.";
//                return View(new List<int>()); // fallback just to load dropdowns
//            }

//            return RedirectToAction(nameof(Index), new { yearQuarterId = currentYQ.Id });
//        }
//        var selectedQuarter = await _context.YearQuarters.FindAsync(yearQuarterId);
//        if (selectedQuarter == null)
//        {
//            TempData["Error"] = "Selected Year-Quarter not found.";
//            return View(new List<int>());
//        }

//        int currentYear = selectedQuarter.Year;
//        var years = Enumerable.Range(currentYear - 10, 11).OrderByDescending(y => y).ToList();

//        ViewBag.CurrentYear = currentYear;
//        ViewBag.CurrentQuarterId = yearQuarterId;



//        var user = await _userManager.GetUserAsync(User);

//        if (user == null) return RedirectToAction("Login", "Account");

//        var roles = await _userManager.GetRolesAsync(user);

//        var nominations = (await _nominationRepo.GetAllNominationsAsync()).ToList();

//        if (roles.Contains("Manager"))
//        {
//            // Get all teams under this manager
//            var teams = await _context.Teams
//                .Include(t => t.Users)
//                .Include(t => t.TeamLead)
//                .Where(t => t.ManagerId == user.Id)
//                .ToListAsync();

//            // Flatten all user IDs under these teams
//            var teamUserIds = teams.SelectMany(t => t.Users).Select(u => u.Id).ToList();

//            // Fetch nominations filtered by YearQuarterId where nominee belongs to these teams
//            var nominationsList = await _context.Nominations
//                .Include(n => n.Nominee)
//                .Include(n => n.Category)
//                .Include(n => n.Approvals)
//                .Include(n => n.Nominator)
//                .Where(n => teamUserIds.Contains(n.NomineeId) && n.YearQuarterId == yearQuarterId)
//                .ToListAsync();

//            // Breakdown nominations per team (for pie/bar chart)
//            var teamStatusData = teams.Select(team =>
//            {
//                var nomineeIds = (team.Users ?? new List<User>()).Select(u => u.Id).ToList();
//                var teamNoms = nominationsList.Where(n => nomineeIds.Contains(n.NomineeId)).ToList();

//                var approved = teamNoms.Count(n => n.Status == NominationStatus.DirectorApproved);
//                var rejected = teamNoms.Count(n => n.Status == NominationStatus.DirectorRejected);
//                var total = teamNoms.Count;
//                var pending = total - approved - rejected;

//                return new
//                {
//                    TeamName = team.Name,
//                    Approved = approved,
//                    Rejected = rejected,
//                    Pending = pending,
//                    Total = total
//                };
//            }).ToList();

//            // Summary: Team Lead + count of nominations they submitted
//            var teamSummaries = teams.Select(t => new
//            {
//                TeamId = t.Id,
//                TeamName = t.Name,
//                TeamLeadName = t.TeamLead?.Name ?? "N/A",
//                NominatedCount = nominationsList.Count(n => n.NominatorId == t.TeamLeadId)
//            }).ToList<dynamic>();

//            // ViewBags
//            ViewBag.TeamStatusData = teamStatusData;
//            ViewBag.TeamsUnderManager = teamSummaries;
//            ViewBag.SelectedQuarterId = yearQuarterId?.ToString();
//            ViewBag.SelectedQuarterNominations = yearQuarterId != null && yearQuarterId != Guid.Empty
//                ? nominationsList
//                : new List<Nomination>();

//            ViewBag.TotalNominations = nominationsList.Count;
//            ViewBag.PendingNominations = nominationsList.Count(n => n.Status == NominationStatus.PendingManager);
//            ViewBag.FinalApprovedNominations = nominationsList.Count(n => n.Status == NominationStatus.DirectorApproved);
//            ViewBag.FinalRejectedNominations = nominationsList.Count(n => n.Status == NominationStatus.DirectorRejected);

//            return View("ManagerDashboard", nominations);
//        }

//        if (roles.Contains("Director"))

//        {

//            // Get all teams under this Director

//            var teams = await _context.Teams

//                .Include(t => t.Users)

//                .Include(t => t.TeamLead)

//                .Where(t => t.DirectorId == user.Id)

//                .ToListAsync();

//            //Fetch ALl Nominations for the Director
//            var NominationsList = await _context.Nominations
//                .Include(n => n.Nominee)
//                .Include(n => n.Category)
//                .Include(n => n.Approvals)
//                .Include(n => n.Nominator)
//                .Where(n =>
//                    n.YearQuarterId == yearQuarterId     // ← filter by selected YearQuarter
//                    && n.Status != NominationStatus.PendingManager)
//                .ToListAsync();


//            // Breakdown nominations per team

//            var teamStatusData = teams.Select(team =>

//            {

//                var nomineeIds = (team.Users ?? new List<User>()).Select(u => u.Id).ToList();

//                var teamNoms = NominationsList.Where(n => nomineeIds.Contains(n.NomineeId)).ToList();

//                var approved = teamNoms.Count(n => n.Status == NominationStatus.DirectorApproved);

//                var rejected = teamNoms.Count(n => n.Status == NominationStatus.DirectorRejected);

//                var total = teamNoms.Count;

//                var pending = total - approved - rejected;

//                return new

//                {

//                    TeamName = team.Name,

//                    Approved = approved,

//                    Rejected = rejected,

//                    Pending = pending,

//                    Total = total

//                };

//            }).ToList();

//            // Summary per team lead

//            var teamSummaries = teams.Select(t => new

//            {

//                TeamId = t.Id,

//                TeamName = t.Name,

//                TeamLeadName = t.TeamLead?.Name ?? "N/A",

//                NominatedCount = NominationsList.Count(n => n.NominatorId == t.TeamLeadId)

//            }).ToList<dynamic>();

//            // ViewBags

//            ViewBag.TeamStatusData = teamStatusData;

//            ViewBag.Teams = teamSummaries;

//            ViewBag.SelectedTeamId = teamId?.ToString();

//            ViewBag.SelectedTeamNominations = teamId != null && teamId != Guid.Empty

//                ? NominationsList.Where(n => n.Nominee?.TeamId == teamId).ToList()

//                : new List<Nomination>();

//            ViewBag.TotalNominations = NominationsList.Count;

//            ViewBag.PendingNominations = NominationsList.Count(n => n.Status == NominationStatus.ManagerApproved || n.Status == NominationStatus.ManagerRejected);

//            ViewBag.FinalApprovedNominations = NominationsList.Count(n => n.Status == NominationStatus.DirectorApproved);

//            ViewBag.FinalRejectedNominations = NominationsList.Count(n => n.Status == NominationStatus.DirectorRejected);

//            return View("DirectorDashboard", NominationsList);

//        }




//        // For Team Lead Role

//        if (roles.Contains("TeamLead"))

//        {

//            var teamLeadNominations = nominations
//             .Where(n => n.NominatorId == user.Id)
//              .ToList();

//            ViewBag.TotalNominations = teamLeadNominations.Count;
//            ViewBag.PendingNominations = teamLeadNominations.Count(n => n.Status == NominationStatus.PendingManager || n.Status == NominationStatus.ManagerApproved || n.Status == NominationStatus.ManagerRejected);
//            ViewBag.FinalApprovedNominations = teamLeadNominations.Count(n => n.Status == NominationStatus.DirectorApproved);
//            ViewBag.RejectedNominations = teamLeadNominations.Count(n => n.Status == NominationStatus.DirectorRejected);
//            return View("TeamLeadDashboard", teamLeadNominations);

//        }

//        // For Admin Role (Optional)

//        if (roles.Contains("Admin"))

//        {

//            ViewBag.TotalNominations = nominations.Count;

//            ViewBag.PendingNominations = nominations.Count(n => n.Status == NominationStatus.PendingManager);

//            ViewBag.FinalApprovedNominations = nominations.Count(n => n.Status == NominationStatus.DirectorApproved);

//            return View("AdminDashboard", nominations);

//        }

//        // For Regular Employee

//        return View("EmployeeDashboard", nominations);

//    }

//}





