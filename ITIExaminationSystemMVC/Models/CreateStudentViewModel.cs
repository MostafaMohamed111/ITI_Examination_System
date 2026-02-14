using System.ComponentModel.DataAnnotations;

namespace ITIExaminationSystemMVC.Models;

public class CreateStudentViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public int TrackId { get; set; }

    [Required]
    [Display(Name = "Branch")]
    public int BranchId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Degree { get; set; }
    public DateOnly? GraduationDate { get; set; }
}
