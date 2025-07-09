﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RewardsAndRecognitionRepository.Models;

namespace RewardsAndRecognitionRepository.Interfaces
{
    public interface INominationRepo
    {
        Task<IEnumerable<Nomination>> GetAllNominationsAsync();
        Task<Nomination?> GetNominationByIdAsync(Guid id);
        Task AddNominationAsync(Nomination nomination);
        Task UpdateNominationAsync(Nomination nomination);
        Task DeleteNominationAsync(Guid id);
        Task<List<Category>> GetUniqueCategoriesAsync();
    }
}