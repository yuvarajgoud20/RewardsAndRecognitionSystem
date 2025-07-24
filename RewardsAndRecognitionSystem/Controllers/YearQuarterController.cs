using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using RewardsAndRecognitionRepository.Enums;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionSystem.ViewModels;

namespace RewardsAndRecognitionSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class YearQuarterController : Controller
    {
        private readonly IYearQuarterRepo _yearQuarterRepo;
        private readonly IMapper _mapper;

        public YearQuarterController(IMapper mapper,IYearQuarterRepo yearQuarterRepo)
        {
            _mapper = mapper;
            _yearQuarterRepo = yearQuarterRepo;
        }

        // GET: /YearQuarter
        public async Task<IActionResult> Index(int page=1)
        {
            int pageSize = 10;

            var allQuarters = await _yearQuarterRepo.GetAllAsync();
            var totalRecords = allQuarters.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var paginated = allQuarters
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            var qList = _mapper.Map<List<YearQuarterViewModel>>(paginated);

            // ✅ This handles AJAX pagination
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_YearQuarterListPartial", qList);
            }
           
              
            return View(qList);
        }

        // GET: /YearQuarter/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var quarter = await _yearQuarterRepo.GetByIdAsync(id);
            if (quarter == null)
                return NotFound();
            var quarterView = _mapper.Map<YearQuarterViewModel>(quarter);
            return View(quarterView);
        }

        // GET: /YearQuarter/Create
        public IActionResult Create()
        {
            ViewBag.Quarters = new SelectList(Enum.GetValues(typeof(Quarter)));

            return View();
        }

        // POST: /YearQuarter/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(YearQuarterViewModel yq)
        {
            var yqs = await _yearQuarterRepo.GetAllAsync();
            if (yqs.Any(yearquarter => yearquarter.Year == yq.Year && yearquarter.Quarter == yq.Quarter))
            {
                ModelState.AddModelError("Quarter", " Year + Quarter Combination already Exists.please select another quarter");
            }
            if (!ModelState.IsValid)
            {
                
                ViewBag.Quarters = new SelectList(Enum.GetValues(typeof(Quarter)));
                ModelState.Remove("Year");
                return View(yq);
            }
                yq.Id = Guid.NewGuid();
               var yearQuarter=_mapper.Map<YearQuarter>(yq); 
                await _yearQuarterRepo.AddAsync(yearQuarter);
                return RedirectToAction(nameof(Index));                
        }

        // GET: /YearQuarter/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var yq = await _yearQuarterRepo.GetByIdAsync(id);
            if (yq == null)
                return NotFound();

            ViewBag.Quarters = new SelectList(Enum.GetValues(typeof(Quarter)));
            var yearQuarter = _mapper.Map<YearQuarterViewModel>(yq);
            return View(yearQuarter);
        }


        // POST: /YearQuarter/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, YearQuarterViewModel yq)
        {
            if (id != yq.Id)
                return BadRequest();

            // 🔒 Fetch existing record from DB
            var existing = await _yearQuarterRepo.GetByIdAsync(id);
            
            if (existing == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                // Re-populate dropdown and return view with user input preserved
                ViewBag.Quarters = new SelectList(Enum.GetValues(typeof(Quarter)));
                // Clear ModelState entries for fields that were removed from the form
                var existingYearQuarter = _mapper.Map<YearQuarterViewModel>(existing);
                //ModelState.Clear();
                return View(existingYearQuarter);
            }

            // 🔐 Only update fields that are editable (never trust full model from form)
            existing.Quarter = yq.Quarter;
            existing.Year = yq.Year;
            existing.StartDate = yq.StartDate;
            existing.EndDate = yq.EndDate;
            existing.IsActive = yq.IsActive;
            // ✅ Save updated data
            await _yearQuarterRepo.UpdateAsync(existing);
            return RedirectToAction(nameof(Index));
        }


        // GET: /YearQuarter/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var yq = await _yearQuarterRepo.GetByIdAsync(id);
            if (yq == null)
                return NotFound();

            return View(yq);
        }

        // POST: /YearQuarter/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _yearQuarterRepo.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
