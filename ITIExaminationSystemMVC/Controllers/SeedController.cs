using ITI.DAL.Data.AppDbContext;
using ITI.DAL.Entities.DomainEntities;
using ITI.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ITIExaminationSystemMVC.Controllers;

[Route("Seed")]
public class SeedController : Controller
{
    private readonly UserManager<AppUsers> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ApplicationDbContext _context;

    public SeedController(UserManager<AppUsers> userManager, RoleManager<IdentityRole<Guid>> roleManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    [Route("")]
    public async Task<IActionResult> Index()
    {
        // 1. Create Roles
        string[] roles = { "Admin", "Instructor", "Student" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        // 2. Create Default Instructor User
        var instructorEmail = "instructor@test.com";
        var instructorUser = await _userManager.FindByEmailAsync(instructorEmail);
        
        if (instructorUser == null)
        {
            instructorUser = new AppUsers { UserName = instructorEmail, Email = instructorEmail };
            var result = await _userManager.CreateAsync(instructorUser, "Password123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(instructorUser, "Instructor");
            }
        }
        else
        {
            if (!await _userManager.IsInRoleAsync(instructorUser, "Instructor"))
            {
                await _userManager.AddToRoleAsync(instructorUser, "Instructor");
            }
        }

        // Link to Domain Instructor
        var instructor = _context.Instructors.FirstOrDefault(i => i.UserId == instructorUser.Id.ToString());
        if (instructor == null)
        {
            instructor = new Instructor
            {
                Name = "John Instructor",
                // Email property removed as it does not exist on Instructor entity
                Degree = "PhD",
                Salary = 5000,
                UserId = instructorUser.Id.ToString()
            };
            _context.Instructors.Add(instructor);
            await _context.SaveChangesAsync();
        }

        // 3. Create Admin User
        var adminEmail = "admin@test.com";
        var adminUser = await _userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new AppUsers { UserName = adminEmail, Email = adminEmail };
            var result = await _userManager.CreateAsync(adminUser, "Password123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        return Content($"Seeding Complete.\nInstructor: {instructorEmail} / Password123!\nAdmin: {adminEmail} / Password123!");
    }
}
