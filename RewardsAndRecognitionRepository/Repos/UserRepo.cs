using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;

namespace RewardsAndRecognitionRepository.Repos
{
    public class UserRepo : IUserRepo
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserRepo(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.Include(u => u.Team).ToListAsync();
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByTeamAsync(Guid teamId)
        {
            return await _context.Users
                .Where(u => u.TeamId == teamId && u.IsActive == true)
                .ToListAsync();
        }

        public async Task<User?> GetUserWithTeamAsync(string userId)
        {
            return await _context.Users
                .Include(u => u.Team)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(string userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                _context.Users.Remove(user);
            }
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetAllManagersAsync()
        {
            var managers = await _userManager.GetUsersInRoleAsync("Manager");
            return managers;
        }
        public async Task<IEnumerable<User>> GetAllDirectorsAsync()
        {
            var directors = await _userManager.GetUsersInRoleAsync("Director");
            return directors;
        }

        public async Task<IEnumerable<User>> GetLeadsAsync(string? currentLeadId = null)
        {
            var teamLeadRoleId = await _context.Roles
                .Where(r => r.Name == "TeamLead")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            var teamLeadUserIds = await _context.UserRoles
                .Where(ur => ur.RoleId == teamLeadRoleId)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var assignedLeadIds = await _context.Teams
                .Where(t => t.TeamLeadId != null)
                .Select(t => t.TeamLeadId)
                .ToListAsync();

            return await _context.Users
                .Where(u =>
                    teamLeadUserIds.Contains(u.Id)
       && (u.Id == currentLeadId || !assignedLeadIds.Contains(u.Id))
                )
                .ToListAsync();
        }
    }
}
