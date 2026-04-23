using TaskFlow.Platform.Domain.Emails.Models;

namespace TaskFlow.Platform.Domain.Emails.Services;

public interface ISendEmailService
{
    Task SendEmailAsync(Email message, CancellationToken cancellationToken = default);
}
