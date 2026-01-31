using System;
using System.Collections.Generic;

namespace ITI.DAL.Entities.DomainEntities;

public partial class Instructor
{
    public int Id { get; set; }

    public decimal? Salary { get; set; }

    public string? Degree { get; set; }

    public string? Phone { get; set; }

    public string? Type { get; set; }

    public string? Name { get; set; }

    public DateOnly? Dob { get; set; }

    public string? Address { get; set; }

    public string? UserId { get; set; }

    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();

    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();

    public virtual ICollection<Course> Crs { get; set; } = new List<Course>();

    public virtual ICollection<Track> TracksNavigation { get; set; } = new List<Track>();
}
