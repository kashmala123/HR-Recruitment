using System.ComponentModel.DataAnnotations;

namespace HRRecruitment.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmNewPassword { get; set; }
    }
}