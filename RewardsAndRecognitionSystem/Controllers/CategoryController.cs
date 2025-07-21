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
        public async Task<IActionResult> Index()
        {
           // var categories = await _categoryRepo.GetAllAsync();
           var categories=await _categoryService.GetAllAsync();
            var viewModelList = _mapper.Map<List<CategoryViewModel>>(categories);
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
            await _categoryRepo.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
