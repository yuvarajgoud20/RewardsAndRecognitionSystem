﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;

namespace RewardsAndRecognitionRepository.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepo _categoryRepo;

        public CategoryService(ICategoryRepo categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
           // throw new Exception("Excepton from Service Layer");
            return await _categoryRepo.GetAllAsync();
        }

        public async Task<Category?> GetByIdAsync(Guid id)
        {
            return await _categoryRepo.GetByIdAsync(id);
        }

        public async Task AddAsync(Category category)
        {
            // Add business rules or validations here if needed
            await _categoryRepo.AddAsync(category);
        }

        public async Task UpdateAsync(Category category)
        {
            // Validate or business rules can go here
            await _categoryRepo.UpdateAsync(category);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _categoryRepo.DeleteAsync(id);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _categoryRepo.ExistsAsync(id);
        }
    }
}
