using HRRecruitment.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRRecruitment.Models
{
    [Table("Employees")]
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(50)]
        public string EmployeeNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string EmployeeName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [ForeignKey("Department")]
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastLoginDate { get; set; }

        public string ProfilePicture { get; set; } = "/images/default-avatar.png";

        // Navigation Properties
        public virtual Department Department { get; set; }
        public virtual ICollection<Vacancy> CreatedVacancies { get; set; }
        public virtual ICollection<Interview> Interviews { get; set; }
    }
}