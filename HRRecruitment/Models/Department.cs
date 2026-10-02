using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRRecruitment.Models
{
    [Table("Departments")]
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string DepartmentName { get; set; }

        [Required]
        [StringLength(20)]
        public string DepartmentCode { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual ICollection<Employee> Employees { get; set; }
        public virtual ICollection<Vacancy> Vacancies { get; set; }
    }
}