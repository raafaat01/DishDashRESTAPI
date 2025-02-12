using DishDashRESTAPIDatabase.DTOs;
using DishDashRESTAPIDatabase.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DishDashRESTAPIDatabase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly DishDashDb _db;
        private readonly IConfiguration _config;

        public LoginController(DishDashDb db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // POST: api/login/register (Registers a regular user)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO userRegister)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the email is already registered (for any role)
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == userRegister.Email);
            if (existingUser != null)
            {
                return BadRequest("Email is already registered.");
            }

            var user = new User
            {
                Username = userRegister.Username,
                Email = userRegister.Email,
                Password = userRegister.Password, // In production, hash the password!
                Role = "User",
                IsVerified = true // For normal users, verification is immediate.
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok("User registered successfully.");
        }

        // POST: api/login/register/admin (Registers an admin user)
        [HttpPost("register/admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] UserRegisterDTO userRegister)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the email is already registered (for any role)
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == userRegister.Email);
            if (existingUser != null)
            {
                return BadRequest("Email is already registered.");
            }

            var user = new User
            {
                Username = userRegister.Username,
                Email = userRegister.Email,
                Password = userRegister.Password, // In production, hash the password!
                Role = "Admin",
                IsVerified = false  // New admin accounts require approval.
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok("Admin registered successfully. Awaiting verification.");
        }

        // POST: api/login/login (Logs in a user and returns a JWT token)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO userLogin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Retrieve the user by username.
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == userLogin.Username);
            if (user == null || user.Password != userLogin.Password)
            {
                return Unauthorized("Invalid credentials.");
            }

            // If the user is an admin and is not verified, deny login.
            if (user.Role == "Admin" && !user.IsVerified)
            {
                return Unauthorized("Admin account not verified. Please contact an administrator.");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token, username = user.Username, role = user.Role });
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // GET: api/login/admin (Example endpoint protected for admin users)
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult VerifyAdmin()
        {
            return Ok("Admin verified");
        }
    }
}
