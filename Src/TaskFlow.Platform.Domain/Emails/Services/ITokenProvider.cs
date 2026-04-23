using TaskFlow.Platform.Domain.Emails.Models;

namespace TaskFlow.Platform.Domain.Emails.Services;

public interface ITokenProvider
{
    string Provider { get; }
    Task<List<EmailMessage>> GetEmailsFromProviderAsync(int maxEmails, CancellationToken cancellationToken);
    Task RevokeTokenFromProviderAsync(CancellationToken cancellationToken = default);
}
