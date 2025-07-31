using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RewardsAndRecognitionRepository.Models
{
    public class Team
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string TeamLeadId { get; set; }
        public User? TeamLead { get; set; }

        public string ManagerId { get; set; }
        public User? Manager { get; set; }

        public string DirectorId { get; set; }
        public User? Director { get; set; }


        public ICollection<User>? Users { get; set; }
        public bool IsDeleted { get; set; } = false;
    }

}
