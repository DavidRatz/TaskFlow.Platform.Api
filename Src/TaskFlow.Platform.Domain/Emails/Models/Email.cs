namespace TaskFlow.Platform.Domain.Emails.Models;

public sealed class Email
{
    public required string From { get; set; }

    public required string To { get; set; }

    public required string Subject { get; set; }

    public required string Message { get; set; }

    public List<EmailAttachment> Attachment { get; set; } = [];

    public void AddAttachment(EmailAttachment attachment)
    {
        Attachment.Add(attachment);
    }

    public bool HasDownloadableAttachments()
    {
        return Attachment.Count > 0;
    }
}
