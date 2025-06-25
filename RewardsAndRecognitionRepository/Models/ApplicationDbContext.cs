using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RewardsAndRecognitionRepository.Enums;

namespace RewardsAndRecognitionRepository.Models
{

    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<YearQuarter> YearQuarters { get; set; }
        public DbSet<Nomination> Nominations { get; set; }
        public DbSet<Approval> Approvals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Enums as strings

            modelBuilder.Entity<User>().HasDiscriminator().HasValue("User");

            

            modelBuilder
                .Entity<Nomination>()
                .Property(n => n.Status)
                .HasConversion<string>();

            modelBuilder
                .Entity<YearQuarter>()
                .Property(yq => yq.Quarter)
                .HasConversion<string>();

            modelBuilder
                .Entity<Approval>()
                .Property(a => a.Action)
                .HasConversion<string>();

            modelBuilder
                .Entity<Approval>()
                .Property(a => a.Level)
                .HasConversion<string>();

            // Relationships
            modelBuilder.Entity<Team>(entity =>
            {
                // Team Lead relationship
                entity.HasOne(t => t.TeamLead)
                      .WithMany()
                      .HasForeignKey(t => t.TeamLeadId)
                      .OnDelete(DeleteBehavior.Restrict);

                // ✅ Manager relationship
                entity.HasOne(t => t.Manager)
                      .WithMany()
                      .HasForeignKey(t => t.ManagerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder
                .Entity<User>()
                .HasOne(u => u.Manager)
                .WithMany()
                .HasForeignKey(u => u.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Nomination>()
                .HasOne(n => n.Nominator)
                .WithMany(u => u.NominationsGiven)
                .HasForeignKey(n => n.NominatorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Nomination>()
                .HasOne(n => n.Nominee)
                .WithMany(u => u.NominationsReceived)
                .HasForeignKey(n => n.NomineeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Dummy seed data
            var adminId = Guid.NewGuid().ToString();
            var teamLeadId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var employeeId = Guid.NewGuid().ToString();
            var teamId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var yqId = Guid.NewGuid();

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = adminId,
                    Name = "Admin User",
                    Email = "admin@company.com",
                    PasswordHash = "hashed_password_here",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = managerId,
                    Name = "Manager User",
                    Email = "manager@company.com",
                    PasswordHash = "hashed_password_here",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = teamLeadId,
                    Name = "Team Lead User",
                    Email = "lead@company.com",
                    PasswordHash = "hashed_password_here",
                    ManagerId = managerId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = employeeId,
                    Name = "Employee One",
                    Email = "employee@company.com",
                    PasswordHash = "hashed_password_here",
                    ManagerId = managerId,
                    TeamId = teamId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            modelBuilder.Entity<Team>().HasData(
                new Team
                {
                    Id = teamId,
                    Name = "Alpha Team",
                    TeamLeadId = teamLeadId
                }
            );

            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = categoryId,
                    Name = "Best Performer",
                    Description = "Awarded to best overall performer",
                    CreatedAt = DateTime.UtcNow
                }
            );

            modelBuilder.Entity<YearQuarter>().HasData(
                new YearQuarter
                {
                    Id = yqId,
                    Year = 2025,
                    Quarter = Quarter.Q2,
                    IsActive = true,
                    StartDate = new DateTime(2025, 4, 1),
                    EndDate = new DateTime(2025, 6, 30)
                }
            );
        }
    }

}
