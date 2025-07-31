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
    public class TeamRepo : ITeamRepo
    {
        private readonly ApplicationDbContext _context;

        public TeamRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Team>> GetAllAsync()
        {

            return await _context.Teams
                .Include(t => t.TeamLead)
                .Include(t => t.Manager)
                .Include(t => t.Director)
                .ToListAsync();
        }

        public async Task<Team?> GetByIdAsync(Guid id)
        {
            return await _context.Teams
                .Include(t => t.TeamLead)
                .Include(t => t.Manager)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task AddAsync(Team team)
        {
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Team team)
        {
            _context.Teams.Update(team);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Team team)
        {
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Team>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _context.Teams
                .Include(t => t.TeamLead)
                .Include(t => t.Manager)
                .Include(t => t.Director)
                .Include(t => t.Users);

            if (includeDeleted)
            {
                return await query.Where(t => t.IsDeleted).ToListAsync();
            }
            else
            {
                return await query.Where(t => !t.IsDeleted).ToListAsync();
            }
        }


        public async Task SoftDeleteAsync(Guid id)
        {
            var team = await _context.Teams
                .Include(t => t.Users)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
                return;

            team.IsDeleted = true;

            if (team.Users != null)
            {
                foreach (var user in team.Users)
                {
                    user.TeamId = null;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Team>> GetDeletedAsync()
        {
            return await _context.Teams
                .Include(t => t.TeamLead)
                .Include(t => t.Manager)
                .Include(t => t.Director)
                .Where(t => t.IsDeleted)
                .ToListAsync();
        }

    }
}
