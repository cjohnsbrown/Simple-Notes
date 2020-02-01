using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleNotes.Api.Models;
using SimpleNotes.Api.Services;

namespace SimpleNotes.Api.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase {

        private INotesManager NotesManager { get; }

        public UserController(INotesManager notesManager) {
            NotesManager = notesManager;
        }

        [HttpGet]
        public async Task<UserDataResponse> Get() {
            return await NotesManager.GetUserDataAsync(HttpContext);
        }

    }
}