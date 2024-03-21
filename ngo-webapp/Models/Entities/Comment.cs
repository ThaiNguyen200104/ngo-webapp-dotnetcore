using System;
using System.Collections.Generic;

namespace ngo_webapp.Models.Entities;

public partial class Comment
{
    public int CommentId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreationDate { get; set; }

    public int UserId { get; set; }

    public int BlogId { get; set; }

    public int? ParentCommentId { get; set; }

    public virtual Blog Blog { get; set; } = null!;

    public virtual ICollection<Comment> InverseParentComment { get; set; } = new List<Comment>();

    public virtual Comment? ParentComment { get; set; }

    public virtual User User { get; set; } = null!;
}
