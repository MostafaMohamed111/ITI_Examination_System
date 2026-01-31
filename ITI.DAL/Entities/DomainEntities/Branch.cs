using System;
using System.Collections.Generic;

namespace ITI.DAL.Entities.DomainEntities;

public partial class Branch
{
    public int Id { get; set; }

    public string? Hotline { get; set; }

    public string? Name { get; set; }

    public string? Location { get; set; }

    public int? MgrId { get; set; }

    public virtual Instructor? Mgr { get; set; }

    public virtual ICollection<Track> IdTracks { get; set; } = new List<Track>();
}
