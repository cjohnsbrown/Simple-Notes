using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleNotes.Api.Models;
using SimpleNotes.Api.Services;
using SimpleNotes.Cryptography;

namespace SimpleNotes.Api.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotesController : ControllerBase {


        private INotesManager Manager { get; }

        public NotesController(INotesManager manager) {
            Manager = manager;
        }

        // POST: api/Notes
        [HttpPost]
        public async Task<string> CreateNote([FromBody]Note note) {

            string userKey = HttpContext.Session.GetString(Crypto.UserKey);
            string id = await Manager.CreateNoteAsync(User, userKey, note);
            Response.StatusCode = 201;
            return id;
        }

        // PUT: api/Notes/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(string id, [FromBody] Note note) {
            if (! await Manager.NoteBelongsToUserAsync(User, id)) {
                ModelState.AddModelError(nameof(note.Id), "User does not have a note that matches this id");
                return BadRequest(ModelState);
            }

            note.Id = id;
            string userKey = HttpContext.Session.GetString(Crypto.UserKey);
            await Manager.UpdateNoteAsync(User, userKey, note);
            return Ok();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id) {
            if (!await Manager.NoteBelongsToUserAsync(User, id)) {
                ModelState.AddModelError("Id", "User does not have a note that matches this id");
                return BadRequest(ModelState);
            }

            await Manager.DeleteNoteAsync(id);
            return Ok();
        }

        [HttpPatch]
        [Route("{id}/pinned")]
        public async Task<IActionResult> Pinned(string id, [FromBody]bool pinned) {
            if (!await Manager.NoteBelongsToUserAsync(User, id)) {
                ModelState.AddModelError("Id", "User does not have a note that matches this id");
                return BadRequest(ModelState);
            }

            await Manager.UpdateNotePinnedAsync(id, pinned);
            return Ok();
        }

        [HttpPost]
        [Route("{id}/Label")]
        public void AddLabel(string id, [FromBody]string labelId) {
        }

        [HttpDelete]
        [Route("{id}/Label")]
        public void RemoveLabel(string id, [FromBody]string labelId) { }
    }
}
