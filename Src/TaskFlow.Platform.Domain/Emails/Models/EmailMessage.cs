namespace TaskFlow.Platform.Domain.Emails.Models;

public sealed class EmailMessage
{
    public string Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string From { get; set; }
    public string To { get; set; }
    public DateTime Date { get; set; }
    public string Body { get; set; }
    public bool IsRead { get; set; }
    public List<string> Attachments { get; set; }
}
