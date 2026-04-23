namespace TaskFlow.Platform.Domain.Emails.Models;

public class OAuthToken
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Provider { get; set; }
    public string Email { get; set; }
}
