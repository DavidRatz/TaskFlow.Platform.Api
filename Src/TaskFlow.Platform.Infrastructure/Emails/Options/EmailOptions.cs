namespace TaskFlow.Platform.Infrastructure.Emails.Options;

public class EmailOptions
{
    public const string SectionName = "Email";

    public MailAddressProvider Gmail { get; set; }
    public MailAddressProvider Outlook { get; set; }
}

public sealed class MailAddressProvider
{
    public required string MailAddress { get; set; }
}
