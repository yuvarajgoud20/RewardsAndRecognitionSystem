using RewardsAndRecognitionRepository.Enums;
using RewardsAndRecognitionRepository.Models;

namespace RewardsAndRecognitionSystem.ViewModels
{
    public class YearQuarterViewModel
    {
        public Guid Id { get; set; }

        public Quarter? Quarter { get; set; }
        public int Year { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsDeleted { get; set; } = false;


    }
}
