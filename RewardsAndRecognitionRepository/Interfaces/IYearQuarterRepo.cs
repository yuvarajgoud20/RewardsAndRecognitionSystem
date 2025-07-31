using RewardsAndRecognitionRepository.Models;

namespace RewardsAndRecognitionRepository.Interfaces
{
    public interface IYearQuarterRepo
    {
        Task<IEnumerable<YearQuarter>> GetAllAsync();
        Task<IEnumerable<YearQuarter>> GetActiveAsync();
        Task<IEnumerable<YearQuarter>> GetDeletedAsync();
        Task<YearQuarter?> GetByIdAsync(Guid id);
        Task AddAsync(YearQuarter yq);
        Task UpdateAsync(YearQuarter yq);
        Task DeleteAsync(Guid id);
        Task SoftDeleteAsync(Guid id);
        Task RestoreAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
