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
        public async Task<IEnumerable<User>> GetUnassignedUsersAsync()
        {
            return await _context.Users
                .Where(u => (u.IsDeleted == false || u.IsDeleted == null) && u.TeamId == null)
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.Include(u => u.Team).ThenInclude(u => u.Manager).ToListAsync();
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _context.Users
                .Include(u => u.Team)                     
                .ThenInclude(u => u.Manager)                  
                                                 
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Team)                     // load the user's Team
                .ThenInclude(u => u.Manager)
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
            await _context.SaveChangesAsync();
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(string userId)
        {
           try
            {
                var user = await GetByIdAsync(userId);
                _context.Users.Remove(user);
            }
            catch(Exception ex) 
            {
                    throw new RnRException(ex.InnerException.Message);
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
