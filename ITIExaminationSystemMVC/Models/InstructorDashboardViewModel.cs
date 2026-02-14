using ITI.DAL.Entities.DomainEntities;

namespace ITIExaminationSystemMVC.Models;

public class InstructorDashboardViewModel
{
    public Instructor? Instructor { get; set; }
    public List<Branch> Branches { get; set; } = new();
    public List<Course> Courses { get; set; } = new();
}
