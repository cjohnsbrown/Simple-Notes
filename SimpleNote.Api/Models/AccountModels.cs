using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleNotes.Api.Models {
    public class LoginModel {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class RegisterModel {
        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Password must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public string NewPassword { get; set; }
    }

    public class ChangePasswordModel {
        [Required]
        [StringLength(100, ErrorMessage = "Password must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public string NewPassword { get; set; }

        [Required]
        public string CurrentPassword { get; set; }
    }
}
