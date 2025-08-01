using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScreenTimeMonitor.Models
{
    /// <summary>
    /// Represents a session where an application was actively used
    /// </summary>
    public class UsageSession
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ApplicationId { get; set; }
        
        [Required]
        public DateTime StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        /// <summary>
        /// Duration of the session in seconds
        /// </summary>
        public int DurationSeconds { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        /// <summary>
        /// Window title when the session started
        /// </summary>
        [MaxLength(500)]
        public string? WindowTitle { get; set; }
        
        /// <summary>
        /// Whether this session is currently active
        /// </summary>
        public bool IsActive { get; set; }
        
        // Navigation property
        [ForeignKey(nameof(ApplicationId))]
        public Application Application { get; set; } = null!;
        
        /// <summary>
        /// Calculated property for duration as TimeSpan
        /// </summary>
        [NotMapped]
        public TimeSpan Duration => TimeSpan.FromSeconds(DurationSeconds);
    }
}
