using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMEMS.Models;
using SMEMS.Helpers;
using SMEMS.ViewModels;

namespace SMEMS.Controllers
{
    [RequireLogin(Roles.Staff)]
    public class StaffController : Controller
    {
        private readonly MyDbContext _db;

        public StaffController(MyDbContext db)
        {
            _db = db;
        }

        // ══════════════════════════════════════════════════════
        //  Dashboard
        // ══════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var staffId = HttpContext.Session.GetUserId();
            var staff = await _db.Staff.Include(s => s.Department).FirstOrDefaultAsync(s => s.StaffId == staffId);
            var deptId = staff?.DepartmentId;

            var deptDevices = await _db.Devices
                .AsNoTracking()
                .Include(d => d.Department)
                .Where(d => deptId == null || d.DepartmentId == deptId)
                .ToListAsync();

            ViewBag.TotalDevices = deptDevices.Count;
            ViewBag.Available = deptDevices.Count(d =>
                d.Status?.Equals("Operational", StringComparison.OrdinalIgnoreCase) == true);

            ViewBag.MaintenanceNeeded = deptDevices.Count(d =>
                d.Status?.Equals("Maintenance Needed", StringComparison.OrdinalIgnoreCase) == true ||
                d.Status?.Equals("MaintenanceNeeded", StringComparison.OrdinalIgnoreCase) == true);

            ViewBag.OutOfService = deptDevices.Count(d =>
     d.Status != null && d.Status.Contains("Out of", StringComparison.OrdinalIgnoreCase));


            ViewBag.Devices = deptDevices;

            ViewBag.MyRequests = await _db.MaintenanceRequests
                .Include(r => r.Device)
                .Include(r => r.AssignedEngineer)
                .Where(r => r.ReportedByStaffId == staffId)
                .OrderByDescending(r => r.RequestDate)
                .Take(5)
                .ToListAsync();

            //  Session-based notifications
            HttpContext.Session.SeedDefaultNotifications();
            ViewBag.RecentNotifications = HttpContext.Session
                .GetNotificationsForUser(Roles.Staff, staffId)
                .Take(5)
                .ToList();

            return View();
        }

        // ══════════════════════════════════════════════════════
        //  MY DEVICES (department)
        // ══════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Devices(string? search, string? status, string? risk)
        {
            var staffId = HttpContext.Session.GetUserId();
            var staff = await _db.Staff.FindAsync(staffId);
            var deptId = staff?.DepartmentId;

            var query = _db.Devices
                .Include(d => d.Department)
                .Where(d => deptId == null || d.DepartmentId == deptId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(d => d.Name.Contains(search) || d.SerialNumber.Contains(search));
        
            if (!string.IsNullOrEmpty(status) && status.Contains("Out of", StringComparison.OrdinalIgnoreCase))
                query = query.Where(d => d.Status.Contains("Out of"));
            else if (!string.IsNullOrEmpty(status))
                query = query.Where(d => d.Status == status);

            if (!string.IsNullOrEmpty(risk))
                query = query.Where(d => d.RiskLevel == risk);

            return View(await query.OrderBy(d => d.Name).ToListAsync());
        }

        // ══════════════════════════════════════════════════════
        //  MAINTENANCE REQUESTS
        // ══════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> MyRequests(string? status)
        {
            var staffId = HttpContext.Session.GetUserId();

            var query = _db.MaintenanceRequests
                .Include(r => r.Device).ThenInclude(d => d.Department)
                .Include(r => r.AssignedEngineer)
                .Where(r => r.ReportedByStaffId == staffId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status == status);

            return View(await query.OrderByDescending(r => r.RequestDate).ToListAsync());
        }

        // ── Submit Request (GET) ──
        [HttpGet]
        public async Task<IActionResult> SubmitRequest(int? deviceId)
        {
            var staffId = HttpContext.Session.GetUserId();
            var staff = await _db.Staff.FindAsync(staffId);
            var deptId = staff?.DepartmentId;

            ViewBag.Devices = await _db.Devices
                .Where(d => (deptId == null || d.DepartmentId == deptId) && d.Status != "Out of Service")
                .OrderBy(d => d.Name)
                .ToListAsync();

            return View(new MaintenanceRequestViewModel { DeviceId = deviceId ?? 0 });
        }

        // ── Submit Request (POST) ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRequest(MaintenanceRequestViewModel vm)
        {
            var staffId = HttpContext.Session.GetUserId();
            var staff = await _db.Staff.FindAsync(staffId);

            ViewBag.Devices = await _db.Devices
                .Where(d => (staff == null || d.DepartmentId == staff.DepartmentId) && d.Status != "Out of Service")
                .OrderBy(d => d.Name)
                .ToListAsync();

            if (!ModelState.IsValid) return View(vm);

            // Generate unique request code
            var code = $"REQ-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..5].ToUpper()}";

            _db.MaintenanceRequests.Add(new MaintenanceRequest
            {
                RequestCode = code,
                DeviceId = vm.DeviceId,
                ReportedByStaffId = staffId,
                IssueTitle = vm.IssueTitle,
                IssueDescription = vm.IssueDescription,
                RiskLevel = vm.RiskLevel,
                HasAlternative = vm.HasAlternative,
                Status = "Pending"
            });

            // Update device status
            var device = await _db.Devices.FindAsync(vm.DeviceId);
            if (device != null)
            {
                device.Status = "Maintenance Needed";
                device.FailureCount++;
                device.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            // Notify admin and engineers
            HttpContext.Session.AddNotification(
                "New Maintenance Request",
                $"New request submitted for {device?.Name}: {vm.IssueTitle}",
                "admin", null, "Request", "high");

            HttpContext.Session.AddNotification(
                "New Assignment Available",
                $"A new maintenance request for {device?.Name} is pending assignment.",
                "engineer", null, "Request", "medium");

            TempData["Success"] = "Maintenance request submitted successfully!";
            return RedirectToAction(nameof(MyRequests));
        }



        // ══════════════════════════════════════════════════════
        //  NOTIFICATIONS (Session-based, no database table)
        // ══════════════════════════════════════════════════════
       
        [HttpGet]
        public IActionResult Notifications(string? filter)  
        {
            var role = HttpContext.Session.GetRole();
            var userId = HttpContext.Session.GetUserId();

            HttpContext.Session.SeedDefaultNotifications();

            var allNotifs = HttpContext.Session.GetNotificationsForUser(role, userId);

           
            var notifs = filter switch
            {
                "unread" => allNotifs.Where(n => !n.IsRead),
                "read" => allNotifs.Where(n => n.IsRead),
                _ => allNotifs
            };

            ViewBag.CurrentFilter = filter ?? "all"; 

            return View(notifs.ToList());
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


        [HttpGet]
        public async Task<IActionResult> DeviceDetail(int id)
        {
            var staffId = HttpContext.Session.GetUserId();
            var staff = await _db.Staff.FindAsync(staffId);
            var deptId = staff?.DepartmentId;

            var device = await _db.Devices
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.DeviceId == id);

            if (device == null) return NotFound();
            if (deptId.HasValue && device.DepartmentId != deptId) return Forbid();

            ViewBag.ReturnController = "Staff";  
            return View("~/Views/Shared/_DeviceDetail.cshtml", device);
        }

        [HttpGet]
        public async Task<IActionResult> DeviceHistory(int deviceId)
        {
            var device = await _db.Devices.Include(d => d.Department).FirstOrDefaultAsync(d => d.DeviceId == deviceId);
            if (device == null) return NotFound();

            ViewBag.Device = device;
            ViewBag.ReturnController = "Staff";  

            var requests = await _db.MaintenanceRequests
                .Include(r => r.MaintenanceType)
                .Include(r => r.AssignedEngineer)
                .Where(r => r.DeviceId == deviceId && r.Status == "Completed")
                .OrderByDescending(r => r.CompletedAt)
                .ToListAsync();

            return View("~/Views/Shared/_DeviceHistory.cshtml", requests);
        }

    }


}