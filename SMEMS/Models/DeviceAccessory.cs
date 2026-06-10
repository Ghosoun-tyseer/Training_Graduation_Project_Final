using System;
using System.Collections.Generic;

namespace SMEMS.Models;

public partial class DeviceAccessory
{
    public int DeviceAccessoryId { get; set; }

    public int DeviceId { get; set; }

    public int AccessoryId { get; set; }

    public int Quantity { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Accessory Accessory { get; set; } = null!;

    public virtual Device Device { get; set; } = null!;
}
