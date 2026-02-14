using ITI.DAL.Data.AppDbContext;
using ITI.DAL.Entities.DomainEntities;
using ITI.DAL.Entities.Identity;
using ITIExaminationSystemMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITIExaminationSystemMVC.Controllers;

[Authorize]
public class InstructorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUsers> _userManager;

    public InstructorController(ApplicationDbContext context, UserManager<AppUsers> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> AssignStudentToCourse()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var instructor = await _context.Instructors
            .Include(i => i.Branches)
            .Include(i => i.Crs)
            .FirstOrDefaultAsync(i => i.UserId == user.Id.ToString());

        if (instructor == null) return RedirectToAction("Index");

        // Simplified Strategy:
        // Get students in the Instructor's Branches.
        // This is the pool of students the instructor can manage.
        var branchIds = instructor.Branches.Select(b => b.Id).ToList();
        
        var students = await _context.Students
            .Include(s => s.Track)
            .Where(s => s.BranchId != null && branchIds.Contains(s.BranchId.Value))
            .ToListAsync();

        // If strict branch matching fails (e.g. seed data issues), fallback to ALL students for demonstration
        if (!students.Any())
        {
             students = await _context.Students.Include(s => s.Track).ToListAsync();
        }

        // Filter out students who are ALREADY in the selected course?
        // UI is dynamic (all courses). We can't filter by course yet until one is picked.
        // The View will show all available students.

        var viewModel = new AssignStudentToCourseViewModel
        {
            InstructorId = instructor.Id,
            AvailableCourses = instructor.Crs.ToList(),
            AvailableStudents = students
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AssignStudentToCourse(AssignStudentToCourseViewModel model)
    {
        if (model.SelectedCourseId != 0 && model.SelectedStudentId != 0)
        {
            // Check if already assigned
            var exists = await _context.StudCrs
                .AnyAsync(sc => sc.StdId == model.SelectedStudentId && sc.CrsId == model.SelectedCourseId);

            if (!exists)
            {
                var studCr = new StudCr
                {
                    StdId = model.SelectedStudentId,
                    CrsId = model.SelectedCourseId,
                    Grade = null // Initial grade
                };

                _context.StudCrs.Add(studCr);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Student assigned to course successfully!";
            }
            else
            {
                TempData["Error"] = "Student is already assigned to this course.";
            }

            return RedirectToAction("Index");
        }

        // Reload lists if failed
        var user = await _userManager.GetUserAsync(User);
        var instructor = await _context.Instructors
           .Include(i => i.Branches)
           .Include(i => i.Crs)
           .FirstOrDefaultAsync(i => i.UserId == user!.Id.ToString());
           
        model.AvailableCourses = instructor?.Crs.ToList() ?? new List<Course>();
        model.AvailableStudents = await _context.Students
            .Where(s => instructor!.Branches.Select(b => b.Id).Contains(s.BranchId ?? 0))
            .ToListAsync();

        return View(model);
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var instructor = await _context.Instructors
            .Include(i => i.Branches)
            .Include(i => i.Crs)
            .FirstOrDefaultAsync(i => i.UserId == user.Id.ToString());

        if (instructor == null)
        {
            // Fallback for testing if current user is not linked to an instructor
            ViewBag.Message = "Your account is not linked to an Instructor profile.";
            return View(new InstructorDashboardViewModel()); 
        }

        var viewModel = new InstructorDashboardViewModel
        {
            Instructor = instructor,
            Branches = instructor.Branches.ToList(),
            Courses = instructor.Crs.ToList()
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult AddStudent()
    {
        // Populate Dropdowns
        ViewBag.Tracks = _context.Tracks.ToList();
        ViewBag.Branches = _context.Branches.ToList();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddStudent(CreateStudentViewModel model)
    {
        if (ModelState.IsValid)
        {
            // 1. Create Identity User
            var user = new AppUsers { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign Student Role
                await _userManager.AddToRoleAsync(user, "Student");

                // 2. Create Domain Student
                var student = new Student
                {
                    Name = model.Name,
                    Address = model.Address,
                    Phone = model.Phone,
                    Degree = model.Degree,
                    TrackId = model.TrackId,
                    BranchId = model.BranchId, // Set Branch
                    GraduationDate = model.GraduationDate,
                    UserId = user.Id.ToString()
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Student added successfully!";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        ViewBag.Tracks = _context.Tracks.ToList();
        ViewBag.Branches = _context.Branches.ToList();
        return View(model);
    }

    [HttpGet]
    public IActionResult AssignExam(int courseId)
    {
        var course = _context.Courses.Find(courseId);
        if (course == null) return NotFound();

        ViewBag.CourseName = course.Name;
        var model = new Exam { CourseId = courseId };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AssignExam(int courseId, int countMCQ, int countTF, int? duration, int? minDegree, int? totDegree)
    {
        if (countMCQ < 0 || countTF < 0)
        {
            ModelState.AddModelError("", "Question counts must be non-negative.");
            var course = _context.Courses.Find(courseId);
            ViewBag.CourseName = course?.Name;
            return View(new Exam { CourseId = courseId });
        }

        try
        {
            // Define output parameter for ExamId
            var examIdParam = new Microsoft.Data.SqlClient.SqlParameter
            {
                ParameterName = "@return_value",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output
            };

            // Call Stored Procedure
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC @return_value = [dbo].[newExam] @min_degree={0}, @duration={1}, @countMCQ={2}, @countTF={3}, @total_degree={4}, @courseId={5}",
                minDegree ?? 50, duration ?? 60, countMCQ, countTF, totDegree ?? 100, courseId, examIdParam);

            // Since parameters are not automatically updated in raw SQL for output, 
            // and EF Core ExecuteSqlRaw doesn't easily return output params without reloading...
            // AND we need to show the questions.
            // Let's assume the newExam simply inserts. We need the ExamId. 
            // A common pattern with EF + SPs for IDs is slightly complex. 
            // Alternative: Fetch the latest exam for this course created just now.
            
            var latestExam = await _context.Exams
                .Where(e => e.CourseId == courseId)
                .OrderByDescending(e => e.Id)
                .Include(e => e.Course)
                .Include(e => e.IdQuests)
                .ThenInclude(q => q.Choices)
                .FirstOrDefaultAsync();

            if (latestExam == null) throw new Exception("Exam generation failed or could not be retrieved.");

            // Prepare ViewModel for Preview
            var viewModel = new StudentExamViewModel
            {
                ExamId = latestExam.Id,
                CourseId = (int)latestExam.CourseId!,
                CourseName = latestExam.Course?.Name ?? "Unknown",
                DurationMinutes = latestExam.Duration ?? 60,
                Questions = latestExam.IdQuests.Select(q => new QuestionViewModel
                {
                    Id = q.Id,
                    Body = q.Body!,
                    Type = q.Type!,
                    Mark = q.Mark ?? 1,
                    Choices = q.Choices.Select(c => c.Choice1).ToList()
                }).ToList()
            };

            return View("ReviewGeneratedExam", viewModel);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error creating exam: {ex.Message}");
            var course = _context.Courses.Find(courseId);
            ViewBag.CourseName = course?.Name;
            return View(new Exam { CourseId = courseId });
        }
    }
}
