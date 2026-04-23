using TaskFlow.Platform.Domain.Emails.Models;

namespace TaskFlow.Platform.Domain.Emails.Services;

public interface IEmailConnector
{
    Task<List<EmailMessage>> GetEmailsAsync(string provider, int maxEmails = 10, CancellationToken cancellationToken = default);

    Task<EmailMessage?> GetEmailByIdAsync(EmailConnectorConfig config, uint emailId,
        CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(CancellationToken cancellationToken = default);
}
