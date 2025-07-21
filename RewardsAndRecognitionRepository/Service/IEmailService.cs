using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RewardsAndRecognitionRepository.Service
{
    public interface IEmailService
    {
        Task SendEmailAsync(string subject, string body, string? to = null, string? cc = null, string? bcc = null, List<string>? attachments = null,bool isHtml = false);
    }

}
