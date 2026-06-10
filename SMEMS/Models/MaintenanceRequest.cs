using System;
using System.Collections.Generic;

namespace SMEMS.Models;

public partial class MaintenanceRequest
{
    public int RequestId { get; set; }

    public string RequestCode { get; set; } = null!;

    public int DeviceId { get; set; }

    public int? ReportedByStaffId { get; set; }

    public int? AssignedEngineerId { get; set; }

    public int? MaintenanceTypeId { get; set; }

    public string Status { get; set; } = null!;

    public string RiskLevel { get; set; } = null!;

    public string IssueTitle { get; set; } = null!;

    public string? IssueDescription { get; set; }

    public string? EngineerNotes { get; set; }

    public string? CompletionNotes { get; set; }

    public bool HasAlternative { get; set; }

    public DateTime RequestDate { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Engineer? AssignedEngineer { get; set; }

    public virtual Device Device { get; set; } = null!;

    public virtual ICollection<MaintenancePart> MaintenanceParts { get; set; } = new List<MaintenancePart>();

    public virtual MaintenanceType? MaintenanceType { get; set; }

    public virtual Staff? ReportedByStaff { get; set; }
}
