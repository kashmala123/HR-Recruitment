using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRRecruitment.Models
{
    [Table("Applicants")]
    public class Applicant
    {
        [Key]
        public int ApplicantId { get; set; }

        [Required]
        [StringLength(50)]
        public string ApplicantNumber { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "First name can only contain letters and spaces")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Last name can only contain letters and spaces")]
        public string LastName { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string Phone { get; set; }

        public string Address { get; set; }

        public string ResumePath { get; set; }

        [StringLength(100)]
        public string HighestQualification { get; set; }

        public decimal? ExperienceYears { get; set; }

        [StringLength(200)]
        public string CurrentCompany { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Not in Process";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("CreatedByEmployee")]
        public int? CreatedBy { get; set; }

        // Added from user theme
        [Required]
        public string Password { get; set; }

        public string? ProfilePicture { get; set; }

        // Navigation Properties
        public virtual Employee CreatedByEmployee { get; set; }
        public virtual ICollection<ApplicantVacancy> ApplicantVacancies { get; set; }
    }
}