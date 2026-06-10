using System;
using System.Collections.Generic;

namespace SMEMS.Models;

public partial class DeviceWarranty
{
    public int WarrantyId { get; set; }

    public int DeviceId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string? WarrantyProvider { get; set; }

    public string? Terms { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Device Device { get; set; } = null!;
}
