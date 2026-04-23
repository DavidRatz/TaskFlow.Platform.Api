namespace TaskFlow.Platform.Presentation.Tasks.Requests;

public record CreateTaskFromEmailRequest(
    string Subject,
    string Message,
    string Email
);
