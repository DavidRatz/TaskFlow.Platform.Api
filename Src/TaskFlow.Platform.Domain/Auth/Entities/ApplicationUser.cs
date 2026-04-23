using Microsoft.AspNetCore.Identity;
using TaskFlow.Platform.Domain.Users.Entities;

namespace TaskFlow.Platform.Domain.Auth.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public User? User { get; set; }
}
