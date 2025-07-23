using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PerfumeAPI.Models.DTOs;
using PerfumeAPI.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PerfumeAPI.Controllers.Api
{
    [Route("api/auth")]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthApiController> _logger;

        public AuthApiController(
            UserManager<User> userManager,
            IConfiguration config,
            ILogger<AuthApiController> logger)
        {
            _userManager = userManager;
            _config = config;
            _logger = logger;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] AuthRegisterDto dto)
        {
            try
            {
                var user = new User
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    ShippingAddress = dto.ShippingAddress
                };

                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Registration failed for {Email}", dto.Email);
                    return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
                }

                await _userManager.AddToRoleAsync(user, "Customer");
                _logger.LogInformation("User {Email} registered successfully", dto.Email);

                return Ok(new { Message = "Registration successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { Message = "An error occurred during registration" });
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] AuthLoginDto dto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                {
                    _logger.LogWarning("Login failed for {Email}", dto.Email);
                    return Unauthorized(new { Message = "Invalid email or password" });
                }

                var token = await GenerateJwtToken(user);
                _logger.LogInformation("User {Email} logged in successfully", dto.Email);

                return Ok(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { Message = "An error occurred during login" });
            }
        }

        private async Task<AuthTokenResponse> GenerateJwtToken(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Name, user.UserName ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_config["Jwt:ExpireDays"] ?? "30"));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new AuthTokenResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expires = expires
            };
        }
    }

    public class AuthTokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
    }
}