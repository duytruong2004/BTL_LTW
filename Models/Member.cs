using System;
using System.Collections.Generic;

namespace BTL_LTW.Models;

public partial class Member
{
    public int MembersId { get; set; }

    public string? FullName { get; set; }

    public string Emai { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string PassWordHash { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<MemberPermision> MemberPermisions { get; set; } = new List<MemberPermision>();

    public virtual ICollection<Post> PostApproveByNavigations { get; set; } = new List<Post>();

    public virtual ICollection<Post> PostMembers { get; set; } = new List<Post>();
}
