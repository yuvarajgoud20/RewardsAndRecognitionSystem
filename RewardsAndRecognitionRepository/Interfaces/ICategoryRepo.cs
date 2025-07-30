using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RewardsAndRecognitionRepository.Models;

namespace RewardsAndRecognitionRepository.Interfaces
{
    public interface ICategoryRepo
    {
        Task<IEnumerable<Category>> GetAllAsync(bool includeDeleted = false);
        Task<Category?> GetByIdAsync(Guid id);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(Guid id);
        Task SoftDeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        public Task<IEnumerable<Category>> GetDeletedAsync();
        Task<List<Guid>> GetNominatedCategoriesAsync(string nomineeId);
        public Task<List<Guid>> GetUsedCategoryIdsAsync();
    }
}
