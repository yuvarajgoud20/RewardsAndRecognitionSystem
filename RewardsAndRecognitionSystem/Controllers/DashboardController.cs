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

    public async Task<IActionResult> Index(Guid? teamId = null)

    {

        var user = await _userManager.GetUserAsync(User);

        if (user == null) return RedirectToAction("Login", "Account");

        var roles = await _userManager.GetRolesAsync(user);

        var nominations = (await _nominationRepo.GetAllNominationsAsync()).ToList();

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

                .Where(n => teamUserIds.Contains(n.NomineeId))

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

                                .Where(n => n.Status != NominationStatus.PendingManager)

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

            ViewBag.PendingNominations = NominationsList.Count(n => n.Status == NominationStatus.ManagerApproved||n.Status==NominationStatus.ManagerRejected);

            ViewBag.FinalApprovedNominations = NominationsList.Count(n => n.Status == NominationStatus.DirectorApproved);

            ViewBag.FinalRejectedNominations = NominationsList.Count(n => n.Status == NominationStatus.DirectorRejected);

            return View("DirectorDashboard", NominationsList);

        }


        // For Team Lead Role

        if (roles.Contains("TeamLead"))

        {

            var teamLeadNominations = nominations
             .Where(n => n.NominatorId == user.Id)
              .ToList();

            ViewBag.TotalNominations = teamLeadNominations.Count;
            ViewBag.PendingNominations = teamLeadNominations.Count(n => n.Status == NominationStatus.PendingManager || n.Status == NominationStatus.ManagerApproved || n.Status == NominationStatus.ManagerRejected);
            ViewBag.FinalApprovedNominations = teamLeadNominations.Count(n => n.Status == NominationStatus.DirectorApproved);
            ViewBag.RejectedNominations = teamLeadNominations.Count(n => n.Status == NominationStatus.DirectorRejected);
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

