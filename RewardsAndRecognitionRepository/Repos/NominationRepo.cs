using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;

namespace RewardsAndRecognitionRepository.Repos
{
    public class NominationRepo : INominationRepo
    {
        private readonly ApplicationDbContext _context;

        public NominationRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Nomination>> GetAllNominationsAsync()
        {
            return await _context.Nominations
                .Include(n => n.Nominator)
                .Include(n => n.Nominee)
                .Include(n => n.Category)
                .Include(n => n.YearQuarter)
                .Include(n => n.Approvals)
                .ToListAsync();
        }

        public async Task<Nomination?> GetNominationByIdAsync(Guid id)
        {
            return await _context.Nominations
                .Include(n => n.Nominator)
                .Include(n => n.Nominee)
                .ThenInclude(u => u.Team)
                .Include(n => n.Category)
                .Include(n => n.YearQuarter)
                .Include(n => n.Approvals)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task AddNominationAsync(Nomination nomination)
        {
            _context.Nominations.Add(nomination);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateNominationAsync(Nomination nomination)
        {
            _context.Nominations.Update(nomination);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteNominationAsync(Guid id)
        {
            var nomination = await _context.Nominations.FindAsync(id);
            if (nomination != null)
            {
                _context.Nominations.Remove(nomination);
                await _context.SaveChangesAsync();
            }
        }
    }
}
