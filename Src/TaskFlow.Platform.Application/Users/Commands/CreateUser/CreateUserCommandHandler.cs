using TaskFlow.Platform.Domain.Auth.Entities;
using TaskFlow.Platform.Domain.Commons;
using TaskFlow.Platform.Domain.Users.Entities;
using TaskFlow.Platform.Domain.Users.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Platform.Application.Auth.Exceptions;
using TaskFlow.Platform.Application.Users.Dtos;

namespace TaskFlow.Platform.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    IUserProfileRepository userProfileRepository,
    IAddressRepository addressRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
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

        if (request.Address is not null)
        {
            var address = new Address(
                Guid.NewGuid(),
                request.Address.Street,
                request.Address.City,
                request.Address.PostalCode,
                request.Address.Country);
            await addressRepository.AddAsync(address, cancellationToken);
            profile.SetAddress(address);
        }

        await userProfileRepository.AddAsync(profile, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

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
