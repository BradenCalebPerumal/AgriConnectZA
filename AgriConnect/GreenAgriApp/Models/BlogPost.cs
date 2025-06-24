using System;
using System.Collections.Generic;

namespace GreenAgriApp.Models;

public partial class BlogPost
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string? ImagePath { get; set; }

    public DateTime DatePosted { get; set; }

    public bool IsViolation { get; set; }

    public string? ViolationNote { get; set; }
}
