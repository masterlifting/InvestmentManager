using InvestManager.ViewModels.AuthenticationModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace InvestManager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        public AccountsController(UserManager<IdentityUser> userManager) => this.userManager = userManager;
        
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RegisterModel model)
        {
            var newUser = new IdentityUser { Email = model.Email, UserName = model.Email.Split('@')[0] };
            var result = await userManager.CreateAsync(newUser, model.Password).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(x => x.Description);
                return Ok(new RegisterResult { Successful = false, Errors = errors });
            }
            return Ok(new RegisterResult { Successful = true });
        }
    }
}
