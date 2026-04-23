namespace TaskFlow.Platform.Presentation.Auth.Requests;

public sealed record ResetPasswordRequest(string Token, string Password);
