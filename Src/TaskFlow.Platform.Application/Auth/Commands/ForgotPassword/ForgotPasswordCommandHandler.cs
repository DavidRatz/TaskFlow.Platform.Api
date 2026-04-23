using TaskFlow.Platform.Domain.Auth.Entities;
using TaskFlow.Platform.Domain.Emails.Models;
using TaskFlow.Platform.Domain.Emails.Services;
using TaskFlow.Platform.Domain.Users.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace TaskFlow.Platform.Application.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler(
    UserManager<ApplicationUser> userManager,
    IUserProfileRepository userProfileRepository,
    IEmailService emailService,
    ILogger<ForgotPasswordCommandHandler> logger)
    : IRequestHandler<ForgotPasswordCommand>
{
    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            // Don't reveal that the user does not exist
            logger.LogWarning("Forgot password requested for non-existent email: {Email}", request.Email);
            return Unit.Value;
        }

        var identityToken = await userManager.GeneratePasswordResetTokenAsync(user);

        // Create composite token: base64(userId:identityToken)
        var compositeToken = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes($"{user.Id}:{identityToken}"));

        var resetUrl = $"http://localhost:3000/dashboard/reset-password?token={Uri.EscapeDataString(compositeToken)}";

        var profile = await userProfileRepository.GetByIdAsync(user.Id, cancellationToken);
        var firstName = profile?.FirstName ?? "Utilisateur";

        await emailService.SendEmailAsync(
            "ResetPasswordMail",
            new ResetPasswordMail
            {
                FirstName = firstName,
                ResetUrl = resetUrl,
            },
            user.Email!,
            "Réinitialisation de votre mot de passe - CarWashFlow",
            cancellationToken);

        return Unit.Value;
    }
}
