using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRRecruitment.ViewModels
{
    public class ScheduleInterviewViewModel
    {
        public int ApplicantVacancyId { get; set; }
        public string ApplicantName { get; set; }
        public string JobTitle { get; set; }

        [Required(ErrorMessage = "Please select an interviewer")]
        public int InterviewerId { get; set; }

        [Required(ErrorMessage = "Interview date is required")]
        [DataType(DataType.Date)]
        [FutureDate]
        public DateTime InterviewDate { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        [DataType(DataType.Time)]
        [EndTimeAfterStartTime]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Please select interview mode")]
        public string InterviewMode { get; set; }

        [Url]
        public string MeetingLink { get; set; }

        public int InterviewRound { get; set; } = 1;

        public SelectList InterviewersList { get; set; }
        public SelectList InterviewModes { get; set; }
    }

    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is DateTime date)
                return date.Date > DateTime.Today;
            return false;
        }
    }

    public class EndTimeAfterStartTimeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            var model = (ScheduleInterviewViewModel)context.ObjectInstance;
            if (model.EndTime <= model.StartTime)
                return new ValidationResult("End time must be after start time");
            return ValidationResult.Success;
        }
    }
}