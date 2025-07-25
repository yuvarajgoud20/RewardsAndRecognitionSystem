using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionRepository.Service;
using RewardsAndRecognitionSystem.ViewModels;
using RewardsAndRecognitionRepository.Enums;

namespace RewardsAndRecognitionSystem.Controllers
{
    [Authorize(Roles = nameof(Roles.Admin))]
    public class CategoryController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ICategoryRepo _categoryRepo;
        private readonly INominationRepo _nominationRepo;
        private readonly ICategoryService _categoryService;

        public CategoryController(IMapper mapper, ICategoryRepo categoryRepo, INominationRepo nominationRepo, ICategoryService service)
        {
            _mapper = mapper;
            _categoryRepo = categoryRepo;
            _nominationRepo = nominationRepo;
            _categoryService = service;
        }

        public async Task<IActionResult> Index(int page = 1, bool showDeleted = false)
        {
            int pageSize = 5;

            var allCategories = await _categoryRepo.GetAllAsync(showDeleted);
            var usedCategoryIds = await _nominationRepo.GetUsedCategoryIdsAsync();

            int totalCategories = allCategories.Count();
            int totalPages = (int)Math.Ceiling(totalCategories / (double)pageSize);

            var paginatedCategories = allCategories
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.ShowDeleted = showDeleted;
            ViewBag.UsedCategoryIds = usedCategoryIds;

            var viewModelList = _mapper.Map<List<CategoryViewModel>>(paginatedCategories);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CategoryListPartial", viewModelList);
            }

            return View(viewModelList);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return NotFound();
            var viewModel = _mapper.Map<CategoryViewModel>(category);
            return View(viewModel);
        }

        public IActionResult Create()
        {
            return View();
        }

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

        public async Task<IActionResult> Edit(Guid id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return NotFound();
            var viewModel = _mapper.Map<CategoryViewModel>(category);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CategoryViewModel viewModel)
        {
            if (id != viewModel.Id)
                return BadRequest();

            var existing = await _categoryRepo.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ModelState.Clear();
                var existingModel = _mapper.Map<CategoryViewModel>(existing);
                return View(existingModel);
            }

            existing.Name = viewModel.Name;
            existing.Description = viewModel.Description;
            existing.isActive = viewModel.isActive;
            existing.CreatedAt = viewModel.CreatedAt;

            await _categoryRepo.UpdateAsync(existing);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            var categoriesInNominations = await _nominationRepo.GetUniqueCategoriesAsync();
            ViewBag.CategoryIdsJson = JsonConvert.SerializeObject(
                categoriesInNominations.Select(c => c.Id).ToList());

            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null || category.IsDeleted)
            {
                return NotFound();
            }

            await _categoryRepo.SoftDeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Deleted()
        {
            var deletedCategories = await _categoryRepo.GetDeletedAsync();
            return View(deletedCategories);
        }
    }
}
