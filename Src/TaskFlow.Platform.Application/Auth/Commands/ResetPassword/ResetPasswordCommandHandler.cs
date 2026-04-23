using TaskFlow.Platform.Domain.Auth.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Platform.Application.Common.Exceptions;

namespace TaskFlow.Platform.Application.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler(
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<ResetPasswordCommand>
{
    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var (userId, identityToken) = DecodeCompositeToken(request.Token);

        var user = await userManager.FindByIdAsync(userId)
                   ?? throw new BadRequestException("Token invalide ou expiré");

        var result = await userManager.ResetPasswordAsync(user, identityToken, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Impossible de réinitialiser le mot de passe : {errors}");
        }

        return Unit.Value;
    }

    private static (string UserId, string IdentityToken) DecodeCompositeToken(string compositeToken)
    {
        try
        {
            var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(compositeToken));
            var separatorIndex = decoded.IndexOf(':', StringComparison.Ordinal);
            if (separatorIndex < 0)
            {
                throw new BadRequestException("Token invalide ou expiré");
            }

            return (decoded[..separatorIndex], decoded[(separatorIndex + 1)..]);
        }
        catch (FormatException)
        {
            throw new BadRequestException("Token invalide ou expiré");
        }
    }
}
