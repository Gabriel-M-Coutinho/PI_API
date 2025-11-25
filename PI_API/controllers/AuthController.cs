using LeadSearch.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PI_API.dto;
using PI_API.services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PI_API.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager, UserService userService, IConfiguration configuration)
        {
            _userManager = userManager;
            _userService = userService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDTO loginDto)
        {
            
            var user = await _userService.GetByEmail(loginDto.Username);

            if (user == null) return Unauthorized("Usuário não encontrado.");

            var roles = await _userService.GetRolesAsync(user);

            bool validPassword = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!validPassword) return Unauthorized("Senha incorreta.");

            // Cria uma lista de Claims, inicialmente apenas com uma claim com o ID do usuário
            var claims = new List<Claim>
            {
                new Claim("userid", user.Id.ToString()),
            };

            // ForEach das roles para colocar na claim
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Gera o token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(4),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = jwt,
                expiration = token.ValidTo
            });
        }
        [Authorize]
        [HttpPost("changePassword")]
        public async Task<ActionResult> ChancePassword([FromBody] ChangePasswordDTO dto)
        {
            if (dto.newPassword != dto.confirmPassword)
            {
                return BadRequest("Nova Senha tem que ser confirmada");
            }
            var user = await _userService.GetByIdAsync(User.FindFirst("userid")?.Value);
            if (user == null)
                return Unauthorized();

            // Tenta trocar a senha
            var result = await _userService.ChangePasswordAsync(
                user,
                dto.currentPassword,
                dto.newPassword
            );

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Senha alterada com sucesso!");
        }
    }
}
