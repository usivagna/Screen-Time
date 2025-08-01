using System;
using System.ComponentModel.DataAnnotations;

namespace ScreenTimeMonitor.Models
{
    /// <summary>
    /// Represents a daily summary of screen time usage
    /// </summary>
    public class DailySummary
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        /// <summary>
        /// Total screen time for the day in seconds
        /// </summary>
        public int TotalScreenTimeSeconds { get; set; }
        
        [MaxLength(255)]
        public string? MostUsedApp { get; set; }
        
        /// <summary>
        /// Productivity score from 0.0 to 10.0
        /// </summary>
        public double ProductivityScore { get; set; }
        
        /// <summary>
        /// Number of different applications used
        /// </summary>
        public int AppsUsedCount { get; set; }
        
        /// <summary>
        /// Number of times the user switched between applications
        /// </summary>
        public int AppSwitchCount { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Calculated property for total screen time as TimeSpan
        /// </summary>
        public TimeSpan TotalScreenTime => TimeSpan.FromSeconds(TotalScreenTimeSeconds);
    }
}
