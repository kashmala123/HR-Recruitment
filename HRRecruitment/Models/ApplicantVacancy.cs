using HRRecruitment.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRRecruitment.Models
{
    [Table("ApplicantVacancies")]
    public class ApplicantVacancy
    {
        [Key]
        public int ApplicantVacancyId { get; set; }

        [ForeignKey("Applicant")]
        public int ApplicantId { get; set; }

        [ForeignKey("Vacancy")]
        public int VacancyId { get; set; }

        public DateTime AttachedDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(30)]
        public string CurrentStatus { get; set; } = "Applied";

        [ForeignKey("AttachedByEmployee")]
        public int? AttachedBy { get; set; }

        // Navigation Properties
        public virtual Applicant Applicant { get; set; }
        public virtual Vacancy Vacancy { get; set; }
        public virtual Employee AttachedByEmployee { get; set; }
        public virtual ICollection<Interview> Interviews { get; set; }
    }
}