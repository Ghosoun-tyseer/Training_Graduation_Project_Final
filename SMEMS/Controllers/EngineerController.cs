using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMEMS.Helpers;
using SMEMS.Models;
using SMEMS.ViewModels;

namespace SMEMS.Controllers
{
    [RequireLogin(Roles.Engineer)]
    public class EngineerController : Controller
    {
        private readonly MyDbContext _db;

        public EngineerController(MyDbContext db)
        {
            _db = db;
        }

        // ══════════════════════════════════════════════════════
        //  Dashboard
        // ══════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var engId = HttpContext.Session.GetUserId();

            ViewBag.TotalAssigned = await _db.MaintenanceRequests.CountAsync(r => r.AssignedEngineerId == engId);
            ViewBag.Pending = await _db.MaintenanceRequests.CountAsync(r => r.AssignedEngineerId == engId && r.Status == "Pending");
            ViewBag.InProgress = await _db.MaintenanceRequests.CountAsync(r => r.AssignedEngineerId == engId && r.Status == "In Progress");
            ViewBag.Completed = await _db.MaintenanceRequests.CountAsync(r => r.AssignedEngineerId == engId && r.Status == "Completed");

            ViewBag.DevicesByStatus = await _db.Devices
                .GroupBy(d => d.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.RequestsByDept = await _db.MaintenanceRequests
                .Include(r => r.Device).ThenInclude(d => d.Department)
                .GroupBy(r => r.Device.Department.Name)
                .Select(g => new { Dept = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            var recentRequests = await _db.MaintenanceRequests
                .Include(r => r.Device).ThenInclude(d => d.Department)
                .Where(r => r.AssignedEngineerId == engId)
                .OrderByDescending(r => r.RequestDate)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentRequests = recentRequests;

            //  Session-based notifications
            HttpContext.Session.SeedDefaultNotifications();
            ViewBag.RecentNotifications = HttpContext.Session
                .GetNotificationsForUser(Roles.Engineer, engId)
                .Take(3)
                .ToList();

            return View();
        }

        // ══════════════════════════════════════════════════════
        //  MAINTENANCE REQUESTS
        // ══════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Maintenance(string? status, string? risk, string? search)
        {
            var engId = HttpContext.Session.GetUserId();

            var query = _db.MaintenanceRequests
                .Include(r => r.Device).ThenInclude(d => d.Department)
                .Include(r => r.ReportedByStaff)
                .Include(r => r.AssignedEngineer)
                .Where(r => r.AssignedEngineerId == engId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(r => r.IssueTitle.Contains(search) ||
                                         r.Device.Name.Contains(search));
            if (!string.IsNullOrEmpty(status)) query = query.Where(r => r.Status == status);
            if (!string.IsNullOrEmpty(risk)) query = query.Where(r => r.RiskLevel == risk);

            return View(await query.OrderByDescending(r => r.RequestDate).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> RequestDetail(int id)
        {
            var request = await _db.MaintenanceRequests
                .Include(r => r.Device).ThenInclude(d => d.Department)
                .Include(r => r.Device.Manufacturer)
                .Include(r => r.ReportedByStaff)
                .Include(r => r.AssignedEngineer)
                .Include(r => r.MaintenanceType)
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null) return NotFound();
            return View(request);
        }

        // ── Start Maintenance ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartMaintenance(int id)
        {
            var request = await _db.MaintenanceRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = "In Progress";
            request.StartedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;

            // Update device status
            var device = await _db.Devices.FindAsync(request.DeviceId);
            if (device != null) { device.Status = "Under Maintenance"; device.UpdatedAt = DateTime.UtcNow; }

            await _db.SaveChangesAsync();

            //  Notify admin
            HttpContext.Session.AddNotification(
                "Maintenance Started",
                $"Engineer has started working on request {request.RequestCode} for {device?.Name}.",
                "admin", null, "Maintenance", "medium");

            TempData["Success"] = "Maintenance started!";
            return RedirectToAction(nameof(Maintenance));
        }

        // ── Complete Maintenance (GET) ──
        [HttpGet]
        public async Task<IActionResult> CompleteMaintenance(int id)
        {
            var request = await _db.MaintenanceRequests
                .Include(r => r.Device)
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null) return NotFound();

            ViewBag.Request = request;
            ViewBag.MaintenanceTypes = await _db.MaintenanceTypes.ToListAsync();
            return View(new CompleteMaintenanceViewModel { RequestId = id });
        }

        // ── Complete Maintenance (POST) ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteMaintenance(CompleteMaintenanceViewModel vm)
        {
            ViewBag.MaintenanceTypes = await _db.MaintenanceTypes.ToListAsync();

            if (!ModelState.IsValid)
            {
                ViewBag.Request = await _db.MaintenanceRequests
                    .Include(r => r.Device).FirstOrDefaultAsync(r => r.RequestId == vm.RequestId);
                return View(vm);
            }

            var request = await _db.MaintenanceRequests.FindAsync(vm.RequestId);
            if (request == null) return NotFound();

            request.Status = "Completed";
            request.CompletionNotes = vm.CompletionNotes;
            request.EngineerNotes = vm.EngineerNotes;
            request.MaintenanceTypeId = vm.MaintenanceTypeId;
            request.CompletedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;

            // Update device status & failure count
            var device = await _db.Devices.FindAsync(request.DeviceId);
            if (device != null)
            {
                device.Status = "Operational";
                device.FailureCount++;
                device.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            //  Notify admin and reporting staff
            HttpContext.Session.AddNotification(
                "Maintenance Completed",
                $"Request {request.RequestCode} for {device?.Name} has been completed.",
                "admin", null, "Maintenance", "medium");

            if (request.ReportedByStaffId.HasValue)
            {
                HttpContext.Session.AddNotification(
                    "Your Request is Complete",
                    $"Your maintenance request for {device?.Name} has been resolved.",
                    "staff", request.ReportedByStaffId.Value, "Maintenance", "low");
            }

            TempData["Success"] = "Maintenance completed!";
            return RedirectToAction(nameof(Maintenance));
        }

        // ══════════════════════════════════════════════════════
        //  DEVICES (read-only for engineers)
        // ══════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Devices(string? search, string? status)
        {
            var query = _db.Devices
                .Include(d => d.Department)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(d => d.Name.Contains(search) || d.SerialNumber.Contains(search));
            if (!string.IsNullOrEmpty(status))
                query = query.Where(d => d.Status == status);

            return View(await query.OrderBy(d => d.Name).ToListAsync());
        }



        [HttpGet]
        public async Task<IActionResult> DeviceDetail(int id)
        {
            var device = await _db.Devices
                .Include(d => d.Department)
                .Include(d => d.Manufacturer)
                .Include(d => d.Supplier)
                .FirstOrDefaultAsync(d => d.DeviceId == id);

            if (device == null) return NotFound();

            ViewBag.ReturnController = "Engineer";  // ← مهم للزر "Back"
            return View("~/Views/Shared/_DeviceDetail.cshtml", device);
        }

        [HttpGet]
        public async Task<IActionResult> DeviceHistory(int deviceId)
        {
            var device = await _db.Devices.Include(d => d.Department).FirstOrDefaultAsync(d => d.DeviceId == deviceId);
            if (device == null) return NotFound();

            ViewBag.Device = device;
            ViewBag.ReturnController = "Engineer";  // ← مهم للزر "Back"

            var requests = await _db.MaintenanceRequests
                .Include(r => r.MaintenanceType)
                .Include(r => r.AssignedEngineer)
                .Where(r => r.DeviceId == deviceId)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View("~/Views/Shared/_DeviceHistory.cshtml", requests);
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

    }  
}      