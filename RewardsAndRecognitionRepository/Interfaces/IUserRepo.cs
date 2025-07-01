using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RewardsAndRecognitionRepository.Models;

namespace RewardsAndRecognitionRepository.Interfaces
{
    public interface IUserRepo
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(string id);
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<IEnumerable<User>> GetUsersByTeamAsync(Guid teamId);
        Task<User?> GetUserWithTeamAsync(string userId);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(string userId);
        Task SaveAsync();

        Task<IEnumerable<User>> GetAllManagersAsync();

        Task<IEnumerable<User>> GetLeadsAsync(string? currentLeadId = null);

        Task<IEnumerable<User>> GetAllDirectorsAsync();
    }
}
