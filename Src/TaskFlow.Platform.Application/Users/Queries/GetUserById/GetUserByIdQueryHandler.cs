using TaskFlow.Platform.Domain.Auth.Entities;
using TaskFlow.Platform.Domain.Users.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Platform.Application.Common.Exceptions;
using TaskFlow.Platform.Application.Users.Dtos;

namespace TaskFlow.Platform.Application.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler(
    UserManager<ApplicationUser> userManager,
    IUserProfileRepository userProfileRepository)
    : IRequestHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var identityUser = await userManager.FindByIdAsync(request.UserId.ToString())
                           ?? throw new ResourceNotFoundException("User not found");

        var profile = await userProfileRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken)
                      ?? throw new ResourceNotFoundException("User not found");

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
