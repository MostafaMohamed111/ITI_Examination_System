using ITI.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ITI.DAL.Data.IdentityDbContext;


public sealed class ApplicationUsersDbContext : IdentityDbContext<AppUsers,IdentityRole<Guid>,Guid>
{
    public ApplicationUsersDbContext(DbContextOptions<ApplicationUsersDbContext> options) :base(options)
    {
        
    }
}
