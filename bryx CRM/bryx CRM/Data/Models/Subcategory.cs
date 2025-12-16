using System;
using System.Collections.Generic;

namespace bryx_CRM.Data.Models;

public partial class Subcategory
{
    public int Id { get; set; }

    public string CategoryName { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[]? RowVersion { get; set; }
}
