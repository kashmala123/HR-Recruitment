using System;
using System.ComponentModel.DataAnnotations;

namespace HRRecruitment.Models
{
    public class Subscriber
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public DateTime SubscribedDate { get; set; } = DateTime.UtcNow;

        // New properties for unsubscribe
        public string UnsubscribeToken { get; set; } = Guid.NewGuid().ToString();
        public bool IsActive { get; set; } = true;   // Active by default
    }
}