using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using TaskFlow.Platform.Domain.Emails.Models;
using TaskFlow.Platform.Domain.Emails.Services;
using TaskFlow.Platform.Infrastructure.Emails.Options;

namespace TaskFlow.Platform.Infrastructure.Emails.Services;

public class OutlookTokenProvider(IOptions<MailSecretOptions> options) : ITokenProvider
{
    public string Provider => "Outlook";

    public async Task<List<EmailMessage>> GetEmailsFromProviderAsync(int maxEmails, CancellationToken cancellationToken)
    {
        var emails = new List<EmailMessage>();
        var interactiveCredential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
        {
            ClientId = options.Value.Outlook.ClientId,
            TenantId = "common", // for personal account
            RedirectUri = new Uri("http://localhost") // Must match app registration
        });
        var scopes = new[] { "User.Read", "Mail.Read" };

        GraphServiceClient graphClient = new GraphServiceClient(interactiveCredential, scopes);

        var me = await graphClient.Me.GetAsync();

        var response = await graphClient.Me.MailFolders["Inbox"].Messages.GetAsync(
            config =>
        {
            config.QueryParameters.Top = maxEmails;
            config.QueryParameters.Select = new[]
            {
                "subject",
                "from",
                "receivedDateTime",
                "body",
                "isRead",
                "attachments"
            };
            config.QueryParameters.Orderby = new[] { "receivedDateTime DESC" };
        }, cancellationToken);

        if (response?.Value != null)
        {
            emails = response.Value.Select(m => new EmailMessage()
            {
                Subject = m.Subject ?? "default subject",
                From = m.From?.EmailAddress?.Address ?? "default email",
                Body = m.Body?.Content ?? "default body",
                IsRead = m.IsRead ?? false,
                Date = m.ReceivedDateTime?.DateTime ?? default,
                Attachments = (m.Attachments?.OfType<FileAttachment>().Select(a => a.Name).ToList() ?? [])!
            }).ToList();
        }

        return emails;
    }

    public Task RevokeTokenFromProviderAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
