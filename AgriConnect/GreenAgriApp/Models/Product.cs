using System;
using System.Collections.Generic;

namespace GreenAgriApp.Models;

public partial class Product
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Quantity { get; set; }

    public int CategoryId { get; set; }

    public byte[]? Image { get; set; }

    public DateTime DatePosted { get; set; }

    public virtual Category Category { get; set; } = null!;
}
