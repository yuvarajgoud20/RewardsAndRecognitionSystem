﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RewardsAndRecognitionRepository.Models
{
    public class GroupedTeam
    {
        public Team Team { get; set; }
        public List<User> Users { get; set; }
    }
}
