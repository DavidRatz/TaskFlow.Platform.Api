using TaskFlow.Platform.Domain.Auth.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Platform.Application.Common.Exceptions;

namespace TaskFlow.Platform.Application.Users.Commands.DeleteUser;

public sealed class DeleteUserCommandHandler(
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<DeleteUserCommand>
{
    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var identityUser = await userManager.FindByIdAsync(request.UserId.ToString())
                           ?? throw new ResourceNotFoundException("User not found");

        var deleteResult = await userManager.DeleteAsync(identityUser);
        if (!deleteResult.Succeeded)
        {
            throw new BadRequestException("Failed to delete user");
        }

        return Unit.Value;
    }
}
