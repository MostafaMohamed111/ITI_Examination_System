using System;
using System.Collections.Generic;

namespace ITI.DAL.Entities.DomainEntities;

public partial class Choice
{
    public int QuestionId { get; set; }

    public string Choice1 { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;
}
