using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using InvestmentManager.ViewModels.AuthenticationModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace InvestmentManager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly UserManager<IdentityUser> userManager;

        public LoginController(
            IConfiguration configuration
            , SignInManager<IdentityUser> signInManager
            , UserManager<IdentityUser> userManager)
        {
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LoginModel model)
        {
            var result = await signInManager.PasswordSignInAsync(model.Email.Split('@')[0], model.Password, false, false).ConfigureAwait(false);

            if (!result.Succeeded)
                return BadRequest(new LoginResult { Successful = false, Error = "Username or password are invalid." });

            var currentUser = await userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);
            var roles = await userManager.GetRolesAsync(currentUser).ConfigureAwait(false);

            var claims = new List<Claim> { new Claim(ClaimTypes.Name, model.Email) };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSecurityKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.Now.AddDays(Convert.ToInt32(configuration["JwtExpiryInDays"]));

            var token = new JwtSecurityToken(configuration["JwtIssuer"], configuration["JwtAudience"], claims, expires: expiry, signingCredentials: creds);

            return Ok(new LoginResult { Successful = true, Token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}
