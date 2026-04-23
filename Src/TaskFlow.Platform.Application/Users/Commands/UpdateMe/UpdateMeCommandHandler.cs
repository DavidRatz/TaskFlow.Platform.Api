using TaskFlow.Platform.Domain.Auth.Entities;
using TaskFlow.Platform.Domain.Commons;
using TaskFlow.Platform.Domain.Users.Entities;
using TaskFlow.Platform.Domain.Users.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Platform.Application.Auth.Exceptions;
using TaskFlow.Platform.Application.Common.Dtos;
using TaskFlow.Platform.Application.Common.Exceptions;
using TaskFlow.Platform.Application.Users.Dtos;

namespace TaskFlow.Platform.Application.Users.Commands.UpdateMe;

public sealed class UpdateMeCommandHandler(
    UserManager<ApplicationUser> userManager,
    IUserProfileRepository userProfileRepository,
    IUnitOfWork unitOfWork,
    IAddressRepository addressRepository)
    : IRequestHandler<UpdateMeCommand, UserDto>
{
    public async Task<UserDto> Handle(UpdateMeCommand request, CancellationToken cancellationToken)
    {
        var identityUser = await userManager.FindByIdAsync(request.UserId.ToString())
                           ?? throw new UnauthorizedAuthException();

        var profile = await userProfileRepository.GetForUpdateWithRolesAsync(request.UserId, cancellationToken)
                      ?? throw new UnauthorizedAuthException();

        if (request.FirstName is not null)
        {
            TrySetOrThrow(() => profile.SetFirstName(request.FirstName), "firstName");
        }

        if (request.LastName is not null)
        {
            TrySetOrThrow(() => profile.SetLastName(request.LastName), "lastName");
        }

        if (request.Phone is not null)
        {
            TrySetOrThrow(() => profile.SetPhone(request.Phone), "phone");
        }

        if (request.LegalName is not null && request.LegalName.Trim().Length != 0)
        {
            TrySetOrThrow(() => profile.SetLegalName(request.LegalName), "legalName");
        }

        if (request.VatNumber is not null && request.VatNumber.Trim().Length != 0)
        {
            TrySetOrThrow(() => profile.SetVatNumber(request.VatNumber), "vatNumber");
        }

        if (request.Address is not null)
        {
            await UpsertAddressAsync(profile, request.Address, cancellationToken);
        }

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

    private static void TrySetOrThrow(Action setter, string field)
    {
        try
        {
            setter();
        }
        catch (ArgumentNullException ex)
        {
            throw new ValidationFailedException([
                new ValidationErrorDto(ex.Message, "invalid", field)
            ]);
        }
        catch (ArgumentException ex)
        {
            throw new ValidationFailedException([
                new ValidationErrorDto(ex.Message, "invalid", field)
            ]);
        }
    }

    private static void ThrowDomainValidationAs422(Exception ex, string prefix)
    {
        var param = (ex as ArgumentException)?.ParamName;
        var field = param is null ? prefix : $"{prefix}.{char.ToLowerInvariant(param[0])}{param.Substring(1)}";

        throw new ValidationFailedException([
            new ValidationErrorDto(ex.Message, "invalid", field)
        ]);
    }

    private async Task UpsertAddressAsync(User profile, UpdateAuthenticatedUserAddressDto address, CancellationToken cancellationToken)
    {
        if (profile.AddressId is null)
        {
            Address entity;
            try
            {
                entity = new Address(
                    Guid.NewGuid(),
                    address.Street!,
                    address.City!,
                    address.PostalCode!,
                    address.Country!);
            }
            catch (ArgumentNullException ex)
            {
                ThrowDomainValidationAs422(ex, "address");
                throw;
            }
            catch (ArgumentException ex)
            {
                ThrowDomainValidationAs422(ex, "address");
                throw;
            }

            await addressRepository.AddAsync(entity, cancellationToken);
            profile.SetAddress(entity);
            return;
        }

        var existing = await addressRepository.GetForUpdateAsync(profile.AddressId.Value, cancellationToken) ?? throw new ValidationFailedException(new[]
            {
                new ValidationErrorDto("Address not found", "notFound", "address"),
            });
        if (address.Street is not null)
        {
            TrySetOrThrow(() => existing.SetStreet(address.Street), "address.street");
        }

        if (address.City is not null)
        {
            TrySetOrThrow(() => existing.SetCity(address.City), "address.city");
        }

        if (address.PostalCode is not null)
        {
            TrySetOrThrow(() => existing.SetPostalCode(address.PostalCode), "address.postalCode");
        }

        if (address.Country is not null)
        {
            TrySetOrThrow(() => existing.SetCountry(address.Country), "address.country");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
