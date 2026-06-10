using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMEMS.Helpers;
using SMEMS.Models;
using SMEMS.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.Generic;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

using QuestPDF.Infrastructure;
namespace SMEMS.Controllers
{
    [RequireLogin(Roles.Admin)]
    public class AdminController : Controller
    {
        private readonly MyDbContext _db;

        public AdminController(MyDbContext db)
        {
            _db = db;
        }

       
        //  Dashboard
 
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalDevices = await _db.Devices.CountAsync();
            ViewBag.TotalRequests = await _db.MaintenanceRequests.CountAsync();
            ViewBag.ActiveUsers = await _db.Admins.CountAsync(a => a.IsActive)
                                   + await _db.Engineers.CountAsync(e => e.IsActive)
                                   + await _db.Staff.CountAsync(s => s.IsActive);

            ViewBag.DevicesByStatus = await _db.Devices
     .GroupBy(d => d.Status)
     .Select(g => new { Status = g.Key, Count = g.Count() })
     .ToListAsync();


            ViewBag.DevicesByDept = await _db.Devices
          .Include(d => d.Department)
          .Where(d => d.DepartmentId != null && d.Department != null)
          .GroupBy(d => d.Department!.Name)
          .Select(g => new { Dept = g.Key, Count = g.Count() })
          .ToListAsync();


            ViewBag.RequestsByDept = await _db.MaintenanceRequests
                .Include(r => r.Device).ThenInclude(d => d.Department)
                .GroupBy(r => r.Device.Department.Name)
                .Select(g => new { Dept = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            // Session-based notifications (no database table)
            HttpContext.Session.SeedDefaultNotifications();
            var role = HttpContext.Session.GetRole();
            var userId = HttpContext.Session.GetUserId();
            ViewBag.RecentNotifications = HttpContext.Session
                .GetNotificationsForUser(role, userId)
                .Take(5)
                .ToList();

            // Maintenance trend data (last 4 months)
            ViewBag.TrendData = await _db.MaintenanceRequests
                .Where(r => r.RequestDate >= DateTime.UtcNow.AddMonths(-4))
                .GroupBy(r => new { r.RequestDate.Month, r.Status })
                .Select(g => new { g.Key.Month, Status = g.Key.Status, Count = g.Count() })
                .ToListAsync();

            return View();
        }

        
        //  DEVICES
       
        [HttpGet]
        public async Task<IActionResult> Devices(string? search, string? status, string? risk, int? deptId)
        {
            var query = _db.Devices
                .Include(d => d.Department)
                .Include(d => d.Manufacturer)
                .Include(d => d.Supplier)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(d => d.Name.Contains(search) ||
                                         d.SerialNumber.Contains(search) ||
                                         d.DeviceCode.Contains(search));
            if (!string.IsNullOrEmpty(status))
                query = query.Where(d => d.Status == status);
            if (!string.IsNullOrEmpty(risk))
                query = query.Where(d => d.RiskLevel == risk);
            if (deptId.HasValue)
                query = query.Where(d => d.DepartmentId == deptId);

            ViewBag.Departments = await _db.Departments.OrderBy(d => d.Name).ToListAsync();
            return View(await query.OrderByDescending(d => d.CreatedAt).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> AddDevice()
        {
            ViewBag.Departments = await _db.Departments.OrderBy(d => d.Name).ToListAsync();
            ViewBag.Manufacturers = await _db.Manufacturers.OrderBy(m => m.Name).ToListAsync();
            ViewBag.Suppliers = await _db.Suppliers.OrderBy(s => s.Name).ToListAsync();
            return View(new DeviceFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDevice(DeviceFormViewModel vm)
        {
            ViewBag.Departments = await _db.Departments.OrderBy(d => d.Name).ToListAsync();
            ViewBag.Manufacturers = await _db.Manufacturers.OrderBy(m => m.Name).ToListAsync();
            ViewBag.Suppliers = await _db.Suppliers.OrderBy(s => s.Name).ToListAsync();

            if (!ModelState.IsValid) return View(vm);

            if (await _db.Devices.AnyAsync(d => d.DeviceCode == vm.DeviceCode || d.SerialNumber == vm.SerialNumber))
            {
                ModelState.AddModelError(string.Empty, "Device code or serial number already exists");
                return View(vm);
            }

            var adminId = HttpContext.Session.GetUserId();
            var device = new Device
            {
                DeviceCode = vm.DeviceCode,
                Name = vm.Name,
                ModelNumber = vm.ModelNumber,
                SerialNumber = vm.SerialNumber,
                DepartmentId = vm.DepartmentId,
                ManufacturerId = vm.ManufacturerId,
                SupplierId = vm.SupplierId,
                Status = vm.Status,
                RiskLevel = vm.RiskLevel,
                Location = vm.Location,
                PurchaseDate = vm.PurchaseDate,
                ExpectedLifespan = vm.ExpectedLifespan,
                NextMaintenanceDate = vm.NextMaintenanceDate,
                Notes = vm.Notes
            };

            _db.Devices.Add(device);

            await _db.SaveChangesAsync();

            if (vm.WarrantyStartDate.HasValue &&
                vm.WarrantyEndDate.HasValue)
            {
                _db.DeviceWarranties.Add(new DeviceWarranty
                {
                    DeviceId = device.DeviceId,
                    StartDate = vm.WarrantyStartDate.Value,
                    EndDate = vm.WarrantyEndDate.Value
                });

                await _db.SaveChangesAsync();
            }

            // Add notification for all users
            HttpContext.Session.AddNotification(
                "New Device Added",
                $"Device {vm.Name} ({vm.DeviceCode}) has been registered in the system.",
                "all", null, "Device", "low");

            TempData["Success"] = "Device added successfully!";
            return RedirectToAction(nameof(Devices));
        }

        [HttpGet]
        public async Task<IActionResult> EditDevice(int id)
        {
            var device = await _db.Devices
     .Include(d => d.DeviceWarranties)
     .FirstOrDefaultAsync(d => d.DeviceId == id);

            if (device == null) return NotFound();

            ViewBag.Departments = await _db.Departments.OrderBy(d => d.Name).ToListAsync();
            ViewBag.Manufacturers = await _db.Manufacturers.OrderBy(m => m.Name).ToListAsync();
            ViewBag.Suppliers = await _db.Suppliers.OrderBy(s => s.Name).ToListAsync();

            var warranty = device.DeviceWarranties
    .OrderByDescending(w => w.WarrantyId)
    .FirstOrDefault();

            return View(new DeviceFormViewModel
            {
                DeviceId = device.DeviceId,
                DeviceCode = device.DeviceCode,
                Name = device.Name,
                ModelNumber = device.ModelNumber,
                SerialNumber = device.SerialNumber,
                DepartmentId = device.DepartmentId,
                ManufacturerId = device.ManufacturerId,
                SupplierId = device.SupplierId,
                Status = device.Status,
                RiskLevel = device.RiskLevel,
                Location = device.Location,
                PurchaseDate = device.PurchaseDate,
                ExpectedLifespan = device.ExpectedLifespan,
                NextMaintenanceDate = device.NextMaintenanceDate,
                Notes = device.Notes,

                WarrantyStartDate = warranty?.StartDate,
                WarrantyEndDate = warranty?.EndDate
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDevice(DeviceFormViewModel vm)
        {
            ViewBag.Departments = await _db.Departments.OrderBy(d => d.Name).ToListAsync();
            ViewBag.Manufacturers = await _db.Manufacturers.OrderBy(m => m.Name).ToListAsync();
            ViewBag.Suppliers = await _db.Suppliers.OrderBy(s => s.Name).ToListAsync();

            if (!ModelState.IsValid) return View(vm);

            var device = await _db.Devices
     .Include(d => d.DeviceWarranties)
     .FirstOrDefaultAsync(d => d.DeviceId == vm.DeviceId);

            if (device == null) return NotFound();

            device.DeviceCode = vm.DeviceCode;
            device.Name = vm.Name;
            device.ModelNumber = vm.ModelNumber;
            device.SerialNumber = vm.SerialNumber;
            device.DepartmentId = vm.DepartmentId;
            device.ManufacturerId = vm.ManufacturerId;
            device.SupplierId = vm.SupplierId;
            device.Status = vm.Status;
            device.RiskLevel = vm.RiskLevel;
            device.Location = vm.Location;
            device.PurchaseDate = vm.PurchaseDate;
            device.ExpectedLifespan = vm.ExpectedLifespan;
            device.NextMaintenanceDate = vm.NextMaintenanceDate;
            device.Notes = vm.Notes;
            device.UpdatedAt = DateTime.UtcNow;

            var warranty = device.DeviceWarranties
    .OrderByDescending(w => w.WarrantyId)
    .FirstOrDefault();

            if (warranty == null)
            {
                if (vm.WarrantyStartDate.HasValue &&
                    vm.WarrantyEndDate.HasValue)
                {
                    device.DeviceWarranties.Add(new DeviceWarranty
                    {
                        StartDate = vm.WarrantyStartDate.Value,
                        EndDate = vm.WarrantyEndDate.Value
                    });
                }
            }
            else
            {
                warranty.StartDate =
                    vm.WarrantyStartDate ?? warranty.StartDate;

                warranty.EndDate =
                    vm.WarrantyEndDate ?? warranty.EndDate;

            }
            await _db.SaveChangesAsync();
            TempData["Success"] = "Device updated!";
            return RedirectToAction(nameof(Devices));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            var device = await _db.Devices.FindAsync(id);
            if (device != null) { _db.Devices.Remove(device); await _db.SaveChangesAsync(); }
            TempData["Success"] = "Device deleted!";
            return RedirectToAction(nameof(Devices));
        }
        [HttpGet]
        public async Task<IActionResult> DeviceHistory(int deviceId)
        {
            var device = await _db.Devices
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.DeviceId == deviceId);

            if (device == null) return NotFound();

            ViewBag.Device = device;
            ViewBag.ReturnController = "Admin";  //  زر Back يرجع لصفحة الأجهزة عند الأدمن

            var requests = await _db.MaintenanceRequests
                .Include(r => r.MaintenanceType)
                .Include(r => r.AssignedEngineer)
                .Where(r => r.DeviceId == deviceId)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View("~/Views/Shared/_DeviceHistory.cshtml", requests);
        }

        //  USER MANAGEMENT
        [HttpGet]
        public async Task<IActionResult> Users(string? search, string? role)
        {
            var admins = (await _db.Admins
                .Where(a => !a.IsDeleted)   //  إخفاء المحذوفين
                .ToListAsync())
                .Select(a => new UserListItemViewModel
                {
                    UserId = a.AdminId,
                    FullName = a.FullName,
                    Username = a.Username,
                    Email = a.Email,
                    Role = "Administrator",
                    Avatar = a.Avatar,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt
                });

            var engineers = (await _db.Engineers
                .Where(e => !e.IsDeleted)   //  إخفاء المحذوفين
                .ToListAsync())
                .Select(e => new UserListItemViewModel
                {
                    UserId = e.EngineerId,
                    FullName = e.FullName,
                    Username = e.Username,
                    Email = e.Email,
                    Role = "Engineer",
                    Avatar = e.Avatar,
                    IsActive = e.IsActive,
                    CreatedAt = e.CreatedAt
                });

            var staff = (await _db.Staff
                .Include(s => s.Department)
                .Where(s => !s.IsDeleted)   // إخفاء المحذوفين
                .ToListAsync())
                .Select(s => new UserListItemViewModel
                {
                    UserId = s.StaffId,
                    FullName = s.FullName,
                    Username = s.Username,
                    Email = s.Email,
                    Role = "Medical Staff",
                    Department = s.Department?.Name,
                    Avatar = s.Avatar,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt
                });

            var all = admins.Concat(engineers).Concat(staff).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                all = all.Where(u => u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                     u.Email.Contains(search, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(role))
                all = all.Where(u => u.Role == role);

            return View(all.OrderBy(u => u.FullName).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUser(int id, string role)
        {
            switch (role)
            {
                case "Administrator":
                    var a = await _db.Admins.FindAsync(id);
                    if (a != null) { a.IsActive = !a.IsActive; a.UpdatedAt = DateTime.UtcNow; }
                    break;
                case "Engineer":
                    var e = await _db.Engineers.FindAsync(id);
                    if (e != null) { e.IsActive = !e.IsActive; e.UpdatedAt = DateTime.UtcNow; }
                    break;
                case "Medical Staff":
                    var s = await _db.Staff.FindAsync(id);
                    if (s != null) { s.IsActive = !s.IsActive; s.UpdatedAt = DateTime.UtcNow; }
                    break;
            }
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id, string role)
        {
            switch (role)
            {
                case "Administrator":
                    var a = await _db.Admins.FindAsync(id);
                    if (a != null)
                    {
                        a.IsDeleted = true;
                        a.IsActive = false;
                        a.UpdatedAt = DateTime.UtcNow;
                    }
                    break;

                case "Engineer":
                    var e = await _db.Engineers.FindAsync(id);
                    if (e != null)
                    {
                        e.IsDeleted = true;
                        e.IsActive = false;
                        e.UpdatedAt = DateTime.UtcNow;
                    }
                    break;

                case "Medical Staff":
                    var s = await _db.Staff.FindAsync(id);
                    if (s != null)
                    {
                        s.IsDeleted = true;
                        s.IsActive = false;
                        s.UpdatedAt = DateTime.UtcNow;
                    }
                    break;
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "User has been deactivated and hidden from the system!";
            return RedirectToAction(nameof(Users));
        }

        //  MAINTENANCE (Admin view – all requests)
        [HttpGet]
        public async Task<IActionResult> Maintenance(string? search, string? status, string? risk)
        {
            var query = _db.MaintenanceRequests
                .Include(r => r.Device).ThenInclude(d => d.Department)
                .Include(r => r.ReportedByStaff)
                .Include(r => r.AssignedEngineer)
                .AsQueryable();

            //  Search
            if (!string.IsNullOrEmpty(search))
            {
                var pattern = $"%{search}%";
                query = query.Where(r =>
                    EF.Functions.Like(r.IssueTitle ?? "", pattern) ||
                    EF.Functions.Like(r.RequestCode ?? "", pattern) ||
                    EF.Functions.Like(r.Device.Name ?? "", pattern) ||
                    EF.Functions.Like(r.Device.Department.Name ?? "", pattern) ||
                    EF.Functions.Like(r.RiskLevel ?? "", pattern) ||
                    EF.Functions.Like(r.Status ?? "", pattern) ||
                    EF.Functions.Like(r.AssignedEngineer.FullName ?? "", pattern)
                );
            }

            //  Status filter
            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status == status);

            //  Risk filter
            if (!string.IsNullOrEmpty(risk))
                query = query.Where(r => r.RiskLevel == risk);

            ViewBag.Engineers = await _db.Engineers
                .Where(e => e.IsActive)
                .OrderBy(e => e.FullName)
                .ToListAsync();

            //  Persist filter state for dropdowns
            ViewBag.Status = status;
            ViewBag.Risk = risk;
            ViewBag.Search = search;

            return View(await query.OrderByDescending(r => r.RequestDate).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignEngineer(int requestId, int engineerId)
        {
            var request = await _db.MaintenanceRequests.FindAsync(requestId);
            if (request == null) return NotFound();

            request.AssignedEngineerId = engineerId;
            request.Status = "In Progress";
            request.StartedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            //  Notify the assigned engineer
            HttpContext.Session.AddNotification(
                "New Assignment",
                $"You have been assigned to maintenance request {request.RequestCode}.",
                "engineer", engineerId, "Assignment", "high");

            TempData["Success"] = "Engineer assigned!";
            return RedirectToAction(nameof(Maintenance));
        }



        
        //  NOTIFICATIONS (Session-based, no database table)
       
        [HttpGet]
        public IActionResult Notifications()
        {
            var role = HttpContext.Session.GetRole();
            var userId = HttpContext.Session.GetUserId();

            HttpContext.Session.SeedDefaultNotifications();

            var notifs = HttpContext.Session.GetNotificationsForUser(role, userId);
            return View(notifs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkNotifRead(int id)
        {
            HttpContext.Session.MarkAsRead(id);
            return RedirectToAction(nameof(Notifications));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteNotif(int id)
        {
            HttpContext.Session.DeleteNotification(id);
            return RedirectToAction(nameof(Notifications));
        }

        //  REPORTS 


        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            await PopulateReportViewBags();
            return View(new ReportFilterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateReport(ReportFilterViewModel filter)
        {
            if (!ModelState.IsValid)
            {
                await PopulateReportViewBags();
                return View("Reports", filter);
            }

            var result = new ReportResultViewModel
            {
                ReportType = filter.ReportType,
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = HttpContext.Session.GetFullName(),
                AppliedFilters = filter
            };

            result.ReportTitle = filter.ReportType switch
            {
                "device" => "Device Inventory Report",
                "maintenance" => "Maintenance Activity Report",
                "full" => "Comprehensive System Report",
                _ => "System Report"
            };

            if (filter.ReportType == "device" || filter.ReportType == "full")
            {
                await BuildDeviceReportData(filter, result);
            }

            if (filter.ReportType == "maintenance" || filter.ReportType == "full")
            {
                await BuildMaintenanceReportData(filter, result);
            }

            BuildStatistics(result);

            if (filter.IncludeCharts)
            {
                BuildChartData(result, filter);
            }

            if (filter.Format == "excel")
                return GenerateExcelReport(result, filter);

            return GenerateProfessionalPdf(result, filter);
        }

        // ── بناء بيانات الأجهزة ──
        private async Task BuildDeviceReportData(ReportFilterViewModel filter, ReportResultViewModel result)
        {
            var query = _db.Devices
                .Include(d => d.Department)
                .Include(d => d.Manufacturer)
                .Include(d => d.Supplier)
                .Include(d => d.DeviceWarranties)
                .AsNoTracking()
                .AsQueryable();

            if (filter.DepartmentId.HasValue)
                query = query.Where(d => d.DepartmentId == filter.DepartmentId);

            if (!string.IsNullOrEmpty(filter.DeviceStatus))
            {
                // توحيد "Out of Service" و "Out Of Service"
                if (filter.DeviceStatus == "Out of Service" || filter.DeviceStatus == "Out Of Service")
                    query = query.Where(d => d.Status == "Out of Service" || d.Status == "Out Of Service");
                else
                    query = query.Where(d => d.Status == filter.DeviceStatus);
            }

            if (!string.IsNullOrEmpty(filter.DeviceRiskLevel))
                query = query.Where(d => d.RiskLevel == filter.DeviceRiskLevel);

            if (filter.ManufacturerId.HasValue)
                query = query.Where(d => d.ManufacturerId == filter.ManufacturerId);

            if (filter.SupplierId.HasValue)
                query = query.Where(d => d.SupplierId == filter.SupplierId);

            if (!string.IsNullOrEmpty(filter.PurchaseDateFrom) &&
                DateTime.TryParse(filter.PurchaseDateFrom, out var purchaseFrom))
            {
                var fromDateOnly = DateOnly.FromDateTime(purchaseFrom);
                query = query.Where(d => d.PurchaseDate >= fromDateOnly);
            }

            if (!string.IsNullOrEmpty(filter.PurchaseDateTo) &&
                DateTime.TryParse(filter.PurchaseDateTo, out var purchaseTo))
            {
                var toDateOnly = DateOnly.FromDateTime(purchaseTo);
                query = query.Where(d => d.PurchaseDate <= toDateOnly);
            }

            var devices = await query.OrderBy(d => d.Name).ToListAsync();

            result.DeviceRows = devices.Select(d => new DeviceReportRow
            {
                DeviceCode = d.DeviceCode,
                Name = d.Name,
                ModelNumber = filter.IncludeModel ? d.ModelNumber : null,
                SerialNumber = d.SerialNumber,
                Manufacturer = filter.IncludeManufacturer ? d.Manufacturer?.Name : null,
                Supplier = filter.IncludeSupplier ? d.Supplier?.Name : null,
                Department = filter.IncludeDepartment ? d.Department?.Name : null,
                Location = filter.IncludeLocation ? d.Location : null,
                PurchaseDate = filter.IncludePurchaseDate ? d.PurchaseDate?.ToString("yyyy-MM-dd") : null,
                WarrantyStatus = filter.IncludeWarranty ? GetWarrantyStatus(d.DeviceWarranties) : null,
                Status = d.Status,
                RiskLevel = d.RiskLevel,
                NextMaintenance = filter.IncludeNextMaintenance ? d.NextMaintenanceDate?.ToString("yyyy-MM-dd") : null,
                FailureCount = filter.IncludeFailureCount ? d.FailureCount : 0
            }).ToList();
        }

        // ── بناء بيانات الصيانة
        private async Task BuildMaintenanceReportData(ReportFilterViewModel filter, ReportResultViewModel result)
        {
            var query = _db.MaintenanceRequests
                .Include(r => r.Device).ThenInclude(d => d.Department)
                .Include(r => r.AssignedEngineer)
                .Include(r => r.ReportedByStaff)
                .Include(r => r.MaintenanceType)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.MaintenanceStatus))
                query = query.Where(r => r.Status == filter.MaintenanceStatus);

            if (!string.IsNullOrEmpty(filter.MaintenanceRiskLevel))
                query = query.Where(r => r.RiskLevel == filter.MaintenanceRiskLevel);

            if (!string.IsNullOrEmpty(filter.MaintenanceType))
                query = query.Where(r => r.MaintenanceType != null && r.MaintenanceType.Name == filter.MaintenanceType);

            if (filter.AssignedEngineerId.HasValue)
                query = query.Where(r => r.AssignedEngineerId == filter.AssignedEngineerId);

            if (filter.ReportedByStaffId.HasValue)
                query = query.Where(r => r.ReportedByStaffId == filter.ReportedByStaffId);

            if (!string.IsNullOrEmpty(filter.RequestDateFrom) &&
                DateTime.TryParse(filter.RequestDateFrom, out var reqFrom))
                query = query.Where(r => r.RequestDate >= reqFrom);

            if (!string.IsNullOrEmpty(filter.RequestDateTo) &&
                DateTime.TryParse(filter.RequestDateTo, out var reqTo))
                query = query.Where(r => r.RequestDate <= reqTo);

            var requests = await query.OrderByDescending(r => r.RequestDate).ToListAsync();

            result.MaintenanceRows = requests.Select(r => new MaintenanceReportRow
            {
                RequestCode = r.RequestCode,
                RequestDate = filter.IncludeRequestDate ? r.RequestDate.ToString("yyyy-MM-dd") : string.Empty,
                DeviceName = r.Device?.Name ?? "N/A",
                DeviceCode = filter.IncludeDeviceName ? r.Device?.DeviceCode : null,
                IssueTitle = r.IssueTitle,
                IssueDescription = filter.IncludeIssueDescription ? r.IssueDescription : null,
                MaintenanceType = filter.IncludeType ? r.MaintenanceType?.Name : null,
                Engineer = filter.IncludeEngineer ? r.AssignedEngineer?.FullName : null,
                Reporter = filter.IncludeReporter ? r.ReportedByStaff?.FullName : null,
                Status = r.Status,
                RiskLevel = r.RiskLevel,
                StartedAt = filter.IncludeStartedAt ? r.StartedAt?.ToString("yyyy-MM-dd") : null,
                CompletedAt = filter.IncludeCompletedAt ? r.CompletedAt?.ToString("yyyy-MM-dd") : null,
                CompletionNotes = filter.IncludeCompletionNotes ? r.CompletionNotes : null,
                HasAlternative = r.HasAlternative
            }).ToList();
        }

        //  بناء الإحصائيات
        private void BuildStatistics(ReportResultViewModel result)
        {
            var stats = new ReportStatistics();

            if (result.ReportType == "device" || result.ReportType == "full")
            {
                stats.TotalRecords = result.DeviceRows.Count;
                stats.OperationalDevices = result.DeviceRows.Count(d => d.Status == "Operational" || d.Status == "Active");
                stats.MaintenanceNeeded = result.DeviceRows.Count(d => d.Status == "Maintenance Needed");
                stats.UnderMaintenance = result.DeviceRows.Count(d => d.Status == "Under Maintenance");
                stats.OutOfService = result.DeviceRows.Count(d =>
     d.Status.Equals("Out of Service", StringComparison.OrdinalIgnoreCase) ||
     d.Status.Equals("Out Of Service", StringComparison.OrdinalIgnoreCase));

                stats.StatusDistribution = result.DeviceRows
                    .GroupBy(d => d.Status)
                    .ToDictionary(g => g.Key, g => g.Count());

                stats.DepartmentDistribution = result.DeviceRows
                    .Where(d => !string.IsNullOrEmpty(d.Department))
                    .GroupBy(d => d.Department!)
                    .ToDictionary(g => g.Key, g => g.Count());

                stats.RiskDistribution = result.DeviceRows
                    .GroupBy(d => d.RiskLevel)
                    .ToDictionary(g => g.Key, g => g.Count());
            }

            if (result.ReportType == "maintenance" || result.ReportType == "full")
            {
                if (result.ReportType == "maintenance")
                    stats.TotalRecords = result.MaintenanceRows.Count;

                stats.PendingRequests = result.MaintenanceRows.Count(r => r.Status == "Pending");
                stats.InProgressRequests = result.MaintenanceRows.Count(r => r.Status == "In Progress");
                stats.CompletedRequests = result.MaintenanceRows.Count(r => r.Status == "Completed");

                var completedWithDates = result.MaintenanceRows
                    .Where(r => r.Status == "Completed" && !string.IsNullOrEmpty(r.CompletedAt) && !string.IsNullOrEmpty(r.RequestDate))
                    .Select(r => new
                    {
                        Req = DateTime.Parse(r.RequestDate),
                        Comp = DateTime.Parse(r.CompletedAt!)
                    }).ToList();

                if (completedWithDates.Any())
                    stats.AverageResolutionDays = completedWithDates.Average(x => (x.Comp - x.Req).TotalDays);

                stats.MonthlyTrend = result.MaintenanceRows
                    .Where(r => !string.IsNullOrEmpty(r.RequestDate))
                    .GroupBy(r => DateTime.Parse(r.RequestDate).ToString("yyyy-MM"))
                    .ToDictionary(g => g.Key, g => g.Count());
            }

            result.Statistics = stats;
        }

        //  بناء بيانات الرسوم البيانية 
        private void BuildChartData(ReportResultViewModel result, ReportFilterViewModel filter)
        {
            var chartData = new ChartData();
            var colors = new[] { "#16A34A", "#D97706", "#DC2626", "#0891B2", "#7C3AED", "#EC4899", "#F59E0B", "#10B981" };

            if (result.Statistics.StatusDistribution.Any())
            {
                int i = 0;
                chartData.PieChartData = result.Statistics.StatusDistribution
                    .Select(x => new ChartSlice
                    {
                        Label = x.Key,
                        Value = x.Value,
                        Color = colors[i++ % colors.Length]
                    }).ToList();
            }

            if (result.Statistics.DepartmentDistribution.Any())
            {
                int i = 0;
                chartData.BarChartData = result.Statistics.DepartmentDistribution
                    .OrderByDescending(x => x.Value)
                    .Take(10)
                    .Select(x => new ChartBar
                    {
                        Label = x.Key,
                        Value = x.Value,
                        Color = colors[i++ % colors.Length]
                    }).ToList();
            }

            if (result.Statistics.MonthlyTrend.Any())
            {
                chartData.LineChartData = result.Statistics.MonthlyTrend
                    .OrderBy(x => x.Key)
                    .Select(x => new ChartLine
                    {
                        Label = x.Key,
                        Value = x.Value
                    }).ToList();
            }

            result.ChartData = chartData;
        }

        //   الضمان 
        private string GetWarrantyStatus(ICollection<DeviceWarranty> warranties)
        {
            if (warranties == null || !warranties.Any()) return "No Warranty";

            var latest = warranties.OrderByDescending(w => w.EndDate).First();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (latest.EndDate < today) return "Expired";
            if (latest.EndDate.AddMonths(-3) <= today) return "Expiring Soon";
            return "Active";
        }

        //  PDF REPORT 
       
        private IActionResult GenerateProfessionalPdf(ReportResultViewModel result, ReportFilterViewModel filter)
        {
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4.Landscape());
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Segoe UI"));

                    //  HEADER 
                    page.Header().Element(header =>
                    {
                        header.Column(col =>
                        {
                            col.Item().Height(8).Background(QuestPDF.Helpers.Colors.Blue.Medium);

                            col.Item().PaddingVertical(16).Row(row =>
                            {
                                row.RelativeItem().Column(info =>
                                {
                                    info.Item().Text("SMEMS")
                                        .FontSize(24).Bold().FontColor(QuestPDF.Helpers.Colors.Blue.Medium);
                                    info.Item().Text("Smart Medical Equipment Management System")
                                        .FontSize(11).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                                });

                                row.ConstantItem(200).AlignRight().Column(meta =>
                                {
                                    meta.Item().Text(result.ReportTitle)
                                        .FontSize(14).Bold().FontColor(QuestPDF.Helpers.Colors.Grey.Darken3);
                                    meta.Item().Text($"Generated: {result.GeneratedAt:yyyy-MM-dd HH:mm}")
                                        .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                                    meta.Item().Text($"By: {result.GeneratedBy}")
                                        .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                                });
                            });

                            col.Item().Height(1).Background(QuestPDF.Helpers.Colors.Grey.Lighten2);
                        });
                    });

                    //  CONTENT 
                    page.Content().Element(content =>
                    {
                        content.Column(col =>
                        {
                            // ── SUMMARY CARDS ──
                            if (filter.IncludeSummary && result.Statistics != null)
                            {
                                col.Item().PaddingVertical(12).Row(row =>
                                {
                                    var stats = result.Statistics;

                                    if (result.ReportType == "device" || result.ReportType == "full")
                                    {
                                        row.RelativeItem().Element(c => DrawSummaryCard(c, "Total Devices",
                                            stats.TotalRecords.ToString(), QuestPDF.Helpers.Colors.Blue.Medium));
                                        row.RelativeItem().Element(c => DrawSummaryCard(c, "Operational",
                                            stats.OperationalDevices.ToString(), QuestPDF.Helpers.Colors.Green.Medium));
                                        row.RelativeItem().Element(c => DrawSummaryCard(c, "Maintenance Needed",
                                            stats.MaintenanceNeeded.ToString(), QuestPDF.Helpers.Colors.Orange.Medium));
                                        row.RelativeItem().Element(c => DrawSummaryCard(c, "Out of Service",
                                            stats.OutOfService.ToString(), QuestPDF.Helpers.Colors.Red.Medium));
                                    }

                                    if (result.ReportType == "maintenance" || result.ReportType == "full")
                                    {
                                        row.RelativeItem().Element(c => DrawSummaryCard(c, "Total Requests",
                                            stats.TotalRecords.ToString(), QuestPDF.Helpers.Colors.Purple.Medium));
                                        row.RelativeItem().Element(c => DrawSummaryCard(c, "Pending",
                                            stats.PendingRequests.ToString(), QuestPDF.Helpers.Colors.Orange.Medium));
                                        row.RelativeItem().Element(c => DrawSummaryCard(c, "In Progress",
                                            stats.InProgressRequests.ToString(), QuestPDF.Helpers.Colors.Blue.Medium));
                                        row.RelativeItem().Element(c => DrawSummaryCard(c, "Completed",
                                            stats.CompletedRequests.ToString(), QuestPDF.Helpers.Colors.Green.Medium));
                                    }
                                });
                            }

                            // ── CHARTS ──
                            if (filter.IncludeCharts && result.ChartData != null)
                            {
                                col.Item().PaddingVertical(12).Row(row =>
                                {
                                    if (result.ChartData.PieChartData.Any() &&
                                        (filter.ChartType == "pie" || filter.ChartType == "both"))
                                    {
                                        row.RelativeItem().Element(c => DrawPieChartPdf(c,
                                            "Status Distribution", result.ChartData.PieChartData));
                                    }

                                    if (result.ChartData.BarChartData.Any() &&
                                        (filter.ChartType == "bar" || filter.ChartType == "both"))
                                    {
                                        row.RelativeItem().Element(c => DrawBarChartPdf(c,
                                            "By Department", result.ChartData.BarChartData));
                                    }
                                });
                            }

                            // ── DEVICE TABLE ──
                            if (result.DeviceRows.Any())
                            {
                                col.Item().PaddingVertical(8).Text("Device Inventory")
                                    .FontSize(14).Bold().FontColor(QuestPDF.Helpers.Colors.Grey.Darken3);

                                col.Item().Element(tableContainer =>
                                {
                                    tableContainer.Table(table =>
                                    {
                                        var columns = GetDeviceColumns(filter);
                                        table.ColumnsDefinition(cols =>
                                        {
                                            foreach (var _ in columns)
                                                cols.RelativeColumn();
                                        });

                                        table.Header(header =>
                                        {
                                            foreach (var colName in columns)
                                            {
                                                header.Cell().Background(QuestPDF.Helpers.Colors.Blue.Medium)
                                                    .Padding(6).Text(colName).FontColor(QuestPDF.Helpers.Colors.White)
                                                    .Bold().FontSize(9);
                                            }
                                        });

                                        foreach (var device in result.DeviceRows)
                                        {
                                            var values = GetDeviceRowValues(device, filter);
                                            foreach (var val in values)
                                            {
                                                table.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2)
                                                    .Padding(5).Text(val).FontSize(8)
                                                    .FontColor(QuestPDF.Helpers.Colors.Grey.Darken3);
                                            }
                                        }
                                    });
                                });
                            }

                            // ── MAINTENANCE TABLE ──
                            if (result.MaintenanceRows.Any())
                            {
                                col.Item().PaddingTop(16).PaddingVertical(4).Text("Maintenance Records")
                                    .FontSize(14).Bold().FontColor(QuestPDF.Helpers.Colors.Grey.Darken3);

                                col.Item().Element(tableContainer =>
                                {
                                    tableContainer.Table(table =>
                                    {
                                        var columns = GetMaintenanceColumns(filter);
                                        table.ColumnsDefinition(cols =>
                                        {
                                            foreach (var _ in columns)
                                                cols.RelativeColumn();
                                        });

                                        table.Header(header =>
                                        {
                                            foreach (var colName in columns)
                                            {
                                                header.Cell().Background(QuestPDF.Helpers.Colors.Purple.Medium)
                                                    .Padding(6).Text(colName).FontColor(QuestPDF.Helpers.Colors.White)
                                                    .Bold().FontSize(9);
                                            }
                                        });

                                        foreach (var request in result.MaintenanceRows)
                                        {
                                            var values = GetMaintenanceRowValues(request, filter);
                                            foreach (var val in values)
                                            {
                                                table.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2)
                                                    .Padding(5).Text(val).FontSize(8)
                                                    .FontColor(QuestPDF.Helpers.Colors.Grey.Darken3);
                                            }
                                        }
                                    });
                                });
                            }

                            // ── FOOTER NOTES ──
                            if (!string.IsNullOrEmpty(filter.PdfTemplate))
                            {
                                col.Item().PaddingTop(20).Background(QuestPDF.Helpers.Colors.Grey.Lighten4)
                                    .Padding(12).Column(footer =>
                                    {
                                        footer.Item().Text("Report Configuration")
                                            .FontSize(10).Bold().FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);

                                        footer.Item().Text($"Template: {filter.PdfTemplate} | Charts: {(filter.IncludeCharts ? "Enabled" : "Disabled")} | Format: PDF")
                                            .FontSize(8).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                                    });
                            }
                        });
                    });

                    // ── FOOTER ──
                    page.Footer().AlignCenter().PaddingTop(8).Text(x =>
                    {
                        x.Span("Page ").FontSize(8).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                        x.CurrentPageNumber().FontSize(8).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                        x.Span(" of ").FontSize(8).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                        x.TotalPages().FontSize(8).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf",
                $"SMEMS_{result.ReportType}_Report_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

        private void DrawSummaryCard(IContainer container, string title, string value, string color)
        {
            container.Background(QuestPDF.Helpers.Colors.White)
                .Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2)
                .Padding(12).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(32).Height(32).Background(color)
                            .Element(e => e.AlignCenter().AlignMiddle().Text(title[0].ToString())
                                .FontSize(16).FontColor(QuestPDF.Helpers.Colors.White).Bold());

                        row.RelativeItem().PaddingLeft(8).Column(info =>
                        {
                            info.Item().Text(value).FontSize(20).Bold().FontColor(color);
                            info.Item().Text(title).FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                        });
                    });
                });
        }

        private void DrawPieChartPdf(IContainer container, string title, List<ChartSlice> data)
        {
            container.Background(QuestPDF.Helpers.Colors.White)
                .Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2)
                .Padding(12).Column(col =>
                {
                    col.Item().Text(title).FontSize(11).Bold().FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);

                    var total = data.Sum(x => x.Value);
                    if (total == 0) return;

                    //  مستطيلات ملونة تمثل النسب
                    col.Item().PaddingVertical(8).Row(row =>
                    {
                        foreach (var slice in data)
                        {
                            var pct = (slice.Value / (double)total) * 100;
                            row.RelativeItem((float)pct).Height(24).Background(slice.Color)
                                .Element(e => e.AlignCenter().AlignMiddle()
                                    .Text($"{slice.Label}").FontSize(7).FontColor(QuestPDF.Helpers.Colors.White).Bold());
                        }
                    });

                    
                    col.Item().PaddingTop(8).Row(row =>
                    {
                        int i = 0;
                        foreach (var slice in data)
                        {
                            if (i++ > 0 && i % 3 == 1)
                            {


                                row.RelativeItem().Row(legendRow =>
                                {
                                    legendRow.ConstantItem(10).Height(10).Background(slice.Color);
                                    legendRow.RelativeItem().PaddingLeft(4).Text($"{slice.Label}: {slice.Value}")
                                        .FontSize(8).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                                });
                            }
                        }
                    });
                });
        }

        // ── رسم Bar Chart في PDF 
        private void DrawBarChartPdf(IContainer container, string title, List<ChartBar> data)
        {
            container.Background(QuestPDF.Helpers.Colors.White)
                .Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2)
                .Padding(12).Column(col =>
                {
                    col.Item().Text(title).FontSize(11).Bold().FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);

                    var max = data.Max(x => x.Value);
                    if (max == 0) return;

                    foreach (var bar in data)
                    {
                        var pct = (bar.Value / (double)max) * 100;

                        col.Item().PaddingVertical(3).Row(row =>
                        {
                            row.ConstantItem(80).AlignRight().Text(bar.Label)
                                .FontSize(8).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);

                            row.RelativeItem().PaddingLeft(8).Height(16).Background(QuestPDF.Helpers.Colors.Grey.Lighten3)
                                .Row(track =>
                                {
                                    track.RelativeItem((float)pct).Height(16).Background(bar.Color)
                                        .Element(e => e.AlignRight().AlignMiddle().PaddingRight(4)
                                            .Text(bar.Value.ToString()).FontSize(7).FontColor(QuestPDF.Helpers.Colors.White).Bold());
                                });
                        });
                    }
                });
        }

        // ── أعمدة الجدول ──
        private List<string> GetDeviceColumns(ReportFilterViewModel filter)
        {
            var cols = new List<string>();
            if (filter.IncludeDeviceCode) cols.Add("Code");
            if (filter.IncludeName) cols.Add("Name");
            if (filter.IncludeModel) cols.Add("Model");
            if (filter.IncludeSerial) cols.Add("Serial");
            if (filter.IncludeManufacturer) cols.Add("Manufacturer");
            if (filter.IncludeSupplier) cols.Add("Supplier");
            if (filter.IncludeDepartment) cols.Add("Department");
            if (filter.IncludeLocation) cols.Add("Location");
            if (filter.IncludePurchaseDate) cols.Add("Purchase Date");
            if (filter.IncludeWarranty) cols.Add("Warranty");
            if (filter.IncludeStatusField) cols.Add("Status");
            if (filter.IncludeRisk) cols.Add("Risk");
            if (filter.IncludeNextMaintenance) cols.Add("Next Maint.");
            if (filter.IncludeFailureCount) cols.Add("Failures");
            return cols.Any() ? cols : new List<string> { "Code", "Name", "Status" };
        }

        private List<string> GetDeviceRowValues(DeviceReportRow device, ReportFilterViewModel filter)
        {
            var vals = new List<string>();
            if (filter.IncludeDeviceCode) vals.Add(device.DeviceCode);
            if (filter.IncludeName) vals.Add(device.Name);
            if (filter.IncludeModel) vals.Add(device.ModelNumber ?? "—");
            if (filter.IncludeSerial) vals.Add(device.SerialNumber);
            if (filter.IncludeManufacturer) vals.Add(device.Manufacturer ?? "—");
            if (filter.IncludeSupplier) vals.Add(device.Supplier ?? "—");
            if (filter.IncludeDepartment) vals.Add(device.Department ?? "—");
            if (filter.IncludeLocation) vals.Add(device.Location ?? "—");
            if (filter.IncludePurchaseDate) vals.Add(device.PurchaseDate ?? "—");
            if (filter.IncludeWarranty) vals.Add(device.WarrantyStatus ?? "—");
            if (filter.IncludeStatusField) vals.Add(device.Status);
            if (filter.IncludeRisk) vals.Add(device.RiskLevel);
            if (filter.IncludeNextMaintenance) vals.Add(device.NextMaintenance ?? "—");
            if (filter.IncludeFailureCount) vals.Add(device.FailureCount.ToString());
            return vals;
        }

        private List<string> GetMaintenanceColumns(ReportFilterViewModel filter)
        {
            var cols = new List<string>();
            if (filter.IncludeRequestCode) cols.Add("Request Code");
            if (filter.IncludeRequestDate) cols.Add("Date");
            if (filter.IncludeDeviceName) cols.Add("Device");
            if (filter.IncludeIssueTitle) cols.Add("Issue");
            if (filter.IncludeIssueDescription) cols.Add("Description");
            if (filter.IncludeType) cols.Add("Type");
            if (filter.IncludeEngineer) cols.Add("Engineer");
            if (filter.IncludeReporter) cols.Add("Reported By");
            if (filter.IncludeMaintenanceStatus) cols.Add("Status");
            if (filter.IncludeMaintenanceRisk) cols.Add("Risk");
            if (filter.IncludeStartedAt) cols.Add("Started");
            if (filter.IncludeCompletedAt) cols.Add("Completed");
            if (filter.IncludeCompletionNotes) cols.Add("Notes");
            if (filter.IncludeHasAlternative) cols.Add("Alt.");
            return cols.Any() ? cols : new List<string> { "Request Code", "Device", "Status" };
        }

        private List<string> GetMaintenanceRowValues(MaintenanceReportRow request, ReportFilterViewModel filter)
        {
            var vals = new List<string>();
            if (filter.IncludeRequestCode) vals.Add(request.RequestCode);
            if (filter.IncludeRequestDate) vals.Add(request.RequestDate);
            if (filter.IncludeDeviceName) vals.Add($"{request.DeviceName} ({request.DeviceCode})");
            if (filter.IncludeIssueTitle) vals.Add(request.IssueTitle);
            if (filter.IncludeIssueDescription) vals.Add(request.IssueDescription ?? "—");
            if (filter.IncludeType) vals.Add(request.MaintenanceType ?? "—");
            if (filter.IncludeEngineer) vals.Add(request.Engineer ?? "Unassigned");
            if (filter.IncludeReporter) vals.Add(request.Reporter ?? "—");
            if (filter.IncludeMaintenanceStatus) vals.Add(request.Status);
            if (filter.IncludeMaintenanceRisk) vals.Add(request.RiskLevel);
            if (filter.IncludeStartedAt) vals.Add(request.StartedAt ?? "—");
            if (filter.IncludeCompletedAt) vals.Add(request.CompletedAt ?? "—");
            if (filter.IncludeCompletionNotes) vals.Add(request.CompletionNotes ?? "—");
            if (filter.IncludeHasAlternative) vals.Add(request.HasAlternative ? "Yes" : "No");
            return vals;
        }

   
        //  EXCEL REPORT 
       
        private IActionResult GenerateExcelReport(ReportResultViewModel result, ReportFilterViewModel filter)
        {
            var sb = new StringBuilder();

            // BOM for UTF-8
            sb.Append('\uFEFF');

            // Title
            sb.AppendLine($"SMEMS - {result.ReportTitle}");
            sb.AppendLine($"Generated: {result.GeneratedAt:yyyy-MM-dd HH:mm} by {result.GeneratedBy}");
            sb.AppendLine();

            // Summary
            if (filter.IncludeSummary)
            {
                sb.AppendLine("SUMMARY");
                sb.AppendLine($"Total Records,{result.Statistics.TotalRecords}");
                if (result.ReportType == "device" || result.ReportType == "full")
                {
                    sb.AppendLine($"Operational,{result.Statistics.OperationalDevices}");
                    sb.AppendLine($"Maintenance Needed,{result.Statistics.MaintenanceNeeded}");
                    sb.AppendLine($"Out of Service,{result.Statistics.OutOfService}");
                }
                if (result.ReportType == "maintenance" || result.ReportType == "full")
                {
                    sb.AppendLine($"Pending,{result.Statistics.PendingRequests}");
                    sb.AppendLine($"In Progress,{result.Statistics.InProgressRequests}");
                    sb.AppendLine($"Completed,{result.Statistics.CompletedRequests}");
                    sb.AppendLine($"Avg Resolution (days),{result.Statistics.AverageResolutionDays:F1}");
                }
                sb.AppendLine();
            }

            // Device Data
            if (result.DeviceRows.Any())
            {
                var deviceCols = GetDeviceColumns(filter);
                sb.AppendLine(string.Join(",", deviceCols));

                foreach (var device in result.DeviceRows)
                {
                    var vals = GetDeviceRowValues(device, filter);
                    sb.AppendLine(string.Join(",", vals.Select(EscapeCsv)));
                }
                sb.AppendLine();
            }

            // Maintenance Data
            if (result.MaintenanceRows.Any())
            {
                var maintCols = GetMaintenanceColumns(filter);
                sb.AppendLine(string.Join(",", maintCols));

                foreach (var request in result.MaintenanceRows)
                {
                    var vals = GetMaintenanceRowValues(request, filter);
                    sb.AppendLine(string.Join(",", vals.Select(EscapeCsv)));
                }
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv; charset=utf-8",
                $"SMEMS_{result.ReportType}_Report_{DateTime.Now:yyyyMMdd_HHmm}.csv");
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "\"\"";
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }

        // ── Helper ──
        private async Task PopulateReportViewBags()
        {
            ViewBag.Departments = await _db.Departments.OrderBy(d => d.Name).ToListAsync();
            ViewBag.Manufacturers = await _db.Manufacturers.OrderBy(m => m.Name).ToListAsync();
            ViewBag.Suppliers = await _db.Suppliers.OrderBy(s => s.Name).ToListAsync();
            ViewBag.Engineers = await _db.Engineers.Where(e => e.IsActive).OrderBy(e => e.FullName).ToListAsync();
            ViewBag.Staff = await _db.Staff.Where(s => s.IsActive).OrderBy(s => s.FullName).ToListAsync();
            ViewBag.MaintenanceTypes = await _db.MaintenanceTypes.OrderBy(t => t.Name).ToListAsync();
        }

    }
    }
