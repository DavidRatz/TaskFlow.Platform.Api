using TaskFlow.Platform.Domain.Commons;
using TaskFlow.Platform.Domain.Users.Repositories;
using MediatR;
using TaskFlow.Platform.Application.Common.Exceptions;

namespace TaskFlow.Platform.Application.Users.Commands.DeleteAuthenticatedUserAddressCommand;

public sealed class DeleteAuthenticatedUserAddressCommandHandler(
    IUserProfileRepository userProfileRepository,
    IUnitOfWork unitOfWork,
    IAddressRepository addressRepository)
    : IRequestHandler<DeleteAuthenticatedUserAddressCommand>
{
    public async Task<Unit> Handle(DeleteAuthenticatedUserAddressCommand request, CancellationToken cancellationToken)
    {
        var profile = await userProfileRepository.GetForUpdateAsync(request.UserId, cancellationToken) ?? throw new UnauthorizedAuthException();
        if (profile.AddressId is null)
        {
            throw new ResourceNotFoundException("L'utilisateur n'a pas d'adresse associée");
        }

        var address = await addressRepository.GetForUpdateAsync(profile.AddressId.Value, cancellationToken) ?? throw new ResourceNotFoundException("Adresse introuvable");
        addressRepository.Remove(address);

        profile.SetAddress(null);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
