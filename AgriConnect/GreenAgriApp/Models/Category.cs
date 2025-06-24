using System;
using System.Collections.Generic;

namespace GreenAgriApp.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? RequestorRole { get; set; }

    public virtual ICollection<GreenTechProduct> GreenTechProducts { get; set; } = new List<GreenTechProduct>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
