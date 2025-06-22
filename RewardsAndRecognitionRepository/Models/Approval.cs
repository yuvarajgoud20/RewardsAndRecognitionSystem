using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RewardsAndRecognitionRepository.Enums;

namespace RewardsAndRecognitionRepository.Models
{
    public class Approval
    {
        public Guid Id { get; set; }

        public Guid NominationId { get; set; }
        public Nomination Nomination { get; set; }

        public string ApproverId { get; set; }
        public User Approver { get; set; }

        public ApprovalAction Action { get; set; }
        public ApprovalLevel Level { get; set; }

        public DateTime ActionAt { get; set; }
        public string? Remarks { get; set; }
    }

}
