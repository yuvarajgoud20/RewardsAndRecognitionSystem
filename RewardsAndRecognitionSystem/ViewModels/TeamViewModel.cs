using RewardsAndRecognitionRepository.Models;

namespace RewardsAndRecognitionSystem.ViewModels
{
    public class TeamViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string TeamLeadId { get; set; }
      
        public string ManagerId { get; set; }
       
        public string DirectorId { get; set; }

    }
}
