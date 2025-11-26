using BCrypt.Net;
using LeadSearch.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using PI_API.dto;
using PI_API.models;
using PI_API.services;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PI_API.controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    UserManager<ApplicationUser> _userManager;
    UserService _userService;
    EmailService _emailService;

    public UserController(UserManager<ApplicationUser> userManager, UserService userService, EmailService emailService)
    {
        _userManager = userManager;
        _userService = userService;
        _emailService = emailService;
    }
        
    [HttpPost]
    public async Task<ActionResult<ApplicationUser>> Create([FromBody] UserDTO userDto)
    {
        ApplicationUser? existUser = await _userService.GetByEmail(userDto.Email);
        if (existUser != null) return BadRequest("Email already exists!");
            
        ApplicationUser newUser = new ApplicationUser();

        string userName = userDto.FullName.Replace(" ", "");
        var normalizedString = userName.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new StringBuilder();
        foreach (char character in normalizedString)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(character);
            }
        }

        userName = sb.ToString().Normalize(NormalizationForm.FormC);
        userName = Regex.Replace(userName, @"[^a-zA-Z0-9\s]", "");

        newUser.UserName = userName;
        newUser.Email = userDto.Email;
        newUser.CpfCnpj = userDto.CpfCnpj;
        newUser.FullName = userDto.FullName;
        newUser.CreatedAt = DateTime.Now;
        newUser.UpdatedAt = DateTime.Now;
        newUser.Active = true;

        var result = await _userManager.CreateAsync(newUser, userDto.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(await _userService.GetByEmail(userDto.Email), nameof(ROLE.STANDARD));
            return Ok("criado com sucesso, faca login agora");
        }
        return BadRequest(result.Errors);
        /*var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
        var confirmationLink = $"https://localhost:3000/confirm-email?userId={newUser.Id.ToString()}&token={Uri.EscapeDataString(token)}";

        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendEmailAsync(
                new SendEmailDTO(newUser.Email, confirmationLink, "Confirme seu e-mail")
                );
            }
            catch (Exception ex)
            {
                // opcional: logar o erro para acompanhamento
                Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
            }
        });*/
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<List<ApplicationUser>>> GetAll()
    {
        List<ApplicationUser> users = await _userService.GetAsync();

        return Ok(users);
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<ApplicationUser>> Get()
    {
        var userId = User.FindFirst("userid")?.Value;

        if (userId == null) return BadRequest();

        return Ok(await _userService.GetByIdAsync(userId));
    }

    [HttpDelete("{password}")]
    [Authorize]
    public async Task<ActionResult> Delete(string password)
    {
        var deletedUser = await _userService.GetByIdAsync(User.FindFirst("userid")?.Value);
        bool validPassword = await _userManager.CheckPasswordAsync(deletedUser, password);
        if (!validPassword) return Unauthorized("Senha incorreta.");
        await _userService.DeleteAsync(User.FindFirst("userid")?.Value);

        return Ok();
    }
    /*[HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult> Delete(string id)
    {
        await _userService.DeleteAsync(id);

        return Ok();
    }*/
    
    [HttpPut]
    [Authorize]
    public async Task<ActionResult> Update([FromBody] UserDTO userDto)
    {
        ApplicationUser? updatedUser = await _userService.GetByIdAsync(User.FindFirst("userid")?.Value);
        bool validPassword = await _userManager.CheckPasswordAsync(updatedUser, userDto.Password);
        if (!validPassword) return Unauthorized("Senha incorreta.");

        if (updatedUser == null) return BadRequest("User not found");

        updatedUser.FullName = userDto.FullName;
        updatedUser.Email = userDto.Email;
        updatedUser.CpfCnpj = userDto.CpfCnpj;
        updatedUser.UpdatedAt = DateTime.Now;
        updatedUser.Active = true;

        await _userService.UpdateAsync(updatedUser);

        return Ok();
    }
    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, [FromBody] UserDTO userDto)
    {
        ApplicationUser? updatedUser = await _userService.GetByIdAsync(id);

        if (updatedUser == null) return BadRequest("User not found");

        updatedUser.FullName = userDto.FullName;
        updatedUser.Email = userDto.Email;
        updatedUser.CpfCnpj = userDto.CpfCnpj;
        //updatedUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password); vai mudar senha?
        updatedUser.UpdatedAt = DateTime.Now;
        updatedUser.Active = true;
            
        await _userService.UpdateAsync(updatedUser);

        return Ok();
    }    
}