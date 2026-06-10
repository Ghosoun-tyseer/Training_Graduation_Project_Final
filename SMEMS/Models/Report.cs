using System;
using System.Collections.Generic;

namespace SMEMS.Models;

public partial class Report
{
    public int ReportId { get; set; }

    public string Title { get; set; } = null!;

    public string? ReportType { get; set; }

    public string? Format { get; set; }

    public string? Parameters { get; set; }

    public int? GeneratedByAdminId { get; set; }

    public int? GeneratedByEngineerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Admin? GeneratedByAdmin { get; set; }

    public virtual Engineer? GeneratedByEngineer { get; set; }
}
