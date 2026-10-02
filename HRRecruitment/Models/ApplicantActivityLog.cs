using System;

namespace HRRecruitment.Models
{
    public class ApplicantActivityLog
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public string ActionType { get; set; }      // e.g., "Login", "ProfileUpdate", "PasswordChange"
        public string Description { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}