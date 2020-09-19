using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ASP_SampleAPI.Config;
using ASP_SampleAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ASP_SampleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> manager;
        private readonly JwtConfig config;

        public AuthController(UserManager<User> userManager, IConfiguration config)
        {
            manager = userManager;
            this.config = config.GetJwtConfig();
        }

        public class AuthenticationRequest
        {
            public string Login { get; set; }

            public string Password { get; set; }

            public bool AsRole { get; set; }
        }

        public class AuthenticationResponse
        {
            public string Token { get; set; }

            public DateTime ExpiresAt { get; set; }

            public string RefreshToken { get; set; }
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<AuthenticationResponse>> Authenticate([FromBody] AuthenticationRequest request)
        {
            var user = await GetUserByUsernameOrEmail(request.Login);

            if (user == null)
                return NotFound("User not found");

            if (await manager.CheckPasswordAsync(user, request.Password))
            {
                var (expiresAt, token) = GenerateToken(user);
                return new AuthenticationResponse
                {
                    Token = token,
                    RefreshToken = "",
                    ExpiresAt = expiresAt
                };
            }
            else
            {
                return NotFound("User not found");
            }
        }


        public class RegisterRequest
        {
            [Required]
            public string Username { get; set; }

            [Required]
            public string Email { get; set; }

            [Required]
            public string Password { get; set; }
        }

        public class RegisterResponse
        {
            public bool Success { get; set; } = true;
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
        {
            var user = await manager.FindByEmailAsync(request.Email);

            if (user != null)
                return Conflict("Email is taken");

            user = await manager.FindByNameAsync(request.Username);

            if (user != null)
                return Conflict("Username is taken");

            var result = await manager.CreateAsync(new Models.User
            {
                UserName = request.Username,
                Email = request.Email
            }, request.Password);

            if (result.Succeeded)
                return new RegisterResponse();

            var dictionary = new ModelStateDictionary();
            foreach (var err in result.Errors)
            {
                dictionary.AddModelError(err.Code, err.Description);
            }
            return new BadRequestObjectResult(dictionary);
        }

        private async Task<User> GetUserByUsernameOrEmail(string usernameOrEmail)
        {
            var user = await manager.FindByEmailAsync(usernameOrEmail);

            if (user != null)
                return user;

            user = await manager.FindByNameAsync(usernameOrEmail);

            return user;
        }

        private (DateTime, string) GenerateToken(User identityUser)
        {

            var now = DateTime.UtcNow;
            var expiresAt = now.AddDays(1);
            var creds = new SigningCredentials(
                config.GetSecurityKey(), 
                SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, identityUser.Id),
                new Claim(JwtRegisteredClaimNames.Email, identityUser.Email),
                new Claim(JwtRegisteredClaimNames.UniqueName, identityUser.UserName),
                new Claim("role", identityUser.Role ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: config.Issuer,
                notBefore: now,
                expires: expiresAt,
                claims: claims,
                signingCredentials: creds,
                audience: config.Audience
                );
            var tokenHandler = new JwtSecurityTokenHandler();
            return (expiresAt, tokenHandler.WriteToken(token));
        }
    }
}
