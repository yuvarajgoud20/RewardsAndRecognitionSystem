using RewardsAndRecognitionRepository.Models;

namespace RewardsAndRecognitionRepository.Interfaces
{
    public interface IYearQuarterRepo
    {
        Task<IEnumerable<YearQuarter>> GetAllAsync();
        Task<YearQuarter?> GetByIdAsync(Guid id);
        Task AddAsync(YearQuarter yq);
        Task UpdateAsync(YearQuarter yq);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
