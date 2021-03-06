﻿using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Models.Security
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Enter email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Incorrect email")]
        [StringLength(40, ErrorMessage = "Email must be from {2} to {1} characters", MinimumLength = 6)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Enter password")]
        [DataType(DataType.Password, ErrorMessage = "Password too simple")]
        [StringLength(30, ErrorMessage = "Password must be from {2} to {1} characters", MinimumLength = 10)]
        public string Password { get; set; }
    }
}
