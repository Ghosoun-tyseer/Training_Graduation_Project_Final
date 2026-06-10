using System;
using System.Collections.Generic;

namespace SMEMS.Models;

public partial class Part
{
    public int PartId { get; set; }

    public string Name { get; set; } = null!;

    public string? PartNumber { get; set; }

    public int? ManufacturerId { get; set; }

    public int QuantityInStock { get; set; }

    public decimal UnitPrice { get; set; }

    public int MinimumStockLevel { get; set; }

    public string? Notes { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<MaintenancePart> MaintenanceParts { get; set; } = new List<MaintenancePart>();

    public virtual Manufacturer? Manufacturer { get; set; }
}
