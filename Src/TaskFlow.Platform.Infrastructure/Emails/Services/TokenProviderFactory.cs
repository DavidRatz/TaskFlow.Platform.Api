using TaskFlow.Platform.Domain.Emails.Services;

namespace TaskFlow.Platform.Infrastructure.Emails.Services;

public sealed class TokenProviderFactory(IEnumerable<ITokenProvider> providers) : ITokenProviderFactory
{
    public ITokenProvider Resolve(string provider)
    {
        var service = providers.FirstOrDefault(p => p.Provider == provider);

        if (service == null)
        {
            throw new NotSupportedException($"Provider {provider} not supported");
        }

        return service;
    }
}
