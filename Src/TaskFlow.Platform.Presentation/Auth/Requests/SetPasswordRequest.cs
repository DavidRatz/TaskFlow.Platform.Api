namespace TaskFlow.Platform.Presentation.Auth.Requests;

public sealed record SetPasswordRequest(string Token, string Password);
