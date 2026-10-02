using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRRecruitment.Models
{
    [Table("Vacancies")]
    public class Vacancy
    {
        [Key]
        public int VacancyId { get; set; }

        [Required]
        [StringLength(50)]
        public string VacancyNumber { get; set; }

        [Required]
        [StringLength(200)]
        public string JobTitle { get; set; }

        public string JobDescription { get; set; }

        public string RequiredQualifications { get; set; }

        [Required]
        public int TotalOpenings { get; set; }

        public int RemainingOpenings { get; set; }

        [ForeignKey("Department")]
        public int DepartmentId { get; set; }

        [ForeignKey("CreatedByEmployee")]
        public int CreatedBy { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Open";

        public DateTime CreatedDate { get; set; } = DateTime.Today;

        public DateTime? ClosingDate { get; set; }

        public string ClosingReason { get; set; }

        public bool IsAutoClosed { get; set; } = false;

        // Navigation Properties
        public virtual Department Department { get; set; }
        public virtual Employee CreatedByEmployee { get; set; }
        public virtual ICollection<ApplicantVacancy> ApplicantVacancies { get; set; }
    }
}