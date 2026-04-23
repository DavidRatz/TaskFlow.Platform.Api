using TaskFlow.Platform.Domain.Auth.Entities;
using TaskFlow.Platform.Domain.Users.Repositories;
using TaskFlow.Platform.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Platform.Application.Users.Dtos;

namespace TaskFlow.Platform.Application.Users.Queries.GetAllUsers;

public sealed class GetAllUsersQueryHandler(
    IUserProfileRepository userProfileRepository,
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<GetAllUsersQuery, PagedList<UserDto>>
{
    public async Task<PagedList<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var pagedUsers = await userProfileRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Search,
            request.SortBy,
            request.Asc,
            cancellationToken);

        var userDtos = new List<UserDto>(pagedUsers.Data.Count);

        foreach (var user in pagedUsers.Data)
        {
            var identityUser = user.IdentityUser ?? await userManager.FindByIdAsync(user.Id.ToString());

            userDtos.Add(new UserDto(
                user.Id,
                user.CreatedAt,
                user.UpdatedAt,
                identityUser?.Email,
                user.FirstName,
                user.LastName,
                user.Phone,
                user.AddressId,
                user.Address is not null ? new AddressDto(user.Address.Id, user.Address.Street, user.Address.City, user.Address.PostalCode, user.Address.Country) : null,
                user.LegalName,
                user.VatNumber));
        }

        return new PagedList<UserDto>(userDtos, pagedUsers.Page, pagedUsers.PageSize, pagedUsers.TotalCount);
    }
}
