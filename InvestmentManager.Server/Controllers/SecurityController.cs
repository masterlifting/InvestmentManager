using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using InvestmentManager.Models;
using InvestmentManager.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class SecurityController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly UserManager<IdentityUser> userManager;

        public SecurityController(
            IConfiguration configuration
            , SignInManager<IdentityUser> signInManager
            , UserManager<IdentityUser> userManager)
        {
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        [HttpPost("login/")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var result = await signInManager.PasswordSignInAsync(model.Email.Split('@')[0], model.Password, false, false);
            if (!result.Succeeded)
                return BadRequest(new LoginResult { IsSuccess = false, Info = "Username or password are invalid." });

            var currentUser = await userManager.FindByEmailAsync(model.Email);
            var roles = await userManager.GetRolesAsync(currentUser);

            var claims = new List<Claim> { new Claim(ClaimTypes.Name, model.Email) };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSecurityKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.Now.AddDays(Convert.ToInt32(configuration["JwtExpiryInDays"]));

            var token = new JwtSecurityToken(configuration["JwtIssuer"], configuration["JwtAudience"], claims, expires: expiry, signingCredentials: creds);

            return Ok(new LoginResult { IsSuccess = true, Token = new JwtSecurityTokenHandler().WriteToken(token), Expiry = expiry });
        }
        [HttpPost("register/")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var newUser = new IdentityUser { Email = model.Email, UserName = model.Email.Split('@')[0] };
            var result = await userManager.CreateAsync(newUser, model.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(x => x.Description);
                return BadRequest(new BaseActionResult { IsSuccess = false, Info = string.Join(";", errors) });
            }
            return Ok(new BaseActionResult { IsSuccess = true });
        }
    }
}
