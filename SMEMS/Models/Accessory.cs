using System;
using System.Collections.Generic;

namespace SMEMS.Models;

public partial class Accessory
{
    public int AccessoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int QuantityInStock { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<DeviceAccessory> DeviceAccessories { get; set; } = new List<DeviceAccessory>();
}
