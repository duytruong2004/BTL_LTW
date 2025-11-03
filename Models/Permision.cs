using System;
using System.Collections.Generic;

namespace BTL_LTW.Models;

public partial class Permision
{
    public int PermisionId { get; set; }

    public string? PermisionName { get; set; }

    public virtual ICollection<MemberPermision> MemberPermisions { get; set; } = new List<MemberPermision>();
}
