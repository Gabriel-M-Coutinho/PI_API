using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace PI_API.dto
{
    public class LoginDTO
    {
        [Required]
        [NotNull]
        [EmailAddress]
        public string Username { get; set; }
        [Required]
        [NotNull]
        [MinLength(8)]
        public string Password { get; set; }

        public LoginDTO(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
