using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RewardsAndRecognitionRepository.Enums;
using RewardsAndRecognitionRepository.Models;

namespace RewardsAndRecognitionRepository.Service
{
    public class PendingNominationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PendingNominationBackgroundService> _logger;
       

        public PendingNominationBackgroundService(IServiceProvider serviceProvider,
                                          ILogger<PendingNominationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var targetTime = DateTime.Today.AddHours(8).AddMinutes(0);

                if (now >= targetTime && now < targetTime.AddMinutes(1))
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

                        await SendPendingNominationsAsync(context, emailService,userManager);
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // check every 1 min
            }
        }

        private async Task SendPendingNominationsAsync(ApplicationDbContext context, IEmailService emailSender,UserManager<User> _userManager)
        {
            var managers = await _userManager.GetUsersInRoleAsync("Manager");
            var directors = await _userManager.GetUsersInRoleAsync("Director");

            var combined = managers.Union(directors).Where(m => m.IsDeleted == false).ToList();

            foreach (var manager in combined)
            {
                var pendingNominations = await context.Nominations
                    .Include(n => n.Nominee)
                    .Include(n => n.Category)
                    .Include(n => n.Nominee.Team) // Ensure Team is loaded
                    .Where(n => ( n.IsDeleted == false ) && ( n.Status == NominationStatus.PendingManager )&&
                                (n.Nominee.Team.ManagerId == manager.Id || n.Nominee.Team.DirectorId == manager.Id))
                    .ToListAsync();

                if (pendingNominations.Any())
                {
                    // Build HTML body
                    var body = $@"
                        <body style=""font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #ffffff;"">
                            <div style=""background-color: #ffffff; padding: 20px; max-width: 600px; margin: auto; color: #000;"">
                                <img src=""cid:bannerImage"" alt=""Zelis Banner"" style=""width: 100%; max-width: 600px;"">
                                <h2 style=""color: #333;"">Pending Nominations</h2>
                                <p>Hi {manager.UserName},</p>
                                <p>The following nominations are pending under you:</p>
                                <table style=""border-collapse: collapse; width: 100%;"">
                                    <thead>
                                        <tr>
                                            <th style=""border: 1px solid #ddd; padding: 8px; background-color: #f2f2f2; text-align: left;"">Nominee</th>
                                            <th style=""border: 1px solid #ddd; padding: 8px; background-color: #f2f2f2; text-align: left;"">Category</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {string.Join("", pendingNominations.Select(n => $@"
                                            <tr>
                                                <td style=""border: 1px solid #ddd; padding: 8px;"">{n.Nominee.Name}</td>
                                                <td style=""border: 1px solid #ddd; padding: 8px;"">{n.Category.Name}</td>
                                                <td style=""border: 1px solid #ddd; padding: 8px;"">{n.Nominee.Team.Name}</td>
                                            </tr>"))}
                                    </tbody>
                                </table>
                                <p>Please review them at your earliest convenience.</p>
                                <br/>
                                <p>Regards,<br/>R&amp;R Team</p>
                            </div>
                        </body>";

                    await emailSender.SendEmailAsync(subject:"Pending Nominations" , isHtml: true , body:body , to: manager.Email );
                }
            }

            _logger.LogInformation("Pending nominations email sent at {time}", DateTime.Now);
        }

    }

}
