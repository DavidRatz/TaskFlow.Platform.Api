using TaskFlow.Platform.Domain.Auth.Entities;
using TaskFlow.Platform.Domain.Authentication.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Platform.Application.Common.Exceptions;

namespace TaskFlow.Platform.Application.Users.Commands.DeleteMe;

public sealed class DeleteMeCommandHandler(
    UserManager<ApplicationUser> userManager,
    ITokenRevocationService tokenRevocationService,
    ILogoutTokenInfoProvider logoutTokenInfoProvider)
    : IRequestHandler<DeleteMeCommand>
{
    public async Task<Unit> Handle(DeleteMeCommand request, CancellationToken cancellationToken)
    {
        if (!logoutTokenInfoProvider.TryGet(request.UserId, request.Jti, request.AuthorizationHeader, out var tokenInfo))
        {
            throw new UnauthorizedAuthException();
        }

        var identityUser = await userManager.FindByIdAsync(tokenInfo.UserId.ToString());
        if (identityUser is null)
        {
            throw new UnauthorizedAuthException();
        }

        var deleteResult = await userManager.DeleteAsync(identityUser);
        if (!deleteResult.Succeeded)
        {
            throw new UnauthorizedAuthException();
        }

        await tokenRevocationService.RevokeAsync(tokenInfo.Jti, tokenInfo.UserId, tokenInfo.ExpiresAt, cancellationToken);

        return Unit.Value;
    }
}
