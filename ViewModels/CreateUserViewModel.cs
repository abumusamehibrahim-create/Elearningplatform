using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ELearningPlatform.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public string FullName { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password), Compare("Password")]
        public string ConfirmPassword { get; set; }

        public string SelectedRole { get; set; }

        public List<string> AvailableRoles { get; set; } = new();
    }
}
