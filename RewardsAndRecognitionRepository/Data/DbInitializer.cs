using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RewardsAndRecognitionRepository.Models;

namespace RewardsAndRecognitionRepository.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndUsersAsync(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            // Step 1: Define roles
            string[] roleNames = { "Admin", "TeamLead", "Manager", "Director", "Employee" };

            foreach (var role in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
