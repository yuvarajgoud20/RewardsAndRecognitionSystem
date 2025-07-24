using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionRepository.Service;
using RewardsAndRecognitionSystem.ViewModels;

namespace RewardsAndRecognitionSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ICategoryRepo _categoryRepo;
        private readonly INominationRepo _nominationRepo;
        private readonly ICategoryService _categoryService;

        public CategoryController(IMapper mapper,ICategoryRepo categoryRepo, INominationRepo nominationRepo,ICategoryService service)
        {
            _mapper = mapper;
            _categoryRepo = categoryRepo;
            _nominationRepo = nominationRepo;
            _categoryService = service;
        }

        // GET: Category
        public async Task<IActionResult> Index(int page = 1, bool showDeleted = false)
        {
            // var categories = await _categoryRepo.GetAllAsync();
            int pageSize = 10;

            // 🔁 Fetch all categories based on showDeleted flag
            var allCategories = await _categoryRepo.GetAllAsync(showDeleted);
            var usedCategoryIds = await _nominationRepo.GetUsedCategoryIdsAsync(); // NEW LINE ✅

            int totalCategories = allCategories.Count();
            int totalPages = (int)Math.Ceiling(totalCategories / (double)pageSize);

            var paginatedCategories = allCategories
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.ShowDeleted = showDeleted;
            ViewBag.UsedCategoryIds = usedCategoryIds; // PASS TO VIEW ✅
            var viewModelList = _mapper.Map<List<CategoryViewModel>>(paginatedCategories);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CategoryListPartial", viewModelList);
            }
            return View(viewModelList);
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return NotFound();
            var viewModel= _mapper.Map<CategoryViewModel>(category);    
            return View(viewModel);
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var category = _mapper.Map<Category>(viewModel);
                category.Id = Guid.NewGuid();
                category.CreatedAt = DateTime.UtcNow;

                await _categoryRepo.AddAsync(category);
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            var viewModel=_mapper.Map<CategoryViewModel>(category);
            if (category == null) return NotFound();
            return View(viewModel);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CategoryViewModel viewModel)
        {
            if (id != viewModel.Id)
                return BadRequest();

            // 🔒 Fetch existing record from DB
            var existing = await _categoryRepo.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ModelState.Clear();
                var existingModel=_mapper.Map<CategoryViewModel>(existing);
                return View(existingModel);
            }

            // 🔐 Only update editable properties
            existing.Name = viewModel.Name;
            existing.Description =viewModel.Description;
            existing.isActive = viewModel.isActive;
            existing.CreatedAt = viewModel.CreatedAt;

            // ✅ Save updated record
            await _categoryRepo.UpdateAsync(existing);
            return RedirectToAction(nameof(Index));
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            var categoriesInNominations = await _nominationRepo.GetUniqueCategoriesAsync();
            ViewBag.CategoryIdsJson = JsonConvert.SerializeObject(
            categoriesInNominations.Select(c => c.Id).ToList());

            if (category == null) return NotFound();
            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null || category.IsDeleted)
            {
                return NotFound();
            }

            await _categoryRepo.SoftDeleteAsync(id); // ✅ Call soft delete
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Deleted()
        {
            var deletedCategories = await _categoryRepo.GetDeletedAsync();
            return View(deletedCategories);
        }
    }
}
