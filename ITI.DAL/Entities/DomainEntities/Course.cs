using System;
using System.Collections.Generic;

namespace ITI.DAL.Entities.DomainEntities;

public partial class Course
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? Hours { get; set; }

    public int? TotDegree { get; set; }

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<StudCr> StudCrs { get; set; } = new List<StudCr>();

    public virtual ICollection<Instructor> Ins { get; set; } = new List<Instructor>();

    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
}
