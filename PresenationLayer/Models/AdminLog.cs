using System;

namespace DCAssignment2.Models
{
    public class AdminLog
    {
        public int Id { get; set; }               // Optional, PK
        public DateTime Timestamp { get; set; }   // When the action occurred
        public string AdminName { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string Details { get; set; } = null!;
    }
}
