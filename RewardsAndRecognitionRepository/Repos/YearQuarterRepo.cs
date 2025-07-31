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
                .Where(yq => !yq.IsDeleted)
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
            if (yq.IsActive)
            {
                await _context.YearQuarters
                    .Where(x => x.IsActive)
                    .ExecuteUpdateAsync(set => set.SetProperty(x => x.IsActive, false));
            }
            await _context.SaveChangesAsync();

        }

        public async Task UpdateAsync(YearQuarter yq)
        {
            _context.YearQuarters.Update(yq);
            if (yq.IsActive)
            {
                await _context.YearQuarters
                    .Where(x => x.IsActive)
                    .ExecuteUpdateAsync(set => set.SetProperty(x => x.IsActive, false));
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var yq = await _context.YearQuarters.FindAsync(id);

            _context.YearQuarters.Remove(yq);
            await _context.SaveChangesAsync();

        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.YearQuarters.AnyAsync(yq => yq.Id == id && !yq.IsDeleted);
        }

        public async Task<IEnumerable<YearQuarter>> GetDeletedAsync()
        {
            return await _context.YearQuarters
                .Where(yq => yq.IsDeleted)
                .OrderByDescending(yq => yq.Year)
                .ThenByDescending(yq => yq.Quarter)
                .ToListAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var yq = await _context.YearQuarters.FindAsync(id);
            if (yq != null&& !yq.IsActive)
            {
                yq.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RestoreAsync(Guid id)
        {
            var yq = await _context.YearQuarters.FindAsync(id);
            if (yq != null && yq.IsDeleted)
            {
                yq.IsDeleted = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<YearQuarter>> GetActiveAsync()
        {
            return await _context.YearQuarters
                .Where(yq => !yq.IsDeleted)
                .OrderByDescending(yq => yq.Year)
                .ThenByDescending(yq => yq.Quarter)
                .ToListAsync();
        }

    }
}
