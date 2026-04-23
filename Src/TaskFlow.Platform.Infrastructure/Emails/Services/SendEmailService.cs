using System.Net;
using System.Net.Mail;
using TaskFlow.Platform.Domain.Emails.Models;
using TaskFlow.Platform.Domain.Emails.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskFlow.Platform.Infrastructure.Emails.Options;

namespace TaskFlow.Platform.Infrastructure.Emails.Services;

public sealed class SendEmailService : ISendEmailService, IDisposable
{
    private readonly SmtpClient _smtpClient;
    private readonly ILogger<SendEmailService> _logger;

    public SendEmailService(IOptions<MailServerOptions> options, ILogger<SendEmailService> logger)
    {
        _logger = logger;
        var optionsValue = options.Value;
        _smtpClient = new SmtpClient(optionsValue.Host, optionsValue.Port)
        {
            Credentials = new NetworkCredential(optionsValue.Username, optionsValue.Password),
            EnableSsl = optionsValue.UseSsl
        };
    }

    public async Task SendEmailAsync(Email message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Preparing to send email to {Recipient} with subject '{Subject}'",
            message.To, message.Subject);

        using var mailMessage = new MailMessage(message.From, message.To, message.Subject, message.Message);
        mailMessage.IsBodyHtml = true;

        if (message.HasDownloadableAttachments())
        {
            _logger.LogInformation(
                "Processing {Count} downloadable attachment(s) for the email",
                message.Attachment.Count);

            foreach (var attachment in message.Attachment)
            {
                try
                {
                    if (attachment.Content.Length <= 0)
                    {
                        continue;
                    }

                    var memoryStream = new MemoryStream(attachment.Content);
                    var mailAttachment =
                        new Attachment(memoryStream, attachment.FileName, attachment.ContentType);
                    mailMessage.Attachments.Add(mailAttachment);

                    _logger.LogDebug(
                        "Attachment added: {FileName}, Size: {Size} bytes",
                        attachment.FileName, attachment.Content.Length);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error attachment {FileName}: {Message}",
                        attachment.FileName, ex.Message);
                }
            }
        }

        await _smtpClient.SendMailAsync(mailMessage, cancellationToken);
    }

    public void Dispose()
    {
        _smtpClient.Dispose();
    }
}
