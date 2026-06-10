using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SMEMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace SMEMS.Helpers
{
    //  Constants 
    public static class SessionKeys
    {
        public const string UserId = "smems_user_id";
        public const string UserName = "smems_user_name";
        public const string UserEmail = "smems_user_email";
        public const string UserRole = "smems_user_role";
        public const string UserAvatar = "smems_user_avatar";
        public const string UserDept = "smems_user_dept";
        public const string UserFullName = "smems_user_fullname";
    }

    public static class Roles
    {
        public const string Admin = "admin";
        public const string Engineer = "engineer";
        public const string Staff = "staff";
    }

    public static class CookieKeys
    {
        public const string RememberUsername = "smems_remember_username";
      
    }

    //  Session Extension Helpers
    public static class SessionExtensions
    {
        public static bool IsLoggedIn(this ISession session)
            => !string.IsNullOrEmpty(session.GetString(SessionKeys.UserId));

        public static int GetUserId(this ISession session)
            => int.TryParse(session.GetString(SessionKeys.UserId), out var id) ? id : 0;

        public static string GetRole(this ISession session)
            => session.GetString(SessionKeys.UserRole) ?? string.Empty;

        public static string GetFullName(this ISession session)
            => session.GetString(SessionKeys.UserFullName) ?? string.Empty;

        public static string? GetAvatar(this ISession session)
            => session.GetString(SessionKeys.UserAvatar);
    }

    // Auth Attribute – requires login 
    public class RequireLoginAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

        public RequireLoginAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;

            if (!session.IsLoggedIn())
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (_allowedRoles.Length > 0)
            {
                var role = session.GetRole();
                if (!_allowedRoles.Contains(role))
                {
                    var redirect = role switch
                    {
                        Roles.Admin => new RedirectToActionResult("Index", "Admin", null),
                        Roles.Engineer => new RedirectToActionResult("Index", "Engineer", null),
                        Roles.Staff => new RedirectToActionResult("Index", "Staff", null),
                        _ => new RedirectToActionResult("Login", "Account", null)
                    };
                    context.Result = redirect;
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  Notification Session Helper (IN-MEMORY ONLY - No Database Table)
    // ═══════════════════════════════════════════════════════════════════════════
    public static class NotificationHelper
    {
        private const string SessionKey = "smems_notifications";
        private static int _idCounter = 0;

        // ── Get all notifications from Session ──
        public static List<Notification> GetNotifications(this ISession session)
        {
            var json = session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json))
                return new List<Notification>();

            try
            {
                return JsonSerializer.Deserialize<List<Notification>>(json) ?? new List<Notification>();
            }
            catch
            {
                return new List<Notification>();
            }
        }

        // ── Save notifications to Session ──
        private static void SaveNotifications(this ISession session, List<Notification> list)
        {
            var json = JsonSerializer.Serialize(list);
            session.SetString(SessionKey, json);
        }

        // ── Add a new notification ──
        public static void AddNotification(this ISession session, string title, string message,
            string recipientRole = "all", int? recipientId = null,
            string type = "General", string priority = "medium")
        {
            var list = session.GetNotifications();

            var notif = new Notification
            {
                NotificationId = Interlocked.Increment(ref _idCounter),
                Title = title,
                Message = message,
                NotificationType = type,
                Priority = priority,
                RecipientRole = recipientRole,
                RecipientId = recipientId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            // Keep only last 50 notifications
            list.Add(notif);
            if (list.Count > 50)
                list = list.Skip(list.Count - 50).ToList();

            session.SaveNotifications(list);
        }

        // ── Get notifications for a specific user role ──
        public static List<Notification> GetNotificationsForUser(this ISession session, string role, int? userId = null)
        {
            var list = session.GetNotifications();

            return list.Where(n =>
                n.RecipientRole == "all" ||
                n.RecipientRole == role ||
                (n.RecipientId.HasValue && n.RecipientId == userId)
            )
            .OrderByDescending(n => n.CreatedAt)
            .ToList();
        }

        // ── Count unread notifications ──
        public static int GetUnreadCount(this ISession session, string role, int? userId = null)
        {
            return session.GetNotificationsForUser(role, userId).Count(n => !n.IsRead);
        }

        // ── Mark notification as read ──
        public static bool MarkAsRead(this ISession session, int id)
        {
            var list = session.GetNotifications();
            var notif = list.FirstOrDefault(n => n.NotificationId == id);
            if (notif == null) return false;

            notif.IsRead = true;
            session.SaveNotifications(list);
            return true;
        }

        // ── Delete a notification ──
        public static bool DeleteNotification(this ISession session, int id)
        {
            var list = session.GetNotifications();
            var notif = list.FirstOrDefault(n => n.NotificationId == id);
            if (notif == null) return false;

            list.Remove(notif);
            session.SaveNotifications(list);
            return true;
        }

        // ── Seed default notifications (called once per session) ──
        public static void SeedDefaultNotifications(this ISession session)
        {
            var existing = session.GetNotifications();
            if (existing.Any()) return;

            session.AddNotification(
                "Welcome to SMEMS",
                "System is now running with in-memory notifications.",
                "all", null, "System", "low");

            session.AddNotification(
                "Maintenance Reminder",
                "Device DEV-1008 (CT Scanner) requires preventive maintenance.",
                "engineer", null, "Maintenance", "high");

            session.AddNotification(
                "New Request Submitted",
                "A new maintenance request has been submitted for your department.",
                "staff", null, "Request", "medium");
        }

        public static class StatusHelper
        {
            public static bool IsOutOfService(string? status) =>
                status?.Contains("Out of", StringComparison.OrdinalIgnoreCase) == true;

            public static string NormalizeStatus(string? status) =>
                IsOutOfService(status) ? "Out of Service" : status ?? "Unknown";
        }
    }


}