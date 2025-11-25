using System.ComponentModel.DataAnnotations;

namespace PI_API.dto
{
    public class ChangePasswordDTO
    {
        public string currentPassword { get; set; }
        public string newPassword { get; set; }
        [Compare("newPassword", ErrorMessage = "A confirmação não confere com a nova senha.")]
        public string confirmPassword { get; set; }
    }
}
