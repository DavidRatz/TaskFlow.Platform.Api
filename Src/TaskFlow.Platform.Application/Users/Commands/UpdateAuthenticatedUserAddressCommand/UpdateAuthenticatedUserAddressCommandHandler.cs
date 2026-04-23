using TaskFlow.Platform.Domain.Commons;
using TaskFlow.Platform.Domain.Users.Repositories;
using MediatR;
using TaskFlow.Platform.Application.Auth.Exceptions;
using TaskFlow.Platform.Application.Common.Dtos;
using TaskFlow.Platform.Application.Common.Exceptions;
using TaskFlow.Platform.Application.Users.Dtos;

namespace TaskFlow.Platform.Application.Users.Commands.UpdateAuthenticatedUserAddressCommand;

public sealed class UpdateAuthenticatedUserAddressCommandHandler(
    IUserProfileRepository userProfileRepository,
    IUnitOfWork unitOfWork,
    IAddressRepository addressRepository)
    : IRequestHandler<UpdateAuthenticatedUserAddressCommand, AddressDto>
{
    public async Task<AddressDto> Handle(UpdateAuthenticatedUserAddressCommand request, CancellationToken cancellationToken)
    {
        var profile = await userProfileRepository.GetByIdAsync(request.UserId, cancellationToken)
                      ?? throw new UnauthorizedAuthException();
        if (profile.AddressId is null)
        {
            throw new ResourceNotFoundException("L'utilisateur n'a pas d'adresse associée");
        }

        var address = await addressRepository.GetForUpdateAsync(profile.AddressId.Value, cancellationToken)
                      ?? throw new ResourceNotFoundException("Adresse introuvable");
        if (request.Street is not null)
        {
            TrySetOrThrow(() => address.SetStreet(request.Street), "street");
        }

        if (request.City is not null)
        {
            TrySetOrThrow(() => address.SetCity(request.City), "city");
        }

        if (request.PostalCode is not null)
        {
            TrySetOrThrow(() => address.SetPostalCode(request.PostalCode), "postalCode");
        }

        if (request.Country is not null)
        {
            TrySetOrThrow(() => address.SetCountry(request.Country), "country");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AddressDto(address.Id, address.Street, address.City, address.PostalCode, address.Country);
    }

    private static void TrySetOrThrow(Action setter, string field)
    {
        try
        {
            setter();
        }
        catch (Exception ex) when (ex is ArgumentException or ArgumentNullException)
        {
            throw new ValidationFailedException(new[]
            {
                new ValidationErrorDto(ex.Message, "invalid", field),
            });
        }
    }
}
