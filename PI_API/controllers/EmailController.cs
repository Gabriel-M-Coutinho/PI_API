
using Microsoft.AspNetCore.Mvc;

using PI_API.models;
using PI_API.services;

namespace PI_API.controllers;




[Route("api/[controller]")]
[ApiController]
public class EmailController: ControllerBase
{
    private readonly EmailService _emailService;

    public EmailController(EmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost]
    public ActionResult SendEmail([FromBody] SendEmailDTO email)
    {
        try
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendEmailAsync(email);
                }
                catch (Exception ex)
                {
                    // ⚠️ Não pode dar return aqui — apenas logue
                    Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
                    // ou use um logger, ex: _logger.LogError(ex, "Erro no envio de e-mail");
                }
            });

            return Ok("E-mail sendo processado em background");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    
    
    
}