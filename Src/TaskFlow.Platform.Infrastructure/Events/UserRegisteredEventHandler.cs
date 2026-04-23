using TaskFlow.Platform.Domain.Emails.Enums;
using TaskFlow.Platform.Domain.Emails.Models;
using TaskFlow.Platform.Domain.Emails.Services;
using TaskFlow.Platform.Domain.Events;
using Rebus.Handlers;

namespace TaskFlow.Platform.Infrastructure.Events;

public sealed class UserRegisteredEventHandler(
    IEmailService emailService)
    : IHandleMessages<UserRegisteredEvent>
{
    public async Task Handle(UserRegisteredEvent message)
    {
        var cancellationToken = CancellationToken.None;

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "id", message.UserId.ToString() },
            { "type", "user" },
        };

        if (!string.IsNullOrWhiteSpace(message.LegalName))
        {
            metadata["legalName"] = message.LegalName;
        }

        if (!string.IsNullOrWhiteSpace(message.VatNumber))
        {
            metadata["vatNumber"] = message.VatNumber;
        }

        // 3. Send welcome email
        await emailService.SendEmailAsync(
            nameof(EmailTemplateType.WelcomeMail),
            new WelcomeMail
            {
                FirstName = message.FirstName,
                LastName = message.LastName,
                Email = message.Email,
            },
            message.Email,
            "Bienvenue sur TaskFlow !",
            cancellationToken);
    }
}
