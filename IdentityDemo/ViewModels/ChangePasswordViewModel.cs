using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace IdentityDemo.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Old Password is required.")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "New Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+={}\[\]|;:'""<>\/?]).+$",ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public string NewPassword { get; set; }


        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
