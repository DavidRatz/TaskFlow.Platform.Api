using TaskFlow.Platform.Domain.Utils;
using MediatR;
using TaskFlow.Platform.Application.Users.Dtos;

namespace TaskFlow.Platform.Application.Users.Queries.GetAllUsers;

public sealed record GetAllUsersQuery(int Page, int PageSize, string? Search, string? SortBy, bool? Asc) : IRequest<PagedList<UserDto>>;
