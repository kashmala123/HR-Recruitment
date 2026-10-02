using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HRRecruitment.ViewModels
{
    public class CreateApplicantViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Only letters allowed")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Only letters allowed")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [RegularExpression(@"^[0-9+\-\s]+$")]
        public string Phone { get; set; }

        public string Address { get; set; }

        public string HighestQualification { get; set; }

        [Range(0, 50)]
        public decimal? ExperienceYears { get; set; }

        public string CurrentCompany { get; set; }

        public IFormFile Resume { get; set; }
    }
}