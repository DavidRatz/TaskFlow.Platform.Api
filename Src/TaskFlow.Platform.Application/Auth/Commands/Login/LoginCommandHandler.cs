using TaskFlow.Platform.Domain.Auth.Entities;
using TaskFlow.Platform.Domain.Authentication.Services;
using TaskFlow.Platform.Domain.Users.Entities;
using TaskFlow.Platform.Domain.Users.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Platform.Application.Auth.Exceptions;
using TaskFlow.Platform.Application.Users.Dtos;

namespace TaskFlow.Platform.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    UserManager<ApplicationUser> userManager,
    IUserProfileRepository userProfileRepository,
    IJwtTokenService jwtTokenService)
    : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email)
                   ?? throw new InvalidCredentialsException();
        var ok = await userManager.CheckPasswordAsync(user, request.Password);
        if (!ok)
        {
            throw new InvalidCredentialsException();
        }

        var token = jwtTokenService.CreateToken(user);

        var profile = await userProfileRepository.GetByIdWithRolesAsync(user.Id, cancellationToken)
                      ?? throw new InvalidCredentialsException();

        return new LoginResult(ToUserDto(user, profile), token.Token);
    }

    private static UserDto ToUserDto(ApplicationUser identityUser, User profile)
    {
        return new UserDto(
            identityUser.Id,
            profile.CreatedAt,
            profile.UpdatedAt,
            identityUser.Email,
            profile.FirstName,
            profile.LastName,
            profile.Phone,
            profile.AddressId,
            profile.Address is not null ? new AddressDto(profile.Address.Id, profile.Address.Street, profile.Address.City, profile.Address.PostalCode, profile.Address.Country) : null,
            profile.LegalName,
            profile.VatNumber);
    }
}
