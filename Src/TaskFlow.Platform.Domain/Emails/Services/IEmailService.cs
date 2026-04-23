using TaskFlow.Platform.Domain.Emails.Models;

namespace TaskFlow.Platform.Domain.Emails.Services;

public interface IEmailService
{
    Task SendEmailAsync<T>(string templateName, T model, string to, string subject,
        CancellationToken cancellationToken = default)
        where T : class;

    Task SendEmailWithAttachmentsAsync<T>(
        string templateName,
        T model,
        string to,
        string subject,
        List<EmailAttachment> attachments,
        CancellationToken cancellationToken = default)
        where T : class;
}
