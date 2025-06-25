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

            // Step 2: Create a test manager
            var managerEmail = "manager1@example.com";
            if (await userManager.FindByEmailAsync(managerEmail) == null)
            {
                var manager = new User
                {
                    UserName = managerEmail,
                    Email = managerEmail,
                    Name = "Manager One",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(manager, "Manager@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(manager, "Manager");
                }
            }

            // Step 3: Create a test employee
            var empEmail = "employee1@example.com";
            if (await userManager.FindByEmailAsync(empEmail) == null)
            {
                var employee = new User
                {
                    UserName = empEmail,
                    Email = empEmail,
                    Name = "Employee One",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(employee, "Employee@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(employee, "Employee");
                }
            }

            // Add more roles/users as needed
        }
    }
}
