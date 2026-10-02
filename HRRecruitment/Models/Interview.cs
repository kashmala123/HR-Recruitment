using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRRecruitment.Models
{
    [Table("Interviews")]
    public class Interview
    {
        [Key]
        public int InterviewId { get; set; }

        [Required]
        [StringLength(50)]
        public string InterviewNumber { get; set; }

        [ForeignKey("ApplicantVacancy")]
        public int ApplicantVacancyId { get; set; }

        [ForeignKey("Interviewer")]
        public int InterviewerId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime InterviewDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Required]
        [StringLength(20)]
        public string InterviewMode { get; set; } = "Offline";

        [StringLength(500)]
        public string MeetingLink { get; set; }

        public int InterviewRound { get; set; } = 1;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Scheduled";

        [StringLength(20)]
        public string Result { get; set; } = "Pending";

        public string Feedback { get; set; }

        public string CancellationReason { get; set; }

        [ForeignKey("CreatedByEmployee")]
        public int? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? CompletedDate { get; set; }

        // Navigation Properties
        public virtual ApplicantVacancy ApplicantVacancy { get; set; }
        public virtual Employee Interviewer { get; set; }
        public virtual Employee CreatedByEmployee { get; set; }
    }
}