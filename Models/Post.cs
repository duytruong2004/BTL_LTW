using System;
using System.Collections.Generic;

namespace BTL_LTW.Models;

public partial class Post
{
    public int PostId { get; set; }

    public int? CategoryId { get; set; }

    public int? MemberId { get; set; }

    public string? PostThumbnailUrl { get; set; }

    public string? PostContent { get; set; }

    public string PostTitle { get; set; } = null!;

    public int? PostStatus { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? ViewCount { get; set; }

    public int? ApproveBy { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public int? ApproveStatus { get; set; }

    public string? RejectedReason { get; set; }

    public virtual Member? ApproveByNavigation { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Member? Member { get; set; }
}
