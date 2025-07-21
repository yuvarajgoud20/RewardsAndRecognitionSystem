using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionRepository.Service;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendEmailAsync(string subject, string body, string? to = null, string? cc = null, string? bcc = null, List<string>? attachments = null,bool isHtml = false)
    {
        using var message = new MailMessage();
        message.From = new MailAddress(_settings.From);
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = isHtml;

        // Add TO
        //foreach (var email in ParseEmails(to ?? _settings.To))
        message.To.Add(to);

        // Add CC
        if (!string.IsNullOrEmpty(cc ?? _settings.Cc))
        {
            foreach (var email in ParseEmails(cc ?? _settings.Cc))
                message.CC.Add(email);
        }

        // Add BCC
        if (!string.IsNullOrEmpty(bcc))
        {
            foreach (var email in ParseEmails(bcc))
                message.Bcc.Add(email);
        }

        // Attachments
        if (attachments != null)
        {
            foreach (var filePath in attachments)
            {
                if (File.Exists(filePath))
                {
                    message.Attachments.Add(new Attachment(filePath));
                }
            }
        }

        using var smtpClient = new SmtpClient(_settings.SmtpServer, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl
        };

        await smtpClient.SendMailAsync(message);
    }

    private List<MailAddress> ParseEmails(string emailList)
    {
        return emailList.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => new MailAddress(e.Trim()))
                        .ToList();
    }
}
