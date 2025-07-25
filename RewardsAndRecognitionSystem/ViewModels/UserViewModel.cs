using RewardsAndRecognitionRepository.Models;

namespace RewardsAndRecognitionSystem.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } 
        public string SelectedRole { get; set; }
        public Guid? TeamId { get; set; }
        public Team? Team { get; set; }
    }
}
