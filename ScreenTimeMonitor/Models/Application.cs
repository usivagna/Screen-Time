using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ScreenTimeMonitor.Models
{
    /// <summary>
    /// Represents an application that can be monitored for screen time
    /// </summary>
    public class Application
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? ExecutablePath { get; set; }
        
        [MaxLength(100)]
        public string Category { get; set; } = "Uncategorized";
        
        [MaxLength(500)]
        public string? IconPath { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public ICollection<UsageSession> UsageSessions { get; set; } = new List<UsageSession>();
    }
}
