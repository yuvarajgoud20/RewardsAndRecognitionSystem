using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RewardsAndRecognitionRepository.Enums;

namespace RewardsAndRecognitionRepository.Models
{
    public class YearQuarter
    {
        public Guid Id { get; set; }

        public Quarter? Quarter { get; set; }
        public int Year { get; set; }

        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public ICollection<Nomination>? Nominations { get; set; }
        public bool IsDeleted { get; set; } = false;
    }

}
