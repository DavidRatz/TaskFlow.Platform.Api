using TaskFlow.Platform.Domain.Authentication.Services;
using MediatR;
using TaskFlow.Platform.Application.Common.Exceptions;
using TaskFlow.Platform.Domain.Emails.Services;

namespace TaskFlow.Platform.Application.Auth.Commands.Logout;

public sealed class LogoutCommandHandler(
    ITokenRevocationService tokenRevocationService,
    ILogoutTokenInfoProvider logoutTokenInfoProvider,
    IEmailConnector emailConnector)
    : IRequestHandler<LogoutCommand>
{
    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (!logoutTokenInfoProvider.TryGet(request.UserId, request.Jti, request.AuthorizationHeader, out var tokenInfo))
        {
            throw new UnauthorizedAuthException();
        }

        try
        {
            await emailConnector.RevokeTokenAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        await tokenRevocationService.RevokeAsync(tokenInfo.Jti, tokenInfo.UserId, tokenInfo.ExpiresAt, cancellationToken);
        return Unit.Value;
    }
}
