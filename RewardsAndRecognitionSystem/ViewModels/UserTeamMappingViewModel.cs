using RewardsAndRecognitionRepository.Models;

namespace RewardsAndRecognitionSystem.ViewModels
{
    public class UserTeamMappingViewModel
    {
        public Guid SelectedTeamId { get; set; }
        public List<Team> Teams { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public List<string> SelectedUserIds { get; set; } = new();
    }
}
