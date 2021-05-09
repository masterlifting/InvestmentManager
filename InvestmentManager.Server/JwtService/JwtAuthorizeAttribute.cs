using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace InvestmentManager.Server.JwtService
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class JwtAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = (IdentityUser)context.HttpContext.Items["User"];
          
            if (user is null)
                context.Result = new JsonResult(new { message = "unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
        }
    }
}
