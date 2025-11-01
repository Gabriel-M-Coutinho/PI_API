    using LeadSearch.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CodeAnalysis.Scripting;
    using PI_API.dto;
    using PI_API.models;
    using PI_API.services;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    using BCrypt.Net;
    using Microsoft.AspNetCore.Authorization;
    using System.Data;

    namespace PI_API.controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class UserController : ControllerBase
        {
            UserService _userService;
            UserManager<ApplicationUser> _userManager;
            EmailService _emailService;

            public UserController(UserService userService, UserManager<ApplicationUser> userManager,EmailService emailService)
            {
                _userService = userService;
                _userManager = userManager;
                _emailService = emailService;
            }
            
            
            [HttpPost]
            public async Task<ActionResult<ApplicationUser>> Create([FromBody] UserDTO userDto)
            {

                ApplicationUser existUser =  await _userService.GetByEmail(userDto.Email);
                if (existUser != null)
                {
                    return BadRequest("Email already exists");
                }
                
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
                newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

                newUser.CreatedAt = DateTime.Now;
                newUser.UpdatedAt = DateTime.Now;
                newUser.Active = false;

                await _userService.CreateAsync(newUser);
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
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
                });
                
                return Ok("criado com sucesso, faca login agora");
            }

            [HttpGet]
            [Authorize(Roles = nameof(ROLE.ADMIN))]
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
                return Ok(await _userService.GetByIdAsync(userId));
            }

            [HttpDelete("{id}")]
            [Authorize( Roles = nameof(ROLE.ADMIN))]
            public async Task<ActionResult> Delete(string id)
            {
                await _userService.DeleteAsync(id);

                return Ok();
            }

            [HttpPut("{id}")]
            public async Task<ActionResult> Update(string id, [FromBody] UserDTO userDto)
            {
                ApplicationUser updatedUser = await _userService.GetByIdAsync(id);

                if (updatedUser == null) return BadRequest("User not found");

                updatedUser.FullName = userDto.FullName;
                updatedUser.Email = userDto.Email;
                updatedUser.CpfCnpj = userDto.CpfCnpj;
                updatedUser.UpdatedAt = DateTime.Now;
                updatedUser.Active = true;
                
                await _userService.UpdateAsync(id, updatedUser);

                return Ok();
            }    
        }
    }
