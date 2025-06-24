using System;
using System.Collections.Generic;

namespace GreenAgriApp.Models;

public partial class User
{
    public string Id { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime DateRegistered { get; set; }

    public bool IsActive { get; set; }
}
