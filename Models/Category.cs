using System;
using System.Collections.Generic;

namespace BTL_LTW.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public int? MemberId { get; set; }

    public string? Decription { get; set; }

    public int? CategoryStatus { get; set; }

    public virtual Member? Member { get; set; }

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
