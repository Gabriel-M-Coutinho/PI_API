using System.ComponentModel.DataAnnotations;


namespace PI_API.models;

public record SendEmailDTO(
        [EmailAddress]
        [Required]
        string Emailto,
        [Required]
        string message,
        [Required]
        string subject
    
    );