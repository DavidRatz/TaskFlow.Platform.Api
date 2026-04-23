namespace TaskFlow.Platform.Infrastructure.Emails.Options;

public class MailServerOptions
{
    public const string SectionName = "MailServer";

    public required string Host { get; init; }

    public required int Port { get; init; }

    public required bool UseSsl { get; init; }

    public required string Username { get; init; }

    public required string Password { get; init; }
}
