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
    [Authorize]
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
                var applicationUser = await UserManager.FindByNameAsync(model.Username);
                string derivedKey = Crypto.DeriveKey(model.Password);
                HttpContext.Session.SetString(Crypto.UserKey, derivedKey);
                return Ok();
            }

            
            ModelState.AddModelError(string.Empty, "Username or password is incorrect.");
            return BadRequest(ModelState);
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
            if (await UserManager.FindByNameAsync(model.Username) != null) {
                ModelState.AddModelError(nameof(model.Username), $"A user named '{model.Username}' already exists");
                return BadRequest(ModelState);
            }

            ApplicationUser applicationUser = new ApplicationUser { UserName = model.Username };
            var result = await UserManager.CreateAsync(applicationUser, model.NewPassword);
            if (!result.Succeeded) {
                foreach (var error in result.Errors) {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
            
            // Get user key from password
            applicationUser = await UserManager.FindByNameAsync(model.Username);
            string derivedKey = Crypto.DeriveKey(model.NewPassword);

            // Create and encrypt secret key
            string secretKey = Crypto.RandomString();
            applicationUser.SecretKey = Crypto.Encrypt(derivedKey, secretKey);
            await UserManager.UpdateAsync(applicationUser);

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
                return BadRequest(ModelState);
            }

            // Get current derived key and decrypt the secret key
            string derivedKey = HttpContext.Session.GetString(Crypto.UserKey);
            string secretKey = Crypto.Decrypt(derivedKey, applicationUser.SecretKey);

            // Create new derived key to encrypt the secret key with
            applicationUser = await UserManager.GetUserAsync(User);
            derivedKey = Crypto.DeriveKey(model.NewPassword);
            applicationUser.SecretKey = Crypto.Encrypt(derivedKey, secretKey);

            // Update stored keys
            await UserManager.UpdateAsync(applicationUser);
            HttpContext.Session.SetString(Crypto.UserKey, derivedKey);
            return Ok();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("IsAuthenticated")]
        public bool IsAuthenticated() {
            return User.Identity.IsAuthenticated;
        }
    }
}

