using System;
using System.Collections.Generic;

namespace SMEMS.Models;

public partial class MaintenanceType
{
    public int TypeId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
}
