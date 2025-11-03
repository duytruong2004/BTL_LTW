using System;
using System.Collections.Generic;

namespace BTL_LTW.Models;

public partial class MemberPermision
{
    public int MemberPermisionId { get; set; }

    public int? PermisionId { get; set; }

    public int? MemberId { get; set; }

    public bool? Licensed { get; set; }

    public virtual Member? Member { get; set; }

    public virtual Permision? Permision { get; set; }
}
