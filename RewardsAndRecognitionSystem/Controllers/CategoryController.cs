using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;

namespace RewardsAndRecognitionSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepo _categoryRepo;

        public CategoryController(ICategoryRepo categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepo.GetAllAsync();
            return View(categories);
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                category.Id = Guid.NewGuid();
                category.CreatedAt = DateTime.UtcNow;

                await _categoryRepo.AddAsync(category);
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Category category)
        {
            if (id != category.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                await _categoryRepo.UpdateAsync(category);
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _categoryRepo.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
