using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;

namespace RewardsAndRecognitionSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class YearQuarterController : Controller
    {
        private readonly IYearQuarterRepo _yearQuarterRepo;

        public YearQuarterController(IYearQuarterRepo yearQuarterRepo)
        {
            _yearQuarterRepo = yearQuarterRepo;
        }

        // GET: /YearQuarter
        public async Task<IActionResult> Index()
        {
            var quarters = await _yearQuarterRepo.GetAllAsync();
            return View(quarters);
        }

        // GET: /YearQuarter/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var quarter = await _yearQuarterRepo.GetByIdAsync(id);
            if (quarter == null)
                return NotFound();

            return View(quarter);
        }

        // GET: /YearQuarter/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /YearQuarter/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(YearQuarter yq)
        {
            if (ModelState.IsValid)
            {
                yq.Id = Guid.NewGuid();
                await _yearQuarterRepo.AddAsync(yq);
                return RedirectToAction(nameof(Index));
            }

            return View(yq);
        }

        // GET: /YearQuarter/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var yq = await _yearQuarterRepo.GetByIdAsync(id);
            if (yq == null)
                return NotFound();

            return View(yq);
        }

        // POST: /YearQuarter/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, YearQuarter yq)
        {
            if (id != yq.Id)
                return BadRequest();


            if (ModelState.IsValid)
            {
                await _yearQuarterRepo.UpdateAsync(yq);
                return RedirectToAction(nameof(Index));
            }

            return View(yq);
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
