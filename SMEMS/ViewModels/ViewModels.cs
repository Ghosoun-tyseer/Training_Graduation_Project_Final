using System.ComponentModel.DataAnnotations;

namespace SMEMS.ViewModels
{
    // ════════════════════════════════════════════════════════
    //  LOGIN
    // ════════════════════════════════════════════════════════
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;


        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    // ════════════════════════════════════════════════════════
    //  REGISTER  (Admin creates new users)
    // ════════════════════════════════════════════════════════
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(150, ErrorMessage = "Maximum 150 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9._]+$",
            ErrorMessage = "Only letters, numbers, dots and underscores")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(200)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        [MaxLength(50)]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;  // admin | engineer | staff

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [MaxLength(150)]
        [Display(Name = "Job Title / Position")]
        public string? JobTitle { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public IFormFile? AvatarFile { get; set; }
    }

    // ════════════════════════════════════════════════════════
    //  PROFILE  (read-only display)
    // ════════════════════════════════════════════════════════
    public class ProfileViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? JobTitle { get; set; }
        public string? Avatar { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ════════════════════════════════════════════════════════
    //  EDIT PROFILE
    // ════════════════════════════════════════════════════════
    public class EditProfileViewModel
    {
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(150)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [MaxLength(100)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email")]
        [MaxLength(200)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        [MaxLength(50)]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        [MaxLength(150)]
        [Display(Name = "Job Title / Position")]
        public string? JobTitle { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        public string? CurrentAvatar { get; set; }

        [Display(Name = "Upload New Photo")]
        public IFormFile? AvatarFile { get; set; }
    }

    // ════════════════════════════════════════════════════════
    //  CHANGE PASSWORD
    // ════════════════════════════════════════════════════════
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "Minimum 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm new password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    // ════════════════════════════════════════════════════════
    //  RESET PASSWORD (forgot password)
    // ════════════════════════════════════════════════════════
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email")]
        [Display(Name = "Work Email")]
        public string Email { get; set; } = string.Empty;
    }

    // ════════════════════════════════════════════════════════
    //  USER MANAGEMENT (Admin)
    // ════════════════════════════════════════════════════════
    public class UserListItemViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Avatar { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ════════════════════════════════════════════════════════
    //  MAINTENANCE REQUEST FORM (Staff submits)
    // ════════════════════════════════════════════════════════
    public class MaintenanceRequestViewModel
    {
        public int? RequestId { get; set; }

        [Required(ErrorMessage = "Please select a device")]
        [Display(Name = "Device")]
        public int DeviceId { get; set; }

        [Required(ErrorMessage = "Issue title is required")]
        [MaxLength(300)]
        [Display(Name = "Issue Title")]
        public string IssueTitle { get; set; } = string.Empty;

        [Display(Name = "Issue Description")]
        public string? IssueDescription { get; set; }

        [Required(ErrorMessage = "Risk level is required")]
        [Display(Name = "Risk Level")]
        public string RiskLevel { get; set; } = "Low";

        [Display(Name = "Alternative Device Available?")]
        public bool HasAlternative { get; set; }
    }

    // ════════════════════════════════════════════════════════
    //  COMPLETE MAINTENANCE (Engineer)
    // ════════════════════════════════════════════════════════
    public class CompleteMaintenanceViewModel
    {
        [Required]
        public int RequestId { get; set; }

        [Required(ErrorMessage = "Completion notes are required")]
        [Display(Name = "Completion Notes")]
        public string CompletionNotes { get; set; } = string.Empty;

        [Display(Name = "Engineer Notes")]
        public string? EngineerNotes { get; set; }

        public int? MaintenanceTypeId { get; set; }
    }

    // ════════════════════════════════════════════════════════
    //  DEVICE FORM (Admin creates/edits)
    // ════════════════════════════════════════════════════════
    public class DeviceFormViewModel
    {
        public int? DeviceId { get; set; }

        [Required(ErrorMessage = "Device code is required")]
        [MaxLength(50)]
        [Display(Name = "Device Code")]
        public string DeviceCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Device name is required")]
        [MaxLength(200)]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        [Display(Name = "Model Number")]
        public string? ModelNumber { get; set; }

        [Required(ErrorMessage = "Serial number is required")]
        [MaxLength(100)]
        [Display(Name = "Serial Number")]
        public string SerialNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        public int? ManufacturerId { get; set; }
        public int? SupplierId { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Operational";

        [Required]
        [Display(Name = "Risk Level")]
        public string RiskLevel { get; set; } = "Medium";

        [MaxLength(200)]
        [Display(Name = "Location")]
        public string? Location { get; set; }

        [Display(Name = "Purchase Date")]
        public DateOnly? PurchaseDate { get; set; }

        [MaxLength(100)]
        [Display(Name = "Expected Lifespan")]
        public string? ExpectedLifespan { get; set; }

        [Display(Name = "Next Maintenance Date")]
        public DateOnly? NextMaintenanceDate { get; set; }

        [Display(Name = "Warranty Start Date")]
        public DateOnly? WarrantyStartDate { get; set; }

        [Display(Name = "Warranty End Date")]
        public DateOnly? WarrantyEndDate { get; set; }
        public string? Notes { get; set; }
    }
    // ════════════════════════════════════════════════════════
    //  REPORTS FILTER 
    // ════════════════════════════════════════════════════════
    public class ReportFilterViewModel
    {
        // ── نوع التقرير ──
        public string ReportType { get; set; } = "device"; // device | maintenance | full

        // ── فلاتر الأجهزة ──
        public int? DepartmentId { get; set; }
        public string? DeviceStatus { get; set; }        
        public string? DeviceRiskLevel { get; set; }     
        public string? PurchaseDateFrom { get; set; }
        public string? PurchaseDateTo { get; set; }
        public int? ManufacturerId { get; set; }
        public int? SupplierId { get; set; }

        // ── فلاتر الصيانة ──
        public string? MaintenanceStatus { get; set; }     
        public string? MaintenanceRiskLevel { get; set; } 
        public string? MaintenanceType { get; set; }
        public int? AssignedEngineerId { get; set; }
        public int? ReportedByStaffId { get; set; }
        public string? RequestDateFrom { get; set; }
        public string? RequestDateTo { get; set; }

        // ── حقول التحديد للأجهزة ──
        public bool IncludeDeviceCode { get; set; } = true;
        public bool IncludeName { get; set; } = true;
        public bool IncludeSerial { get; set; } = true;
        public bool IncludeModel { get; set; }
        public bool IncludeManufacturer { get; set; }
        public bool IncludeSupplier { get; set; }
        public bool IncludeDepartment { get; set; } = true;
        public bool IncludeLocation { get; set; }
        public bool IncludePurchaseDate { get; set; }
        public bool IncludeWarranty { get; set; }
        public bool IncludeStatusField { get; set; } = true;
        public bool IncludeRisk { get; set; }
        public bool IncludeNextMaintenance { get; set; }
        public bool IncludeFailureCount { get; set; }

        // ── حقول التحديد للصيانة ──
        public bool IncludeRequestCode { get; set; } = true;
        public bool IncludeRequestDate { get; set; } = true;
        public bool IncludeDeviceName { get; set; } = true;
        public bool IncludeIssueTitle { get; set; } = true;
        public bool IncludeIssueDescription { get; set; }
        public bool IncludeType { get; set; }
        public bool IncludeEngineer { get; set; }
        public bool IncludeReporter { get; set; }
        public bool IncludeMaintenanceStatus { get; set; } = true;
        public bool IncludeMaintenanceRisk { get; set; }
        public bool IncludeStartedAt { get; set; }
        public bool IncludeCompletedAt { get; set; }
        public bool IncludeCompletionNotes { get; set; }
        public bool IncludeHasAlternative { get; set; }

        // ── إعدادات التصدير ──
        public bool IncludeCharts { get; set; } = true;
        public bool IncludeSummary { get; set; } = true;
        public string Format { get; set; } = "pdf";
        public string PdfTemplate { get; set; } = "professional"; // professional | compact | detailed
        public string ChartType { get; set; } = "both"; // pie | bar | both | none
    }

    // ════════════════════════════════════════════════════════
    //  REPORT RESULT VIEW MODEL
    // ════════════════════════════════════════════════════════
    public class ReportResultViewModel
    {
        public string ReportType { get; set; } = string.Empty;
        public string ReportTitle { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;

        // البيانات
        public List<DeviceReportRow> DeviceRows { get; set; } = new();
        public List<MaintenanceReportRow> MaintenanceRows { get; set; } = new();

        // الإحصائيات
        public ReportStatistics Statistics { get; set; } = new();

        // بيانات الرسوم البيانية
        public ChartData ChartData { get; set; } = new();

        // الفلاتر المستخدمة
        public ReportFilterViewModel AppliedFilters { get; set; } = new();
    }

    public class DeviceReportRow
    {
        public string DeviceCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ModelNumber { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string? Manufacturer { get; set; }
        public string? Supplier { get; set; }
        public string? Department { get; set; }
        public string? Location { get; set; }
        public string? PurchaseDate { get; set; }
        public string? WarrantyStatus { get; set; }
        public string Status { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public string? NextMaintenance { get; set; }
        public int FailureCount { get; set; }
    }

    public class MaintenanceReportRow
    {
        public string RequestCode { get; set; } = string.Empty;
        public string RequestDate { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string? DeviceCode { get; set; }
        public string IssueTitle { get; set; } = string.Empty;
        public string? IssueDescription { get; set; }
        public string? MaintenanceType { get; set; }
        public string? Engineer { get; set; }
        public string? Reporter { get; set; }
        public string Status { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public string? StartedAt { get; set; }
        public string? CompletedAt { get; set; }
        public string? CompletionNotes { get; set; }
        public bool HasAlternative { get; set; }
    }

    public class ReportStatistics
    {
        public int TotalRecords { get; set; }
        public int OperationalDevices { get; set; }
        public int MaintenanceNeeded { get; set; }
        public int UnderMaintenance { get; set; }
        public int OutOfService { get; set; }
        public int PendingRequests { get; set; }
        public int InProgressRequests { get; set; }
        public int CompletedRequests { get; set; }
        public double AverageResolutionDays { get; set; }
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
        public Dictionary<string, int> DepartmentDistribution { get; set; } = new();
        public Dictionary<string, int> RiskDistribution { get; set; } = new();
        public Dictionary<string, int> MonthlyTrend { get; set; } = new();
    }

    public class ChartData
    {
        public List<ChartSlice> PieChartData { get; set; } = new();
        public List<ChartBar> BarChartData { get; set; } = new();
        public List<ChartLine> LineChartData { get; set; } = new();
    }

    public class ChartSlice
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class ChartBar
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class ChartLine
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}