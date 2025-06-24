using System;
using System.Collections.Generic;

namespace GreenAgriApp.Models;

public partial class Resource
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public DateTime UploadDate { get; set; }
}
