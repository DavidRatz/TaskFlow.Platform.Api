using TaskFlow.Platform.Domain.Users.Repositories;
using MediatR;
using TaskFlow.Platform.Application.Common.Exceptions;
using TaskFlow.Platform.Application.Users.Dtos;

namespace TaskFlow.Platform.Application.Users.Queries.GetAuthenticatedUserAddress;

public sealed class GetAuthenticatedUserAddressQueryHandler(
    IUserProfileRepository userProfileRepository,
    IAddressRepository addressRepository)
    : IRequestHandler<GetAuthenticatedUserAddressQuery, AddressDto>
{
    public async Task<AddressDto> Handle(GetAuthenticatedUserAddressQuery request, CancellationToken cancellationToken)
    {
        var profile = await userProfileRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (profile is null)
        {
            throw new UnauthorizedAuthException();
        }

        if (profile.AddressId is null)
        {
            throw new ResourceNotFoundException("L'utilisateur n'a pas d'adresse associée");
        }

        var address = await addressRepository.GetByIdAsync(profile.AddressId.Value, cancellationToken);
        if (address is null)
        {
            throw new ResourceNotFoundException("Adresse introuvable");
        }

        return new AddressDto(address.Id, address.Street, address.City, address.PostalCode, address.Country);
    }
}
