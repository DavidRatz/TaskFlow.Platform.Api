using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Platform.Application.Tasks.Dtos;
using TaskFlow.Platform.Domain.Auth.Entities;
using TaskFlow.Platform.Domain.Commons;
using TaskFlow.Platform.Domain.Tasks.Entities;
using TaskFlow.Platform.Domain.Tasks.Repositories;

namespace TaskFlow.Platform.Application.Tasks.Commands;

public sealed class CreateTaskFromEmailCommandHandler(
    ITaskEmailRepository taskRepository,
    UserManager<ApplicationUser> userManager,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateTaskFromEmailCommand, TaskDto>
{
    public async Task<TaskDto> Handle(CreateTaskFromEmailCommand request, CancellationToken cancellationToken)
    {
        var task = new TaskEmail
        {
            Title = request.Subject,
            Description = request.Message,
            UserId = userManager.FindByEmailAsync(request.Email).Result?.Id
        };

        await taskRepository.AddAsync(task, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new TaskDto(
            task.Id,
            task.Title,
            task.Description,
            task.UserId,
            task.CreatedAt,
            task.UpdatedAt);
    }
}
