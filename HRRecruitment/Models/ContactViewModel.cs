using System.ComponentModel.DataAnnotations;

namespace HRRecruitment.Models
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain letters and spaces")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; }
    }
}