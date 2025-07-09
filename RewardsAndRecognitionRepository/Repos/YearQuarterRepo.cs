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
            try
            {
                return await _context.YearQuarters.FindAsync(id);
            }
            catch
            {
                throw new RnRException("Cannot find the Year Quarter");
            }
           
        }

        public async Task AddAsync(YearQuarter yq)
        {
                _context.YearQuarters.Add(yq);
                await _context.SaveChangesAsync();
           
        }

        public async Task UpdateAsync(YearQuarter yq)
        {
            _context.YearQuarters.Update(yq);
            if ((DateTime.UtcNow < yq.EndDate) && (!yq.IsActive))
                throw new RnRException("Sorry We cannot Deactivate the Quarter before its End Date..");
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var yq = await _context.YearQuarters.FindAsync(id);
            if(yq != null && !yq.IsActive )
            {
                _context.YearQuarters.Remove(yq);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new RnRException("⚠️ Cannot delete: This quarter is currently active and cannot be deleted. Please deactivate it");
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.YearQuarters.AnyAsync(yq => yq.Id == id);
        }
    }
}
