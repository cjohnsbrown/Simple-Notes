using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimpleNotes.Api.Data;
using SimpleNotes.Api.Models;
using SimpleNotes.Cryptography;

namespace SimpleNotes.Api.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase {

        private SignInManager<ApplicationUser> SignInManager { get; }
        private UserManager<ApplicationUser> UserManager { get; }

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager) {
            SignInManager = signInManager;
            UserManager = userManager;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Login")]
        public async Task<ActionResult> Login(LoginModel model) {
            Microsoft.AspNetCore.Identity.SignInResult result =
                await SignInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded) {
                string derivedKey = Crypto.DeriveKey(model.Password);
                HttpContext.Session.SetString(Crypto.UserKey, derivedKey);
                return Ok();
            }

            
            ModelState.AddModelError(string.Empty, "Username or password is incorrect.");
            return BadRequest();
        }

        [HttpPost]
        [Route("Logout")]
        public async Task<ActionResult> Logout() {
            await SignInManager.SignOutAsync();
            return Ok();
        }

        [HttpPut]
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IActionResult> Register(RegisterModel model) {
            if (!ModelState.IsValid) {
                return BadRequest();
            }

            if (await UserManager.FindByNameAsync(model.Username) != null) {
                ModelState.AddModelError(model.Username, $"A user named '{model.Username}' already exists");
                return BadRequest();
            }

            string derivedKey = Crypto.DeriveKey(model.NewPassword);
            string secretKey = Crypto.RandomString();
            ApplicationUser applicationUser = new ApplicationUser {
                UserName = model.Username,
                SecretKey = Crypto.Encrypt(derivedKey, secretKey)
            };

            var result = await UserManager.CreateAsync(applicationUser, model.NewPassword);
            if (!result.Succeeded) {
                foreach (var error in result.Errors) {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest();
            }

            HttpContext.Session.SetString(Crypto.UserKey, derivedKey);
            return Ok();
        }

        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model) {
            var applicationUser = await  UserManager.GetUserAsync(User);
            var result = await UserManager.ChangePasswordAsync(applicationUser, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded) {
                foreach (var error in result.Errors) {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest();
            }

            // Get current derived key and decrypt the secret key
            string derivedKey = Crypto.DeriveKey(model.CurrentPassword);
            string secretKey = Crypto.Decrypt(derivedKey, applicationUser.SecretKey);

            // Create new derived key to encrypt the secret key with
            derivedKey = Crypto.DeriveKey(model.NewPassword);
            applicationUser.SecretKey = Crypto.Encrypt(derivedKey, secretKey);
            HttpContext.Session.SetString(Crypto.UserKey, derivedKey);

            return Ok();
        }
    }
}

