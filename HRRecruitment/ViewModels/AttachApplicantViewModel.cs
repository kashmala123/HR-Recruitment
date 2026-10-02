using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRRecruitment.ViewModels
{
    public class AttachApplicantViewModel
    {
        public int ApplicantId { get; set; }
        public string ApplicantName { get; set; }
        public string ApplicantNumber { get; set; }

        [Required(ErrorMessage = "Please select a vacancy")]
        public int VacancyId { get; set; }

        public SelectList VacanciesList { get; set; }
    }
}