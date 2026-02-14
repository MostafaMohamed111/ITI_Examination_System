using ITI.DAL.Entities.DomainEntities;

namespace ITIExaminationSystemMVC.Models;

public class AssignStudentToCourseViewModel
{
    public int InstructorId { get; set; }
    public int SelectedCourseId { get; set; }
    public int SelectedStudentId { get; set; }

    public List<Course> AvailableCourses { get; set; } = new();
    public List<Student> AvailableStudents { get; set; } = new();
}
