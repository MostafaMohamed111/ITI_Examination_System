namespace ITIExaminationSystemMVC.Models;

public class ExamResultViewModel
{
    public string CourseName { get; set; } = string.Empty;
    public int TotalScore { get; set; }
    public int MaxScore { get; set; }
    public int PassingScore { get; set; }
    public double Percentage { get; set; }
    public bool Passed { get; set; }
    
    public List<QuestionResultViewModel> QuestionResults { get; set; } = new();
}

public class QuestionResultViewModel
{
    public int QuestionNumber { get; set; }
    public string QuestionBody { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public int Mark { get; set; }
    public string? StudentAnswer { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
