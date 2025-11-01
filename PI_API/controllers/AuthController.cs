using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PI_API.dto;
using PI_API.services;

namespace PI_API.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;

        // ✅ Precisa ser público para o ASP.NET conseguir injetar o serviço
        public AuthController(UserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDTO loginDto)
        {
            // Procura o usuário pelo email/username
            var user = await _userService.GetByEmail(loginDto.Username);

            if (user == null)
                return Unauthorized("Usuário não encontrado.");

            if (!user.EmailConfirmed)
            {
                return Unauthorized("Email não validado");
            }
            
            bool validPassword = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
            if (!validPassword)
                return Unauthorized("Senha incorreta.");

            // 🔐 Cria somente a claim com o ID do usuário
            var claims = new[]
            {
                new Claim("userid", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            // 🔑 Gera o token
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
    }
}
