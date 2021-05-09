using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InvestmentManager.Server.JwtService
{
    public class JwtHelper
    {
        private readonly IConfiguration configuration;
        public JwtHelper(IConfiguration configuration) => this.configuration = configuration;
        
        internal (string token, DateTime expiry) GetTokenData(string userName, IList<string> roles = null)
        {
            var claims = new List<Claim> { new(ClaimTypes.Name, userName) };

            if (roles is not null)
                foreach (var role in roles)
                    claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSecurityKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.Now.AddDays(Convert.ToInt32(configuration["JwtExpiryInDays"]));

            var token = new JwtSecurityToken(configuration["JwtIssuer"], configuration["JwtAudience"], claims, expires: expiry, signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
        }
    }
}
