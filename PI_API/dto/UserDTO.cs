using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace PI_API.dto;

public class UserDTO
{
    [NotNull]
    [Required]
    public string? Name { get; set; } = string.Empty;

    [NotNull]
    [Required]
    [EmailAddress]
    public string? Email { get; set; } = string.Empty;
        
    [NotNull]
    [Required]
    public string? FullName { get; set; }
    [NotNull]
    public string? CpfCnpj { get; set; }
}