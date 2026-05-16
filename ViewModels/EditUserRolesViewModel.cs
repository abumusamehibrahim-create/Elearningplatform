using System.Collections.Generic;

namespace ELearningPlatform.ViewModels
{
    public class EditUserRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<RoleSelection> Roles { get; set; } = new();
    }

    public class RoleSelection
    {
        public string RoleName { get; set; }
        public bool Selected { get; set; }
    }
}
