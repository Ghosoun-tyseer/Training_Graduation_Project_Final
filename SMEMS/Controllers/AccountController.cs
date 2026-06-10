using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMEMS.Helpers;
using SMEMS.Models;
using SMEMS.ViewModels;
using BCrypt.Net;

namespace SMEMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly MyDbContext _db;
        private readonly IWebHostEnvironment _env;

        public AccountController(MyDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        
        //  GET  /Account/Login
      
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.IsLoggedIn())
                return RedirectToDashboard(HttpContext.Session.GetRole());

            var vm = new LoginViewModel();

            // Restore remembered username from cookie (role removed)
            if (Request.Cookies.TryGetValue(CookieKeys.RememberUsername, out var savedUsername))
            {
                vm.Username = savedUsername;
                vm.RememberMe = true;
            }

            return View(vm);
        }

        //  POST  /Account/Login  (Auto-detect role)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // ── Auto-detect user across all tables 
            var (userFound, role, passwordHash, userId, fullName, email, avatar, dept)
                = await FindUserAcrossAllTablesAsync(vm.Username);

            if (!userFound)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password");
                return View(vm);
            }

            //  Validate password 
            if (passwordHash == null || !BCrypt.Net.BCrypt.Verify(vm.Password, passwordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password");
                return View(vm);
            }

            // Save to Session 
            HttpContext.Session.SetString(SessionKeys.UserId, userId.ToString());
            HttpContext.Session.SetString(SessionKeys.UserFullName, fullName);
            HttpContext.Session.SetString(SessionKeys.UserEmail, email);
            HttpContext.Session.SetString(SessionKeys.UserRole, role);
            HttpContext.Session.SetString(SessionKeys.UserAvatar, avatar ?? string.Empty);
            HttpContext.Session.SetString(SessionKeys.UserDept, dept ?? string.Empty);

            //  Remember Me cookie 
            if (vm.RememberMe)
            {
                var opts = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(30),
                    HttpOnly = true,
                    Secure = true
                };
                Response.Cookies.Append(CookieKeys.RememberUsername, vm.Username, opts);
            }
            else
            {
                Response.Cookies.Delete(CookieKeys.RememberUsername);
            }

            // ✅ Seed default notifications on login
            HttpContext.Session.SeedDefaultNotifications();

            return RedirectToDashboard(role);
        }

        //
        //  GET  /Account/Register  (Admin only – add new users)
        // 
        [HttpGet]
        [RequireLogin(Roles.Admin)]
        public async Task<IActionResult> Register()
        {
            ViewBag.Departments = await _db.Departments.OrderBy(d => d.Name).ToListAsync();
            return View(new RegisterViewModel());
        }

         
        //  POST  /Account/Register
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireLogin(Roles.Admin)]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            ViewBag.Departments = await _db.Departments.OrderBy(d => d.Name).ToListAsync();

            if (!ModelState.IsValid)
                return View(vm);

            //  Check if username / email already exist 
            bool userExists = vm.Role switch
            {
                Roles.Admin => await _db.Admins.AnyAsync(a => a.Username == vm.Username || a.Email == vm.Email),
                Roles.Engineer => await _db.Engineers.AnyAsync(e => e.Username == vm.Username || e.Email == vm.Email),
                Roles.Staff => await _db.Staff.AnyAsync(s => s.Username == vm.Username || s.Email == vm.Email),
                _ => false
            };

            if (userExists)
            {
                ModelState.AddModelError(string.Empty, "Username or email is already in use");
                return View(vm);
            }

            //  Upload avatar 
            string? avatarPath = await SaveAvatarAsync(vm.AvatarFile);

            // Hash password & save 
            string hash = BCrypt.Net.BCrypt.HashPassword(vm.Password);

            switch (vm.Role)
            {
                case Roles.Admin:
                    _db.Admins.Add(new Admin
                    {
                        FullName = vm.FullName,
                        Username = vm.Username,
                        Email = vm.Email,
                        Phone = vm.Phone,
                        PasswordHash = hash,
                        Avatar = avatarPath,
                        Position = vm.JobTitle,
                         IsActive = true,          
                        CreatedAt = DateTime.UtcNow
                    });
                    break;

                case Roles.Engineer:
                    _db.Engineers.Add(new Engineer
                    {
                        FullName = vm.FullName,
                        Username = vm.Username,
                        Email = vm.Email,
                        Phone = vm.Phone,
                        PasswordHash = hash,
                        Avatar = avatarPath,
                        Position = vm.JobTitle,
                        IsActive = true,           
                        CreatedAt = DateTime.UtcNow
                    });
                    break;

                case Roles.Staff:
                    _db.Staff.Add(new Staff
                    {
                        FullName = vm.FullName,
                        Username = vm.Username,
                        Email = vm.Email,
                        Phone = vm.Phone,
                        PasswordHash = hash,
                        Avatar = avatarPath,
                        Position = vm.JobTitle,
                        DepartmentId = vm.DepartmentId,
                        IsActive = true,         
                        CreatedAt = DateTime.UtcNow
                    });
                    break;
            }

            await _db.SaveChangesAsync();

            //  Notify all admins
            HttpContext.Session.AddNotification(
                "New User Registered",
                $"User '{vm.FullName}' has been added as {vm.Role}.",
                "admin", null, "User", "low");

            TempData["Success"] = $"User '{vm.FullName}' created successfully!";
            return RedirectToAction("Users", "Admin");
        }

        
        //  GET  /Account/Profile
       
        [HttpGet]
        [RequireLogin]
        public async Task<IActionResult> Profile()
        {
            var vm = await BuildProfileVmAsync();
            return View(vm);
        }

       
        //  GET  /Account/EditProfile
        
        [HttpGet]
        [RequireLogin]
        public async Task<IActionResult> EditProfile()
        {
            var role = HttpContext.Session.GetRole();
            var userId = HttpContext.Session.GetUserId();
            var vm = new EditProfileViewModel { UserId = userId, Role = role };

            ViewBag.Departments = await _db.Departments.OrderBy(d => d.Name).ToListAsync();


            switch (role)
            {
                case Roles.Admin:
                    var a = await _db.Admins.FindAsync(userId);
                    if (a == null) return NotFound();
                    vm.FullName = a.FullName; vm.Username = a.Username;
                    vm.Email = a.Email; vm.Phone = a.Phone; vm.CurrentAvatar = a.Avatar;
                    break;

                case Roles.Engineer:
                    var e = await _db.Engineers.FindAsync(userId);
                    if (e == null) return NotFound();
                    vm.FullName = e.FullName; vm.Username = e.Username;
                    vm.Email = e.Email; vm.Phone = e.Phone;
                    vm.JobTitle = e.Position; vm.CurrentAvatar = e.Avatar;
                    break;

                case Roles.Staff:
                    var s = await _db.Staff.FindAsync(userId);
                    if (s == null) return NotFound();
                    vm.FullName = s.FullName; vm.Username = s.Username;
                    vm.Email = s.Email; vm.Phone = s.Phone;
                    vm.JobTitle = s.Position; vm.CurrentAvatar = s.Avatar;
                    vm.DepartmentId = s.DepartmentId;
                    break;
            }

            return View(vm);
        }

   
        //  POST  /Account/EditProfile
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireLogin]
        public async Task<IActionResult> EditProfile(EditProfileViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var role = HttpContext.Session.GetRole();
            var userId = HttpContext.Session.GetUserId();
            string? newAvatarPath = null;

            if (vm.AvatarFile != null)
                newAvatarPath = await SaveAvatarAsync(vm.AvatarFile);

            switch (role)
            {
                case Roles.Admin:
                    var a = await _db.Admins.FindAsync(userId);
                    if (a == null) return NotFound();

                    if (await _db.Admins.AnyAsync(x => x.AdminId != userId &&
                        (x.Username == vm.Username || x.Email == vm.Email)))
                    {
                        ModelState.AddModelError(string.Empty, "Username or email already used");
                        return View(vm);
                    }
                    a.FullName = vm.FullName; a.Username = vm.Username;
                    a.Email = vm.Email; a.Phone = vm.Phone;
                    a.UpdatedAt = DateTime.UtcNow;
                    if (newAvatarPath != null) a.Avatar = newAvatarPath;
                    break;

                case Roles.Engineer:
                    var e = await _db.Engineers.FindAsync(userId);
                    if (e == null) return NotFound();
                    if (await _db.Engineers.AnyAsync(x => x.EngineerId != userId &&
                        (x.Username == vm.Username || x.Email == vm.Email)))
                    {
                        ModelState.AddModelError(string.Empty, "Username or email already used");
                        return View(vm);
                    }
                    e.FullName = vm.FullName; e.Username = vm.Username;
                    e.Email = vm.Email; e.Phone = e.Phone;
                    e.Position = vm.JobTitle; e.UpdatedAt = DateTime.UtcNow;
                    if (newAvatarPath != null) e.Avatar = newAvatarPath;
                    break;

                case Roles.Staff:
                    var s = await _db.Staff.FindAsync(userId);
                    if (s == null) return NotFound();
                    if (await _db.Staff.AnyAsync(x => x.StaffId != userId &&
                        (x.Username == vm.Username || x.Email == vm.Email)))
                    {
                        ModelState.AddModelError(string.Empty, "Username or email already used");
                        return View(vm);
                    }
                    s.FullName = vm.FullName; s.Username = vm.Username;
                    s.Email = vm.Email; s.Phone = vm.Phone;
                    s.Position = vm.JobTitle; s.UpdatedAt = DateTime.UtcNow;
                    s.DepartmentId = vm.DepartmentId;
                    if (newAvatarPath != null) s.Avatar = newAvatarPath;
                    break;
            }

            await _db.SaveChangesAsync();

            // Update session
            HttpContext.Session.SetString(SessionKeys.UserFullName, vm.FullName);
            HttpContext.Session.SetString(SessionKeys.UserEmail, vm.Email);
            if (newAvatarPath != null)
                HttpContext.Session.SetString(SessionKeys.UserAvatar, newAvatarPath);

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }

        
        //  GET  /Account/ChangePassword
       
        [HttpGet]
        [RequireLogin]
        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

       
        //  POST  /Account/ChangePassword
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireLogin]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var role = HttpContext.Session.GetRole();
            var userId = HttpContext.Session.GetUserId();

            string? currentHash = null;
            switch (role)
            {
                case Roles.Admin: currentHash = (await _db.Admins.FindAsync(userId))?.PasswordHash; break;
                case Roles.Engineer: currentHash = (await _db.Engineers.FindAsync(userId))?.PasswordHash; break;
                case Roles.Staff: currentHash = (await _db.Staff.FindAsync(userId))?.PasswordHash; break;
            }

            if (currentHash == null || !BCrypt.Net.BCrypt.Verify(vm.CurrentPassword, currentHash))
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect");
                return View(vm);
            }

            string newHash = BCrypt.Net.BCrypt.HashPassword(vm.NewPassword);
            switch (role)
            {
                case Roles.Admin:
                    var a = (await _db.Admins.FindAsync(userId))!;
                    a.PasswordHash = newHash; a.UpdatedAt = DateTime.UtcNow;
                    break;
                case Roles.Engineer:
                    var e = (await _db.Engineers.FindAsync(userId))!;
                    e.PasswordHash = newHash; e.UpdatedAt = DateTime.UtcNow;
                    break;
                case Roles.Staff:
                    var s = (await _db.Staff.FindAsync(userId))!;
                    s.PasswordHash = newHash; s.UpdatedAt = DateTime.UtcNow;
                    break;
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Password changed successfully!";
            return RedirectToAction(nameof(Profile));
        }

        //  GET  /Account/ForgotPassword
       
        [HttpGet]
        public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

      
        //  POST  /Account/ForgotPassword
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // Check all tables
            bool exists =
                await _db.Admins.AnyAsync(a => a.Email == vm.Email) ||
                await _db.Engineers.AnyAsync(e => e.Email == vm.Email) ||
                await _db.Staff.AnyAsync(s => s.Email == vm.Email);

            // Always show success to prevent email enumeration
            TempData["Success"] = exists
                ? $"A reset link has been sent to {vm.Email}"
                : $"If {vm.Email} is registered, you will receive a reset link";

            return RedirectToAction(nameof(Login));
        }

       
        //  POST  /Account/Logout
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }


        //  Private helpers

        /// <summary>
        /// Auto-detects user across all three tables (Admin, Engineer, Staff)
        /// Returns tuple with user data and detected role
        /// </summary>
        private async Task<(bool found, string role, string? passwordHash, int userId,
       string fullName, string email, string? avatar, string? dept)>
       FindUserAcrossAllTablesAsync(string username)
        {
            // 1. Check Admin table
            var admin = await _db.Admins
                .FirstOrDefaultAsync(a => a.Username == username && a.IsActive && !a.IsDeleted);
            if (admin != null)
            {
                return (true, Roles.Admin, admin.PasswordHash, admin.AdminId,
                    admin.FullName, admin.Email, admin.Avatar, null);
            }

            // 2. Check Engineer table
            var engineer = await _db.Engineers
                .FirstOrDefaultAsync(e => e.Username == username && e.IsActive && !e.IsDeleted);
            if (engineer != null)
            {
                return (true, Roles.Engineer, engineer.PasswordHash, engineer.EngineerId,
                    engineer.FullName, engineer.Email, engineer.Avatar, null);
            }

            // 3. Check Staff table
            var staff = await _db.Staff
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.Username == username && s.IsActive && !s.IsDeleted);
            if (staff != null)
            {
                return (true, Roles.Staff, staff.PasswordHash, staff.StaffId,
                    staff.FullName, staff.Email, staff.Avatar, staff.Department?.Name);
            }

            // User not found in any table
            return (false, string.Empty, null, 0, string.Empty, string.Empty, null, null);
        }
        private IActionResult RedirectToDashboard(string role) => role switch
        {
            Roles.Admin => RedirectToAction("Index", "Admin"),
            Roles.Engineer => RedirectToAction("Index", "Engineer"),
            Roles.Staff => RedirectToAction("Index", "Staff"),
            _ => RedirectToAction(nameof(Login))
        };

        private async Task<ProfileViewModel> BuildProfileVmAsync()
        {
            var role = HttpContext.Session.GetRole();
            var userId = HttpContext.Session.GetUserId();
            var vm = new ProfileViewModel { UserId = userId, Role = role };

            switch (role)
            {
                case Roles.Admin:
                    var a = await _db.Admins.FirstOrDefaultAsync(x => x.AdminId == userId);
                    if (a != null)
                    {
                        vm.FullName = a.FullName; vm.Username = a.Username;
                        vm.Email = a.Email; vm.Phone = a.Phone;
                        vm.Avatar = a.Avatar;
                        vm.JobTitle = a.Position; vm.CreatedAt = a.CreatedAt;
                    }
                    break;
                case Roles.Engineer:
                    var e = await _db.Engineers.FirstOrDefaultAsync(x => x.EngineerId == userId);
                    if (e != null)
                    {
                        vm.FullName = e.FullName;
                        vm.Username = e.Username;
                        vm.Email = e.Email;
                        vm.Phone = e.Phone;
                        vm.Avatar = e.Avatar;
                        vm.JobTitle = e.Position;
                        vm.CreatedAt = e.CreatedAt;
                    }
                    break;

                case Roles.Staff:
                    var s = await _db.Staff.Include(x => x.Department).FirstOrDefaultAsync(x => x.StaffId == userId);
                    if (s != null)
                    {
                        vm.FullName = s.FullName; vm.Username = s.Username;
                        vm.Email = s.Email; vm.Phone = s.Phone;
                        vm.Avatar = s.Avatar; vm.Department = s.Department?.Name;
                        vm.JobTitle = s.Position; vm.CreatedAt = s.CreatedAt;
                    }
                    break;
            }

            return vm;
        }

        private async Task<string?> SaveAvatarAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext)) return null;

            if (file.Length > 2 * 1024 * 1024) return null; // max 2 MB

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsDir, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/avatars/{fileName}";
        }
    }
}