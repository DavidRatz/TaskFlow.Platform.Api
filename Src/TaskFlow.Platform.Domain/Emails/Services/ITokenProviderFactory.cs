namespace TaskFlow.Platform.Domain.Emails.Services;

public interface ITokenProviderFactory
{
    ITokenProvider Resolve(string provider);
}
