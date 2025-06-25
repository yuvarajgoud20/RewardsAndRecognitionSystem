using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RewardsAndRecognitionRepository.Enums;

namespace RewardsAndRecognitionRepository.Models
{
    public class User : IdentityUser
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public Guid? TeamId { get; set; }
        public string? ManagerId { get; set; }

        public bool? IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Team? Team { get; set; }
        public User? Manager { get; set; }

        public ICollection<Nomination>? NominationsGiven { get; set; }
        public ICollection<Nomination>? NominationsReceived { get; set; }
        public ICollection<Approval>? Approvals { get; set; }
    }

}
