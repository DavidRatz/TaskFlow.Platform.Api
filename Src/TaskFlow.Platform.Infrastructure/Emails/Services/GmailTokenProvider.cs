using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Util.Store;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using Microsoft.Extensions.Options;
using TaskFlow.Platform.Domain.Emails.Models;
using TaskFlow.Platform.Domain.Emails.Services;
using TaskFlow.Platform.Infrastructure.Emails.Options;

namespace TaskFlow.Platform.Infrastructure.Emails.Services;

public class GmailTokenProvider(IOptions<MailSecretOptions> options, IOptions<EmailOptions> optionsMail) : ITokenProvider
{
    public string Provider => "Gmail";

    /**
     * Tache de récupération des emails de Gmail via le mail dans app.settings et les identifiants de l'app google cloud
     */
    public async Task<List<EmailMessage>> GetEmailsFromProviderAsync(int maxEmails, CancellationToken cancellationToken)
    {
        var messages = new List<EmailMessage>();
        var email = optionsMail.Value.Gmail.MailAddress;
        var credential = GetCredentialAsync(email).Result;

        /*var secrets = new ClientSecrets
        {
            ClientId = configuration.GetSection("MailSecret:Gmail:ClientId").Value,
            ClientSecret = configuration.GetSection("MailSecret:Gmail:ClientSecret").Value
        };

        var codeFlow = new GoogleAuthorizationCodeFlow(
            new GoogleAuthorizationCodeFlow.Initializer
            {
                DataStore = new FileDataStore(tokenFolder, fullPath: true),
                Scopes = ["https://mail.google.com/"],
                ClientSecrets = secrets,
                LoginHint = email
            });

        //// On first run: opens browser. On subsequent runs: uses cached token silently.
        var authCode = new AuthorizationCodeInstalledApp(codeFlow, new LocalServerCodeReceiver());
        var credential = await authCode.AuthorizeAsync(email, CancellationToken.None);*/
        if (credential.Token.IsStale)
        {
            await credential.RefreshTokenAsync(CancellationToken.None);
        }

        var oauth2 = new SaslMechanismOAuth2(email, credential.Token.AccessToken);

        using var client = new ImapClient(new ProtocolLogger(Console.OpenStandardOutput()));
        await client.ConnectAsync("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect, cancellationToken);

        await client.AuthenticateAsync(oauth2, cancellationToken);

        var inbox = client.Inbox;
        await inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

        var uids = await inbox.FetchAsync(0, maxEmails - 1, MessageSummaryItems.UniqueId | MessageSummaryItems.Flags, cancellationToken);
        foreach (var uid in uids.Reverse())
        {
            var message = await inbox.GetMessageAsync(uid.UniqueId, cancellationToken);
            messages.Add(new EmailMessage
            {
                Id = uid.UniqueId.ToString(),
                Subject = message.Subject ?? string.Empty,
                From = message.From.ToString(),
                To = message.To.ToString(),
                Date = message.Date.DateTime,
                Body = message.TextBody ?? message.HtmlBody ?? string.Empty,
                IsRead = uid.Flags?.HasFlag(MessageFlags.Seen) == false,
                Attachments = message.Attachments.Select(a => a.ContentDisposition?.FileName ?? "Unknown").ToList()
            });
        }

        await client.DisconnectAsync(true, cancellationToken);

        return messages;
    }

    /**
     * Tache suppression du token gmail
     */
    public async Task RevokeTokenFromProviderAsync(CancellationToken cancellationToken = default)
    {
        var email = optionsMail.Value.Gmail.MailAddress;
        var credential = GetCredentialAsync(email).Result;
        await credential.RevokeTokenAsync(CancellationToken.None);

        var dataStore = new FileDataStore("TaskFlow", true);
        await dataStore.DeleteAsync<string>(email);
    }

    /**
     * Tache création token via les identifiants dans app.settings
     */
    private async Task<UserCredential> GetCredentialAsync(string email)
    {
        var clientId = options.Value.Gmail.ClientId;
        var clientSecret = options.Value.Gmail.ClientSecret;

        ////const string tokenFolder = "/data/token-cache";   // mounted volume in Docker
        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            },
            new[] { GmailService.Scope.MailGoogleCom },
            email,
            CancellationToken.None,
            new FileDataStore("TaskFlow", true));
        return credential;
    }
}
