using System;
using System.Collections.Generic;

namespace ITI.DAL.Entities.DomainEntities;

public partial class Track
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Program { get; set; }

    public int? InsId { get; set; }

    public virtual Instructor? Ins { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<Course> Crs { get; set; } = new List<Course>();

    public virtual ICollection<Branch> IdBranches { get; set; } = new List<Branch>();

    public virtual ICollection<Instructor> InsNavigation { get; set; } = new List<Instructor>();
}
