using TaskFlow.Platform.Domain.Auth.Entities;
using TaskFlow.Platform.Domain.Authentication.Services;
using TaskFlow.Platform.Domain.Commons;
using TaskFlow.Platform.Domain.Users.Entities;
using TaskFlow.Platform.Domain.Users.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Platform.Application.Auth.Dtos;
using TaskFlow.Platform.Application.Auth.Exceptions;
using TaskFlow.Platform.Application.Users.Dtos;

namespace TaskFlow.Platform.Application.Auth.Commands.Register;

public sealed class RegisterCommandHandler(
    IUserProfileRepository userProfileRepository,
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterCommand, RegisterResultDto>
{
    public async Task<RegisterResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var address = new Address(
            Guid.NewGuid(),
            request.Address.Street,
            request.Address.City,
            request.Address.PostalCode,
            request.Address.Country);

        var identityUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
        };

        var createResult = await userManager.CreateAsync(identityUser, request.Password);
        if (!createResult.Succeeded)
        {
            throw new RegistrationFailedException(createResult.Errors.Select(x => x.Description).ToArray());
        }

        var profile = new User(identityUser.Id, request.FirstName, request.LastName);
        profile.SetPhone(request.Phone);
        profile.SetAddress(address);
        profile.SetLegalName(request.LegalName);
        profile.SetVatNumber(request.VatNumber);

        await userProfileRepository.AddAsync(profile, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish event for async processing (Stripe customer creation + welcome email)
        /*var userRegisteredEvent = new UserRegisteredEvent
        {
            UserId = profile.Id,
            Email = identityUser.Email ?? string.Empty,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Phone = profile.Phone,
            LegalName = profile.LegalName,
            VatNumber = profile.VatNumber,
            Address = new UserRegisteredAddressDto(
                address.Street,
                address.City,
                address.PostalCode,
                address.Country),
        };

        await bus.SendLocal(userRegisteredEvent);*/

        var token = jwtTokenService.CreateToken(identityUser);

        return new RegisterResultDto(ToUserDto(identityUser, profile), token.Token);
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
