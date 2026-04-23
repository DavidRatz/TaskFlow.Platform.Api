namespace TaskFlow.Platform.Presentation.Emails.Requests;

public sealed record GetEmailRequest(
    string Username,
    string Password,
    string Provider,
    int? MaxEmails);
