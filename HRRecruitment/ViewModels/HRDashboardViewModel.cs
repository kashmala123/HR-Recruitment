using System.Collections.Generic;
using HRRecruitment.Models;

namespace HRRecruitment.ViewModels
{
    public class HRDashboardViewModel
    {
        public Employee CurrentHR { get; set; }
        public int TotalVacancies { get; set; }
        public int OpenVacancies { get; set; }
        public int ClosedVacancies { get; set; }
        public int SuspendedVacancies { get; set; }
        public int TotalApplicants { get; set; }
        public int ApplicantsInProcess { get; set; }
        public int HiredApplicants { get; set; }
        public int TodayInterviews { get; set; }
        public int UpcomingInterviews { get; set; }
        public int PendingResults { get; set; }
        public List<Vacancy> RecentVacancies { get; set; }
        public List<Applicant> RecentApplicants { get; set; }
        public List<Interview> TodayInterviewList { get; set; }
    }
}