namespace HRRecruitment.Models
{
    public class UserActivityLog
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; } // FK to Employee
        public string ActionType { get; set; } // "Login", "ProfileUpdate", "PasswordChange", "InterviewUpdate"
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
    }
}