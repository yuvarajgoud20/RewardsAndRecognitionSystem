using RewardsAndRecognitionRepository.Models;

namespace RewardsAndRecognitionSystem.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public Guid? TeamId { get; set; }
        public Team? Team { get; set; }
    }
}
