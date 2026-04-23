namespace TaskFlow.Platform.Domain.Emails.Models;

public enum EmailProvider
{
    Outlook,
    Gmail,
    ICloud
}

public sealed class EmailConnectorConfig
{
    public EmailProvider Provider { get; set; }
    public string ImapServer { get; set; }
    public int ImapPort { get; set; } = 993;
    public string Username { get; set; }
    public string Password { get; set; }
}
