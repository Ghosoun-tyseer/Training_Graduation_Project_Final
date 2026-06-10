using System;

namespace SMEMS.Models
{
    /// <summary>
    /// In-memory notification model (no database table).
    /// Stored in Session only.
    /// </summary>
    public class Notification
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string NotificationType { get; set; } = "General";
        public string Priority { get; set; } = "medium";
        public string RecipientRole { get; set; } = "all";  // admin | engineer | staff | all
        public int? RecipientId { get; set; }  // null = for everyone
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}