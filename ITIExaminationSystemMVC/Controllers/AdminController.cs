using ITI.DAL.Data.AppDbContext;
using ITI.DAL.Entities.DomainEntities;
using ITI.DAL.Entities.Identity;
using ITIExaminationSystemMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITIExaminationSystemMVC.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUsers> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<AppUsers> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        // Dashboard Stats
        ViewBag.InstructorCount = await _context.Instructors.CountAsync();
        ViewBag.StudentCount = await _context.Students.CountAsync();
        ViewBag.CourseCount = await _context.Courses.CountAsync();
        ViewBag.BranchCount = await _context.Branches.CountAsync();
        ViewBag.AdminName = user.UserName ?? "Admin";
        
        return View();
    }

    public async Task<IActionResult> Instructors(string searchString, string searchDegree)
    {
        var instructors = _context.Instructors.AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            instructors = instructors.Where(s => s.Name!.Contains(searchString));
        }
        
        if (!string.IsNullOrEmpty(searchDegree))
        {
            instructors = instructors.Where(s => s.Degree!.Contains(searchDegree));
        }

        return View(await instructors.ToListAsync());
    }

    [HttpGet]
    public IActionResult CreateInstructor()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateInstructor(Instructor instructor, string email, string password)
    {
        // Note: For a real app, use a ViewModel. Using Entity directly for speed as per instructions/scaffold style.
        if (ModelState.IsValid)
        {
            // 1. Create Identity User
            var user = new AppUsers { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Instructor");

                // 2. Link User
                instructor.UserId = user.Id.ToString();
                
                _context.Instructors.Add(instructor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Instructors));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        return View(instructor);
    }

    [HttpGet]
    public async Task<IActionResult> DeleteInstructor(int? id)
    {
        if (id == null) return NotFound();
        var instructor = await _context.Instructors.FindAsync(id);
        if (instructor == null) return NotFound();
        return View(instructor);
    }

    [HttpPost, ActionName("DeleteInstructor")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Use Stored Procedure for deletion
        await _context.Database.ExecuteSqlRawAsync("EXEC sp_DeleteInstructor @id = {0}", id);
        return RedirectToAction(nameof(Instructors));
    }

    [HttpGet]
    public async Task<IActionResult> AssignCourse(int id)
    {
        var instructor = await _context.Instructors
            .Include(i => i.Crs)
            .FirstOrDefaultAsync(i => i.Id == id);
            
        if (instructor == null) return NotFound();

        // Get courses not already assigned to this instructor
        var assignedCourseIds = instructor.Crs.Select(c => c.Id).ToList();
        var availableCourses = await _context.Courses
            .Where(c => !assignedCourseIds.Contains(c.Id))
            .ToListAsync();

        ViewBag.InstructorId = id;
        ViewBag.InstructorName = instructor.Name;
        ViewBag.Courses = availableCourses;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AssignCourse(int instructorId, int courseId)
    {
        var instructor = await _context.Instructors.FindAsync(instructorId);
        var course = await _context.Courses.FindAsync(courseId);

        if (instructor != null && course != null)
        {
            // Manual insert into joining table or EF collection add
            // Because creating a joining entity object might be needed if mapped explicitly
            // Instructor.cs has `public virtual ICollection<Course> Crs { get; set; }`
            // EF Core should handle many-to-many link if configured correctly, 
            // but let's check basic mapping. 
            // The mapping might be explicit via `Instructor_Course` table in DB context.
            // If explicit many-to-many not configured in EF 5+ style (skip nav), need to use raw SQL or joining entity if exposed.
            // Looking at Context... ApplicationDbContext lines 114-134 showed `Instructor_Course` table manually mapped in migration.
            // But let's try EF add first.
            
            instructor.Crs.Add(course);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Instructors");
    }
}
