using TaskFlow.Platform.Domain.Commons;
using TaskFlow.Platform.Domain.Users.Entities;
using TaskFlow.Platform.Domain.Users.Repositories;
using MediatR;
using TaskFlow.Platform.Application.Auth.Exceptions;
using TaskFlow.Platform.Application.Common.Dtos;
using TaskFlow.Platform.Application.Common.Exceptions;
using TaskFlow.Platform.Application.Users.Dtos;

namespace TaskFlow.Platform.Application.Users.Commands.CreateAuthenticatedUserAddress;

public sealed class CreateAuthenticatedUserAddressCommandHandler(
    IUserProfileRepository userProfileRepository,
    IAddressRepository addressRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAuthenticatedUserAddressCommand, AddressDto>
{
    public async Task<AddressDto> Handle(CreateAuthenticatedUserAddressCommand request, CancellationToken cancellationToken)
    {
        var profile = await userProfileRepository.GetForUpdateAsync(request.UserId, cancellationToken) ?? throw new UnauthorizedAuthException();
        Address address;
        try
        {
            address = new Address(
                Guid.NewGuid(),
                request.Street,
                request.City,
                request.PostalCode,
                request.Country);
        }
        catch (ArgumentNullException ex)
        {
            ThrowDomainValidationAs422(ex);
            throw;
        }
        catch (ArgumentException ex)
        {
            ThrowDomainValidationAs422(ex);
            throw;
        }

        await addressRepository.AddAsync(address, cancellationToken);
        profile.SetAddress(address);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AddressDto(address.Id, address.Street, address.City, address.PostalCode, address.Country);
    }

    private static void ThrowDomainValidationAs422(Exception ex)
    {
        var param = (ex as ArgumentException)?.ParamName;
        var field = param is null ? "address" : $"{char.ToLowerInvariant(param[0])}{param.Substring(1)}";

        throw new ValidationFailedException(new[]
        {
            new ValidationErrorDto(ex.Message, "invalid", field),
        });
    }
}
