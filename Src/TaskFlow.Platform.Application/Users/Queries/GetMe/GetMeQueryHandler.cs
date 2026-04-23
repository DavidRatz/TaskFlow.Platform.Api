using TaskFlow.Platform.Domain.Auth.Entities;
using TaskFlow.Platform.Domain.Users.Entities;
using TaskFlow.Platform.Domain.Users.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Platform.Application.Common.Exceptions;
using TaskFlow.Platform.Application.Users.Dtos;

namespace TaskFlow.Platform.Application.Users.Queries.GetMe;

public sealed class GetMeQueryHandler(
    UserManager<ApplicationUser> userManager,
    IUserProfileRepository userProfileRepository)
    : IRequestHandler<GetMeQuery, UserDto>
{
    public async Task<UserDto> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var identityUser = await userManager.FindByIdAsync(request.UserId.ToString())
                           ?? throw new UnauthorizedAuthException();

        var profile = await userProfileRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken)
                      ?? throw new UnauthorizedAuthException();

        return ToUserDto(identityUser, profile);
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
