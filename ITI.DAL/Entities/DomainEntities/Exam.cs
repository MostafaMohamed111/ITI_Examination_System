using System;
using System.Collections.Generic;

namespace ITI.DAL.Entities.DomainEntities;

public partial class Exam
{
    public int Id { get; set; }

    public int? MinDegree { get; set; }

    public int? Duration { get; set; }

    public int? QuestionNum { get; set; }

    public int? TotDegree { get; set; }

    public int? CourseId { get; set; }

    public virtual Course? Course { get; set; }

    public virtual ICollection<Question> IdQuests { get; set; } = new List<Question>();
}
