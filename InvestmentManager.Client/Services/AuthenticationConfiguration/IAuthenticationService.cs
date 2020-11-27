﻿using InvestmentManager.Models;
using InvestmentManager.Models.Security;
using System.Threading.Tasks;

namespace InvestmentManager.Client.Services.AuthenticationConfiguration
{
    public interface IAuthenticationService
    {
        Task<BaseResult> RegisterAsync(RegisterModel model);
        Task<LoginResult> LoginAsync(LoginModel model);
        Task LogoutAsync();
    }
}