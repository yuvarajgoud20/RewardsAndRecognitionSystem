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

                // ✅ Director relationship
                entity.HasOne(t => t.Director)
                      .WithMany()
                      .HasForeignKey(t => t.DirectorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

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
        }
    }

}
