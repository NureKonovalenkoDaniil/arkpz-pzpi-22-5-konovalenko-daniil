using MedicationManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MedicationManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Register model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                return Conflict("User already exists!");

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Перевірка кількості користувачів у базі
            var usersCount = _userManager.Users.Count();
            if (usersCount == 1)
            {
                // Якщо це перший користувач, призначаємо роль Administrator
                await _userManager.AddToRoleAsync(user, "Administrator");
            }
            else
            {
                // Усі інші отримують роль User
                await _userManager.AddToRoleAsync(user, "User");
            }

            return Ok("User registered successfully!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (result.Succeeded)
                {
                    var token = GenerateJwtToken(user);
                    return Ok(new { Token = token });
                }
                return Unauthorized("Invalid login attempt");
            }
            return Unauthorized("Invalid login attempt");
        }
        [HttpPost("create-role")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CreateRole([FromBody] RoleDto roleDto)
        {
            if (string.IsNullOrWhiteSpace(roleDto.RoleName))
                return BadRequest("Role name is required.");

            var roleExisting = await _roleManager.RoleExistsAsync(roleDto.RoleName);
            if (roleExisting) return BadRequest($"Role name {roleDto.RoleName} already exists");

            var role = new IdentityRole { Name = roleDto.RoleName };
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                return Ok();
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }
        [HttpPost("assing-role")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AddUserToRole([FromBody] RoleDto roleDto)
        {
            var user = await _userManager.FindByEmailAsync(roleDto.Email);
            if (user == null)
            {
                return NotFound($"User with email: {roleDto.Email} not found");
            }
            var result = await _userManager.AddToRoleAsync(user, roleDto.RoleName);
            if (result.Succeeded)
            {
                return Ok();
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }
        private string GenerateJwtToken(IdentityUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var role = _userManager.GetRolesAsync(user).Result.FirstOrDefault();

            // Обновление SecurityTokenDescriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, role)
                }),
                //Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:DurationInMinutes"])),
                Expires = DateTime.UtcNow.AddMinutes(30),  // Токен истекает через 30 минут
                NotBefore = DateTime.UtcNow,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
                Issuer = _configuration["Jwt:Issuer"],   // Добавлено
                Audience = _configuration["Jwt:Audience"] // Добавлено
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public class RoleDto
        {
            public string Email { get; set; }
            public string RoleName { get; set; }
        }
    }
}
