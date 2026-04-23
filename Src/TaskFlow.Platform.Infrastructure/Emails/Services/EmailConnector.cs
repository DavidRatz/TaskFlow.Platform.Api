using MailKit.Net.Imap;
using MailKit;
using MailKit.Security;
using TaskFlow.Platform.Domain.Emails.Models;
using TaskFlow.Platform.Domain.Emails.Services;

using Microsoft.Identity.Client;

namespace TaskFlow.Platform.Infrastructure.Emails.Services;

public class EmailConnector(ITokenProviderFactory factory) : IEmailConnector
{
    public async Task<List<EmailMessage>> GetEmailsAsync(string provider, int maxEmails, CancellationToken cancellationToken)
    {
        var tokenProvider = factory.Resolve(provider);

        return await tokenProvider.GetEmailsFromProviderAsync(maxEmails, cancellationToken);
    }

    public async Task<EmailMessage?> GetEmailByIdAsync(EmailConnectorConfig config, uint emailId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new ImapClient();
            await client.ConnectAsync(config.ImapServer, config.ImapPort, SecureSocketOptions.SslOnConnect, cancellationToken);
            await client.AuthenticateAsync(config.Username, config.Password, cancellationToken);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

            var uniqueId = new UniqueId(emailId);
            var message = await inbox.GetMessageAsync(uniqueId, cancellationToken);

            if (message == null)
            {
                return null;
            }

            ////var summary = await inbox.FetchAsync(uniqueId, MessageSummaryItems.Flags, cancellationToken);
            ////var flags = summary.FirstOrDefault()?.Flags;

            var emailMessage = new EmailMessage
            {
                Id = uniqueId.ToString(),
                Subject = message.Subject ?? string.Empty,
                From = message.From.ToString(),
                To = string.Join(",", message.To.Select(a => a.ToString())),
                Date = message.Date.DateTime,
                Body = message.TextBody ?? message.HtmlBody ?? string.Empty,
                ////IsRead = flags?.HasFlag(MessageFlags.Seen) == false,
                IsRead = false,
                Attachments = message.Attachments.Select(a => a.ContentDisposition?.FileName ?? "Unknown").ToList()
            };

            await client.DisconnectAsync(true, cancellationToken);
            return emailMessage;
        }
        catch
        {
            return null;
        }
    }

    public async Task RevokeTokenAsync(CancellationToken cancellationToken = default)
    {
        var tokenProvider = factory.Resolve("Gmail");
        await tokenProvider.RevokeTokenFromProviderAsync(cancellationToken);
    }

    private static async Task<AuthenticationResult> GetConfidentialClientOAuth2CredentialsAsync(string protocol, CancellationToken cancellationToken = default)
    {
        var clientId = "38b2559d-1cd8-4171-b3c5-d64f1c34feb2";
        var tenantId = "1d0552d2-0c58-4fce-9743-b6efe3510ab7";
        var clientSecret = "P8g8Q~fVrQLk93zVcOF66XVXr2oRsCM.Hajf9bTZ";
        var confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(clientId)
            .WithAuthority($"https://login.microsoftonline.com/{tenantId}/v2.0")
            .WithClientSecret(clientSecret)
            .Build();

        string[] scopes;

        if (protocol.Equals("SMTP", StringComparison.OrdinalIgnoreCase))
        {
            scopes =
            [
                //// For SMTP, use the following scope
                "https://outlook.office365.com/.default"
            ];
        }
        else
        {
            scopes =
            [
                //// For IMAP and POP3, use the following scope
                "https://ps.outlook.com/.default"
            ];
        }

        return await confidentialClientApplication.AcquireTokenForClient(scopes).ExecuteAsync(cancellationToken);
    }
}
