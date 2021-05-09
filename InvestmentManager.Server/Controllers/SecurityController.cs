using InvestmentManager.Models.Security;
using InvestmentManager.Server.JwtService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<AuthResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return new() { IsSuccess = false, Info = string.Join(";", ModelState.Values.SelectMany(x => x.Errors)) };

            var result = await signInManager.PasswordSignInAsync(model.Email.Split('@')[0], model.Password, false, false);

            if (!result.Succeeded)
                return new() { IsSuccess = false, Info = "email or password are invalid;" };

            var currentUser = await userManager.FindByEmailAsync(model.Email);
            var roles = await userManager.GetRolesAsync(currentUser);

            var (token, expiry) = new JwtHelper(configuration).GetTokenData(currentUser.UserName, roles);

            return new() { IsSuccess = true, Token = token, Expiry = expiry };
        }
        [HttpPost("register/")]
        public async Task<AuthResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
                return new() { IsSuccess = false, Info = string.Join(";", ModelState.Values.SelectMany(x => x.Errors)) };

            var newUser = new IdentityUser { Email = model.Email, UserName = model.Email.Split('@')[0] };
            var result = await userManager.CreateAsync(newUser, model.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(x => x.Description);
                return new() { IsSuccess = false, Info = string.Join(";", errors) };
            }

            var (token, expiry) = new JwtHelper(configuration).GetTokenData(newUser.UserName);

            return new() { IsSuccess = true, Token = token, Expiry = expiry };
        }
    }
}
