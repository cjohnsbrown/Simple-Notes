﻿using Microsoft.AspNetCore.Identity;

namespace SimpleNotes.Api.Data {
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser {

        public string SecretKey { get; set; }
    }
}
