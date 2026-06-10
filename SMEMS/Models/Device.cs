using System;
using System.Collections.Generic;

namespace SMEMS.Models;

public partial class Device
{
    public int DeviceId { get; set; }

    public string DeviceCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? ModelNumber { get; set; }

    public string SerialNumber { get; set; } = null!;

    public int? ManufacturerId { get; set; }

    public int? SupplierId { get; set; }

    public int DepartmentId { get; set; }

    public string Status { get; set; } = null!;

    public string RiskLevel { get; set; } = null!;

    public string? Location { get; set; }

    public DateOnly? PurchaseDate { get; set; }

    public string? ExpectedLifespan { get; set; }

    public int FailureCount { get; set; }

    public DateOnly? NextMaintenanceDate { get; set; }

    public string? Notes { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<DeviceAccessory> DeviceAccessories { get; set; } = new List<DeviceAccessory>();

    public virtual ICollection<DeviceWarranty> DeviceWarranties { get; set; } = new List<DeviceWarranty>();

    public virtual ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();

    public virtual Manufacturer? Manufacturer { get; set; }

    public virtual Supplier? Supplier { get; set; }
}
