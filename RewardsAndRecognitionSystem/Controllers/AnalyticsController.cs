using System;
//MAIN CODE
using System.Collections.Generic;

using System.IO;

using System.Linq;

using System.Security.Claims;

using System.Threading.Tasks;

using iTextSharp.text;

using iTextSharp.text.pdf;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using RewardsAndRecognitionRepository.Enums;

using RewardsAndRecognitionRepository.Models;

namespace RewardsAndRecognitionSystem.Controllers

{

    [Authorize(Roles = "Manager,TeamLead,Director")]

    public class AnalyticsController : Controller

    {

        private readonly ApplicationDbContext _context;

        public AnalyticsController(ApplicationDbContext context)

        {

            _context = context;

        }

        public IActionResult Index(Guid? yearQuarterId)

        {

            if (yearQuarterId == null)

            {

                TempData["Error"] = "No quarter selected.";

                return View();

            }

            ViewBag.SelectedYearQuarterId = yearQuarterId?.ToString();

            return View();

        }

        [HttpGet]

        public IActionResult GetYears()

        {

            var years = _context.YearQuarters

                .Select(yq => yq.Year)

                .Distinct()

                .OrderByDescending(y => y)

                .ToList();

            return Json(years);

        }

        [HttpGet]

        public async Task<IActionResult> GetQuarters(int year)

        {

            var quarters = await _context.YearQuarters

                .Where(yq => yq.Year == year)

                .OrderBy(yq => yq.Quarter)

                .Select(yq => new

                {

                    id = yq.Id,

                    name = $"{yq.Quarter}"

                })

                .ToListAsync();

            return Json(quarters);

        }

        [HttpGet]

        public async Task<IActionResult> RedirectToCurrent()

        {

            var currentYQ = await _context.YearQuarters

                .FirstOrDefaultAsync(yq => yq.IsActive == true);

            if (currentYQ == null)

            {

                TempData["Error"] = "No active quarter is set in the system.";

                return RedirectToAction("Index");

            }

            return RedirectToAction("Index", new Dictionary<string, object>

            {

                { "yearQuarterId", currentYQ.Id }

            });

        }

        [HttpGet]

        public async Task<IActionResult> GetQuarterTeamSummary(Guid yearQuarterId)

        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var isManager = User.IsInRole("Manager");

            var isDirector = User.IsInRole("Director");

            var query = _context.Nominations

                .Include(n => n.Nominator)

                    .ThenInclude(u => u.Team)

                .Where(n => n.YearQuarterId == yearQuarterId && n.Nominator.Team != null);

            if (isManager)

            {

                var teamIds = await _context.Teams

                    .Where(t => t.ManagerId.ToString() == userId)

                    .Select(t => t.Id)

                    .ToListAsync();

                query = query.Where(n => n.Nominator.TeamId != null && teamIds.Contains(n.Nominator.TeamId.Value));

            }

            // Role-specific filtering for summary

            if (isDirector)

            {

                // Only count nominations ready for director

                query = query.Where(n =>

                      n.Status == NominationStatus.ManagerApproved ||

        n.Status == NominationStatus.ManagerRejected ||

        n.Status == NominationStatus.DirectorApproved ||

        n.Status == NominationStatus.DirectorRejected);


            }

            var teamSummary = await query

        .GroupBy(n => n.Nominator.Team.Name)

        .Select(g => new

        {

            team = g.Key,

            count = g.Count()

        })

        .ToListAsync();

            return Json(teamSummary);

        }



        [HttpGet]
        public async Task<IActionResult> GetAnalyticsData(Guid yearQuarterId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isManager = User.IsInRole("Manager");
            var isDirector = User.IsInRole("Director");

            var query = _context.Nominations
                .Include(n => n.Nominator)
                    .ThenInclude(u => u.Team)
                .Include(n => n.Category)
                .Where(n => n.YearQuarterId == yearQuarterId &&
                            n.Nominator.Team != null &&
                            n.Category != null);

            if (isManager)
            {
                var managerTeamIds = await _context.Teams
                    .Where(t => t.ManagerId.ToString() == userId)
                    .Select(t => t.Id)
                    .ToListAsync();

                query = query.Where(n => n.Nominator.TeamId != null && managerTeamIds.Contains(n.Nominator.TeamId.Value));
            }

            var nominations = await query.ToListAsync();

            if (!nominations.Any())
            {
                return Json(new { message = "No nominations were made in this quarter." });
            }

            var categoryTeamData = nominations
                .GroupBy(n => new
                {
                    CategoryName = n.Category.Name,
                    TeamName = n.Nominator.Team.Name
                })
                .Select(g =>
                {
                    int pending, approved, rejected;

                    if (isDirector)
                    {
                        // Director considers pending as anything waiting at director level
                        pending = g.Count(n =>
                            n.Status == NominationStatus.PendingDirector ||
                            n.Status == NominationStatus.ManagerApproved ||
                            n.Status == NominationStatus.ManagerRejected);
                    }
                    else
                    {
                        pending = g.Count(n => n.Status == NominationStatus.PendingManager);
                    }

                    approved = g.Count(n => n.Status == NominationStatus.DirectorApproved);
                    rejected = g.Count(n => n.Status == NominationStatus.DirectorRejected);

                    return new
                    {
                        category = g.Key.CategoryName,
                        team = g.Key.TeamName,
                        totalCount = pending + approved + rejected,
                        pendingCount = pending,
                        approvedCount = approved,
                        rejectedCount = rejected
                    };
                })
                .Where(x => x.pendingCount > 0 || x.approvedCount > 0 || x.rejectedCount > 0) // 👈 Show if ANY count > 0
                .ToList();

            if (!categoryTeamData.Any())
            {
                return Json(new { message = "No data available for this quarter." });
            }

            return Json(new { categoryTeamData });
        }


        [HttpPost]

        public IActionResult GenerateChartsPdf([FromBody] List<string> base64Images)

        {

            using (var memoryStream = new MemoryStream())

            {

                using (var doc = new Document())

                {

                    PdfWriter.GetInstance(doc, memoryStream);

                    doc.Open();

                    foreach (var base64 in base64Images)

                    {

                        var base64Data = base64.Split(',')[1];

                        var imageBytes = Convert.FromBase64String(base64Data);

                        var img = Image.GetInstance(imageBytes);

                        img.ScaleToFit(doc.PageSize.Width - 50, doc.PageSize.Height - 100);

                        img.Alignment = Image.ALIGN_CENTER;

                        doc.Add(img);

                        doc.NewPage();

                    }

                    doc.Close();

                }

                return File(memoryStream.ToArray(), "application/pdf", "AnalyticsCharts.pdf");

            }


        }
        [HttpGet]

        [Authorize(Roles = "TeamLead")]

        public async Task<IActionResult> GetTeamLeadCategoryAnalytics(Guid yearQuarterId)

        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))

            {

                return Unauthorized();

            }

            var nominations = await _context.Nominations

                .Include(n => n.Nominator)

                    .ThenInclude(u => u.Team)

                .Include(n => n.Category)

                .Where(n =>

                    n.YearQuarterId == yearQuarterId &&

                    n.Nominator.Team != null &&

                    n.Category != null &&

                    n.Nominator.Id == userId)

                .ToListAsync();

            var result = nominations

                .GroupBy(n => n.Category.Name)

                .Select(group => new

                {

                    Category = group.Key,

                    Approved = group.Count(n => n.Status == NominationStatus.DirectorApproved),

                    Rejected = group.Count(n => n.Status == NominationStatus.DirectorRejected),

                    Pending = group.Count(n =>

                        n.Status == NominationStatus.ManagerRejected ||

                        n.Status == NominationStatus.PendingManager ||

                        n.Status == NominationStatus.ManagerApproved ||

                        n.Status == NominationStatus.PendingDirector)

                })

                .ToList();



            return Json(new { result });

        }


        [HttpGet]
        public async Task<IActionResult> GetTeamLeadAnalyticsData(Guid yearQuarterId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var nominations = await _context.Nominations
                .Include(n => n.Nominator)
                    .ThenInclude(u => u.Team)
                .Include(n => n.Category)
                .Where(n =>
                    n.YearQuarterId == yearQuarterId &&
                    n.Nominator.Team != null &&
                    n.Category != null &&
                    n.Nominator.Team.TeamLeadId.ToString() == userId)
                .ToListAsync();

            var result = nominations
                .GroupBy(n => n.Category.Name)
                .Select(g => new
                {
                    category = g.Key,
                    approvedCount = g.Count(n => n.Status == NominationStatus.DirectorApproved),
                    rejectedCount = g.Count(n => n.Status == NominationStatus.DirectorRejected),
                    pendingCount = g.Count(n =>
                        n.Status == NominationStatus.PendingManager ||
                        n.Status == NominationStatus.ManagerApproved ||
                        n.Status == NominationStatus.PendingDirector)
                })
                .ToList();

            return Json(result);
        }


    }

}

