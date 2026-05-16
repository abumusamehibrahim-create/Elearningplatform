using System.ComponentModel.DataAnnotations;

namespace ELearningPlatform.ViewModels
{
    public class ResetPasswordViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

        [Required, DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required, DataType(DataType.Password), Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; }
    }
}
