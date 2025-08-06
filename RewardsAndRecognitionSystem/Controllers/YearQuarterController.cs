using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using RewardsAndRecognitionRepository.Enums;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionSystem.Common;
using RewardsAndRecognitionSystem.ViewModels;
using Superpower.Model;

namespace RewardsAndRecognitionSystem.Controllers
{
    [Authorize(Roles = nameof(Roles.Admin))]
    public class YearQuarterController : Controller
    {
        private readonly IYearQuarterRepo _yearQuarterRepo;
        private readonly IMapper _mapper;
        private readonly PaginationSettings _paginationSettings;


        public YearQuarterController(IMapper mapper, IYearQuarterRepo yearQuarterRepo, IOptions<PaginationSettings> paginationOptions)
        {
            _mapper = mapper;
            _yearQuarterRepo = yearQuarterRepo;
            _paginationSettings = paginationOptions.Value;
        }

        public async Task<IActionResult> Index(string filter = "active", int page = 1)
        {
            int pageSize = _paginationSettings.DefaultPageSize;
            IEnumerable<YearQuarter> allQuarters;

            if (filter == "deleted")
            {
                allQuarters = await _yearQuarterRepo.GetDeletedAsync();
            }
            else // default is active
            {
                allQuarters = await _yearQuarterRepo.GetActiveAsync();
            }

            var sortedQuarters = allQuarters
           .OrderBy(q => q.Year)
           .ThenBy(q => q.Quarter)
           .ToList();

            var totalRecords = sortedQuarters.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var paginated = sortedQuarters
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SelectedFilter = filter;

            var qList = _mapper.Map<List<YearQuarterViewModel>>(paginated);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_YearQuarterListPartial", qList);
            }

            return View(qList);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var quarter = await _yearQuarterRepo.GetByIdAsync(id);
            if (quarter == null)
                return NotFound();
            var quarterView = _mapper.Map<YearQuarterViewModel>(quarter);
            return View(quarterView);
        }

        public IActionResult Create()
        {
            ViewBag.Quarters = new SelectList(Enum.GetValues(typeof(Quarter)));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(YearQuarterViewModel yq)
        {
            var yqs = await _yearQuarterRepo.GetAllAsync();

            // Check for duplicate Year + Quarter (non-deleted only)
            if (yqs.Any(q => q.Year == yq.Year && q.Quarter == yq.Quarter && q.IsDeleted == false))
            {
                ModelState.AddModelError("Quarter", "Year + Quarter combination already exists. Please select another.");
            }

            // Ensure both dates are entered before checking overlaps
            if (yq.StartDate.HasValue && yq.EndDate.HasValue)
            {
                var start = yq.StartDate.Value.Date;
                var end = yq.EndDate.Value.Date;

               
                
                // Overlap check with all non-deleted quarters
                bool overlaps = yqs.Any(q =>
                    q.IsDeleted == false &&
                    q.Year == yq.Year &&
                    q.StartDate.HasValue && q.EndDate.HasValue &&
                    (
                        (start >= q.StartDate.Value.Date && start <= q.EndDate.Value.Date) ||      
                        (end >= q.StartDate.Value.Date && end <= q.EndDate.Value.Date) ||           
                        (start <= q.StartDate.Value.Date && end >= q.EndDate.Value.Date)           
                    )
                );

                if (overlaps)
                {
                    ModelState.AddModelError("", "A quarter already exists within this date range.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Quarters = new SelectList(Enum.GetValues(typeof(Quarter)));
                return View(yq);
            }

            yq.Id = Guid.NewGuid();
            var yearQuarter = _mapper.Map<YearQuarter>(yq);
            await _yearQuarterRepo.AddAsync(yearQuarter);

            TempData["message"] = ToastMessages_YearQuarter.CreateYearQuarter;

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]

        public async Task<IActionResult> Edit(Guid id)
        {
            var yq = await _yearQuarterRepo.GetByIdAsync(id);
            if (yq == null)
                return NotFound();

            ViewBag.Quarters = new SelectList(Enum.GetValues(typeof(Quarter)));
            var yearQuarter = _mapper.Map<YearQuarterViewModel>(yq);
            return View(yearQuarter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, YearQuarterViewModel yq)
        {
            if (id != yq.Id)
                return BadRequest();

            var existing = await _yearQuarterRepo.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            var yqs = await _yearQuarterRepo.GetAllAsync();

            // Duplicate year + quarter (excluding itself)
            if (yqs.Any(q => q.Id != yq.Id && q.Year == yq.Year && q.Quarter == yq.Quarter && q.IsDeleted == false))
            {
                ModelState.AddModelError("Quarter", "Year + Quarter combination already exists. Please select another.");
            }

            // Check overlapping date ranges (excluding itself)
            if (yq.StartDate.HasValue && yq.EndDate.HasValue)
            {
                var start = yq.StartDate.Value.Date;
                var end = yq.EndDate.Value.Date;

                bool overlaps = yqs.Any(q =>
                    q.Id != yq.Id &&
                    q.IsDeleted == false &&
                    q.Year == yq.Year &&
                    (
                        (start >= q.StartDate && start <= q.EndDate) ||      
                        (end >= q.StartDate && end <= q.EndDate) ||          
                        (start <= q.StartDate && end >= q.EndDate)           
                    )
                );

                if (overlaps)
                {
                    ModelState.AddModelError("", "A quarter already exists within this date range.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Quarters = new SelectList(Enum.GetValues(typeof(Quarter)));
                return View(yq);
            }

            // Update the existing entity
            existing.Quarter = yq.Quarter;
            existing.Year = yq.Year;
            existing.StartDate = yq.StartDate;
            existing.EndDate = yq.EndDate;
            existing.IsActive = yq.IsActive;

            await _yearQuarterRepo.UpdateAsync(existing);
            TempData["message"] = ToastMessages_YearQuarter.UpdateYearQuarter;
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var yq = await _yearQuarterRepo.GetByIdAsync(id);
            if (yq == null)
                return NotFound();

            return View(yq);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {

            await _yearQuarterRepo.SoftDeleteAsync(id);

            TempData["message"] = ToastMessages_YearQuarter.DeleteYearQuarter;
            return RedirectToAction(nameof(Index));
        }

    }
}
