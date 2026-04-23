namespace TaskFlow.Platform.Infrastructure.Emails.Options;

public sealed class MailSecretOptions
{
    public const string SectionName = "MailSecret";

    public MailProvider Gmail { get; set; }
    public MailProvider Outlook { get; set; }
}

public sealed class MailProvider
{
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
}
