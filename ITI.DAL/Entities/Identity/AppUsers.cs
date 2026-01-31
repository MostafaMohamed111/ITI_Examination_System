using Microsoft.AspNetCore.Identity;

namespace ITI.DAL.Entities.Identity;

public sealed class AppUsers : IdentityUser<Guid>
{
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}
