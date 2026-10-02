using System.ComponentModel.DataAnnotations;

namespace HRRecruitment.ViewModels
{
    public class CreateVacancyViewModel
    {
        [Required(ErrorMessage = "Job title is required")]
        [StringLength(200)]
        public string JobTitle { get; set; }

        [Required(ErrorMessage = "Job description is required")]
        public string JobDescription { get; set; }

        public string RequiredQualifications { get; set; }

        [Required(ErrorMessage = "Total openings is required")]
        [Range(1, 100)]
        public int TotalOpenings { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ClosingDate { get; set; }
    }
}