using System;
using System.ComponentModel.DataAnnotations;

namespace ScreenTimeMonitor.Models
{
    /// <summary>
    /// Represents a usage limit for an application
    /// </summary>
    public class UsageLimit
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string ApplicationName { get; set; } = string.Empty;
        
        /// <summary>
        /// Daily limit in seconds
        /// </summary>
        public int DailyLimitSeconds { get; set; }
        
        /// <summary>
        /// Whether this limit is currently active
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// Last time a warning was shown (to avoid spam)
        /// </summary>
        public DateTime? LastWarningShown { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Calculated property for daily limit as TimeSpan
        /// </summary>
        public TimeSpan DailyLimit => TimeSpan.FromSeconds(DailyLimitSeconds);
    }
}
