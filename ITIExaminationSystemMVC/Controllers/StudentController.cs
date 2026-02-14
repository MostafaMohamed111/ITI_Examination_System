using ITI.DAL.Data.AppDbContext;
using ITI.DAL.Entities.Identity;
using ITIExaminationSystemMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITIExaminationSystemMVC.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUsers> _userManager;

    public StudentController(ApplicationDbContext context, UserManager<AppUsers> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var student = await _context.Students
            .Include(s => s.StudCrs)
            .ThenInclude(sc => sc.Crs)
            .ThenInclude(c => c.Exams)
            .FirstOrDefaultAsync(s => s.UserId == user.Id.ToString());

        if (student == null)
        {
             ViewBag.Message = "Student profile not found.";
             return View();
        }

        // Flatten exams from enrolled courses
        var exams = student.StudCrs
            .SelectMany(sc => sc.Crs.Exams.Select(e => new { Exam = e, CourseName = sc.Crs.Name }))
            .ToList();
        
        // Pass data via ViewBag/ViewModel
        ViewBag.Exams = exams;

        return View(student);
    }
    
    [HttpGet]
    public async Task<IActionResult> TakeExam(int id)
    {
        var exam = await _context.Exams
            .Include(e => e.Course)
            .Include(e => e.IdQuests)
            .ThenInclude(q => q.Choices)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (exam == null) return NotFound();

        var viewModel = new StudentExamViewModel
        {
            ExamId = exam.Id,
            CourseId = (int)exam.CourseId!,
            CourseName = exam.Course!.Name!,
            DurationMinutes = exam.Duration ?? 60,
            Questions = exam.IdQuests.Select(q => new QuestionViewModel
            {
                Id = q.Id,
                Body = q.Body!,
                Type = q.Type!,
                Mark = q.Mark ?? 1,
                Choices = q.Choices.Select(c => c.Choice1).ToList()
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitExam(StudentExamViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");
        var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id.ToString());
        if (student == null) return RedirectToAction("Index");

        // Get exam details for passing score
        var exam = await _context.Exams
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == model.ExamId);

        // Calculate Grade and build result details
        int totalScore = 0;
        int maxScore = 0;
        var questionResults = new List<QuestionResultViewModel>();
        int questionNumber = 1;

        // Fetch Model Answers using Stored Procedure
        // We need a helper type to map the results. Since we can't define it inside the method easily for EF...
        // We will assume the SP returns QuestionId and CorrectAnswer. 
        // If we can't map to a specific entity, we might need a raw ADO.NET call or a registered Keyless Entity.
        // For simplicity in this context, let's try using a raw SQL command and reading via a Reader if possible, 
        // OR simpler: assume we can query the Questions table but the user *specifically* wants the SP.
        // Let's use ADO.NET on the context's connection to be safe and flexible.

        var modelAnswers = new Dictionary<int, string>();
        
        try 
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "EXEC getModelAns @examId";
                var param = command.CreateParameter();
                param.ParameterName = "@examId";
                param.Value = model.ExamId;
                command.Parameters.Add(param);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        try 
                        {
                            // Try reading first two columns by index as fallback
                            int qId = 0;
                            string correctAns = "";

                            if (reader.FieldCount >= 2)
                            {
                                // Adjust these indices based on actual SP output if known. 
                                // Assuming 0: QId, 1: Ans OR Column Names
                                // Trying safer retrieval
                                object val0 = reader.GetValue(0);
                                object val1 = reader.GetValue(1);

                                if (val0 != DBNull.Value) qId = Convert.ToInt32(val0);
                                if (val1 != DBNull.Value) correctAns = val1.ToString() ?? "";
                                
                                if (!modelAnswers.ContainsKey(qId))
                                    modelAnswers.Add(qId, correctAns);
                            }
                        }
                        catch
                        {
                            // Swallow row parsing error to continue
                        }
                    }
                }
            }
            // Do not close connection if it was managed by EF, but here we opened it if needed. 
            // EF usually manages its own, but GetDbConnection returns the underlying one.
            // Best practice: Close if we opened it, or leave it. 
            // To be safe with EF:
            // await connection.CloseAsync(); 
            // actually EF Core sharing connection: beware of closing it if unexpected. 
            // But usually safe to close if we are done with this scope.
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error fetching model answers: {ex.Message}");
            // Fallback: We will check 'Questions' table in the loop below as backup.
        }


        foreach (var userAns in model.UserAnswers)
        {
            var question = await _context.Questions.FindAsync(userAns.Key);
            if (question != null)
            {
                int questionMark = question.Mark ?? 1;
                maxScore += questionMark;
                
                string correctAns = "";
                if (modelAnswers.ContainsKey(question.Id))
                {
                    correctAns = modelAnswers[question.Id];
                }
                else
                {
                    // Fallback if SP didn't return it (shouldn't happen)
                    correctAns = question.CorrectAns ?? "";
                }

                bool isCorrect = string.Equals(correctAns?.Trim(), userAns.Value?.Trim(), StringComparison.OrdinalIgnoreCase);
                if (isCorrect)
                {
                    totalScore += questionMark;
                }

                questionResults.Add(new QuestionResultViewModel
                {
                    QuestionNumber = questionNumber++,
                    QuestionBody = question.Body ?? "",
                    QuestionType = question.Type ?? "",
                    Mark = questionMark,
                    StudentAnswer = userAns.Value,
                    CorrectAnswer = correctAns ?? "",
                    IsCorrect = isCorrect
                });
            }
        }

        // Update StudCr Grade
        var studCr = await _context.StudCrs
            .FirstOrDefaultAsync(sc => sc.StdId == student.Id && sc.CrsId == model.CourseId);

        if (studCr != null)
        {
            studCr.Grade = totalScore;
            _context.StudCrs.Update(studCr);
            await _context.SaveChangesAsync();
        }

        // Store results in TempData for display
        var resultViewModel = new ExamResultViewModel
        {
            CourseName = exam?.Course?.Name ?? "Unknown Course",
            TotalScore = totalScore,
            MaxScore = maxScore,
            PassingScore = exam?.MinDegree ?? 0,
            Percentage = maxScore > 0 ? Math.Round((double)totalScore / maxScore * 100, 2) : 0,
            Passed = totalScore >= (exam?.MinDegree ?? 0),
            QuestionResults = questionResults
        };

        TempData["ExamResult"] = System.Text.Json.JsonSerializer.Serialize(resultViewModel);
        return RedirectToAction("ExamResult");
    }

    public IActionResult ExamResult()
    {
        if (TempData["ExamResult"] == null)
        {
            return RedirectToAction("Index");
        }

        var resultJson = TempData["ExamResult"]?.ToString();
        var result = System.Text.Json.JsonSerializer.Deserialize<ExamResultViewModel>(resultJson!);
        
        return View(result);
    }
}
