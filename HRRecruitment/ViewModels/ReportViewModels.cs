using System;
using System.Collections.Generic;
using HRRecruitment.Models;

namespace HRRecruitment.ViewModels
{
    public class VacancyReportViewModel
    {
        public int TotalVacancies { get; set; }
        public int OpenVacancies { get; set; }
        public int ClosedVacancies { get; set; }
        public int SuspendedVacancies { get; set; }
        public int TotalOpenings { get; set; }
        public int FilledOpenings { get; set; }
        public List<Vacancy> Vacancies { get; set; }
    }

    public class InterviewReportViewModel
    {
        public int TotalInterviews { get; set; }
        public int ScheduledInterviews { get; set; }
        public int CompletedInterviews { get; set; }
        public int CancelledInterviews { get; set; }
        public int SelectedCount { get; set; }
        public int RejectedCount { get; set; }
        public int PendingResults { get; set; }
        public List<Interview> Interviews { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}