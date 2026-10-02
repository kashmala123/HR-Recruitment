using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRRecruitment.Models
{
    [Table("EmailNotifications")]
    public class EmailNotification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string RecipientEmail { get; set; }

        [Required]
        [StringLength(20)]
        public string RecipientType { get; set; }

        [Required]
        [StringLength(500)]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime? SentDate { get; set; }

        public string ErrorMessage { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}