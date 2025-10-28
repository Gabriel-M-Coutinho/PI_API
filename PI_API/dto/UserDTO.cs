using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace PI_API.dto;

public class UserDTO
{
    [NotNull]
    [Required(ErrorMessage = "Por favor, digite o seu nome completo!"), Display(Name = "Nome completo")]
    public string? FullName { get; set; } = string.Empty;

    [NotNull]
    [Required(ErrorMessage = "Por favor, digite um e-mail!"), Display(Name = "E-mail")]
    [EmailAddress]
    public string? Email { get; set; } = string.Empty;

    [NotNull]
    [Required(ErrorMessage = "Por favor, digite um documento!"), Display(Name = "CPF ou CNPJ")]
    public string? CpfCnpj { get; set; }

    [NotNull]
    [MinLength(8)]
    [Required(ErrorMessage = "Por favor, digite uma senha!"), Display(Name = "Senha")]
    public string? Password { get; set; }
}