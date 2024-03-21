using System;
using System.Collections.Generic;

namespace ngo_webapp.Models.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime RegistrationDate { get; set; }

    public bool IsAdmin { get; set; }

    public string? Bio { get; set; }

    public string? GoogleHash { get; set; }

    public string? UserImage { get; set; }

    public decimal Balance { get; set; }

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
}
