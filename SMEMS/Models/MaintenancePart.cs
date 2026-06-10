using System;
using System.Collections.Generic;

namespace SMEMS.Models;

public partial class MaintenancePart
{
    public int MaintenancePartId { get; set; }

    public int MaintenanceRequestId { get; set; }

    public int PartId { get; set; }

    public int QuantityUsed { get; set; }

    public decimal UnitPrice { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual MaintenanceRequest MaintenanceRequest { get; set; } = null!;

    public virtual Part Part { get; set; } = null!;
}
