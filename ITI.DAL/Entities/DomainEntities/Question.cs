using System;
using System.Collections.Generic;

namespace ITI.DAL.Entities.DomainEntities;

public partial class Question
{
    public int Id { get; set; }

    public string? Body { get; set; }

    public string? Type { get; set; }

    public int? Mark { get; set; }

    public string? CorrectAns { get; set; }

    public int CourseId { get; set; }

    public virtual ICollection<Choice> Choices { get; set; } = new List<Choice>();

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<Exam> IdExams { get; set; } = new List<Exam>();
}
