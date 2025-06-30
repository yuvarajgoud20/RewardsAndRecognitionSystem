using Microsoft.EntityFrameworkCore;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;

namespace RewardsAndRecognitionRepository.Repositories
{
    public class YearQuarterRepo : IYearQuarterRepo
    {
        private readonly ApplicationDbContext _context;

        public YearQuarterRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<YearQuarter>> GetAllAsync()
        {
            return await _context.YearQuarters
                .OrderByDescending(yq => yq.Year)
                .ThenByDescending(yq => yq.Quarter)
                .ToListAsync();
        }

        public async Task<YearQuarter?> GetByIdAsync(Guid id)
        {
            return await _context.YearQuarters.FindAsync(id);
        }

        public async Task AddAsync(YearQuarter yq)
        {
            _context.YearQuarters.Add(yq);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(YearQuarter yq)
        {
            _context.YearQuarters.Update(yq);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var yq = await _context.YearQuarters.FindAsync(id);
            if (yq != null)
            {
                _context.YearQuarters.Remove(yq);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.YearQuarters.AnyAsync(yq => yq.Id == id);
        }
    }
}
