using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RewardsAndRecognitionRepository.Enums;

namespace RewardsAndRecognitionRepository.Models
{
    public class Nomination
    {
        public Guid Id { get; set; }

        public string NominatorId { get; set; }
        public User? Nominator { get; set; }

        public string NomineeId { get; set; }
        public User? Nominee { get; set; }

        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }

        public string Description { get; set; }
        public string Achievements { get; set; }

        public NominationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public Guid YearQuarterId { get; set; }
        public YearQuarter? YearQuarter { get; set; }

        public ICollection<Approval>? Approvals { get; set; }
        public bool IsDeleted { get; set; }
    }

}
