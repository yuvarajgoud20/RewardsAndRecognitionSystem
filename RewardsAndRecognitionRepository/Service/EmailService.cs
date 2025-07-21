using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionRepository.Service;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "banner.png");


    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendEmailAsync(
    string subject, string bodyHtml, string? to = null, string? cc = null, string? bcc = null,
    List<string>? attachments = null, bool isHtml = false)
    {
        using var message = new MailMessage();
        message.From = new MailAddress(_settings.From);
        message.Subject = subject;

        // TO
        message.To.Add(to);

        // CC
        if (!string.IsNullOrEmpty(cc ?? _settings.Cc))
        {
            foreach (var email in ParseEmails(cc ?? _settings.Cc))
                message.CC.Add(email);
        }

        // BCC
        if (!string.IsNullOrEmpty(bcc))
        {
            foreach (var email in ParseEmails(bcc))
                message.Bcc.Add(email);
        }

        // HTML with Embedded Image
        if (isHtml && !string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
        {
            var htmlView = AlternateView.CreateAlternateViewFromString(bodyHtml, null, "text/html");

            var linkedImage = new LinkedResource(imagePath, "image/png")
            {
                ContentId = "bannerImage", // should match cid used in HTML
                TransferEncoding = System.Net.Mime.TransferEncoding.Base64,
                ContentType = new System.Net.Mime.ContentType("image/png")
            };

            htmlView.LinkedResources.Add(linkedImage);
            message.AlternateViews.Add(htmlView);
            message.IsBodyHtml = true;
        }
        else
        {
            message.Body = bodyHtml;
            message.IsBodyHtml = isHtml;
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
