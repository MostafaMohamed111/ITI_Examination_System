using ITI.DAL.Entities.DomainEntities;

namespace ITIExaminationSystemMVC.Models;

public class StudentExamViewModel
{
    public int ExamId { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; }
    public int DurationMinutes { get; set; }
    
    public List<QuestionViewModel> Questions { get; set; } = new();
    
    // Key: QuestionId, Value: Selected Answer Text
    public Dictionary<int, string> UserAnswers { get; set; } = new();
}

public class QuestionViewModel
{
    public int Id { get; set; }
    public string Body { get; set; }
    public string Type { get; set; } // MCQ, TF
    public int Mark { get; set; }
    public List<string> Choices { get; set; } = new();
}
