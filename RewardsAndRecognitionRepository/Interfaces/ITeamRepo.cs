using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RewardsAndRecognitionRepository.Models;

namespace RewardsAndRecognitionRepository.Interfaces
{
    public interface ITeamRepo
    {
        Task<IEnumerable<Team>> GetAllAsync(bool includeDeleted = false);  // UPDATED
        Task<Team?> GetByIdAsync(Guid id);
        Task AddAsync(Team team);
        Task UpdateAsync(Team team);
        Task DeleteAsync(Team team);
        Task SoftDeleteAsync(Guid id);  // NEW
        Task<IEnumerable<Team>> GetDeletedAsync();  // NEW
    }
}
