using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HRRecruitment.Models
{
        public class UserProfileVM
        {
            [Required(ErrorMessage = "Full Name is required.")]
            [Display(Name = "Full Name")]
            [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
            [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain letters and spaces")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Invalid Email Address.")]
            [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
            public string Email { get; set; }

            public string? ProfilePicture { get; set; }

            [Display(Name = "Profile Picture")]
            public IFormFile? ProfileImage { get; set; }
        }
    

    public class ChangePasswordVM
    {
        [Required(ErrorMessage = "Current Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

    }
}
