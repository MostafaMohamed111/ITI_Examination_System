using System;
using System.Collections.Generic;

namespace ITI.DAL.Entities.DomainEntities;

public partial class Student
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Phone { get; set; }

    public string? Degree { get; set; }

    public string? Address { get; set; }

    public DateOnly? GraduationDate { get; set; }

    public int? TrackId { get; set; }

    public string? UserId { get; set; }

    public virtual ICollection<StudCr> StudCrs { get; set; } = new List<StudCr>();

    public virtual Track? Track { get; set; }
}
