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
    public class NotesController : ControllerBase {


        private INotesManager Manager { get; }

        public NotesController(INotesManager manager) {
            Manager = manager;
        }

        // POST: api/Notes
        [HttpPost]
        public async Task<string> CreateNote([FromBody]Note note) {

            string id = await Manager.CreateNoteAsync(HttpContext, note);
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
            await Manager.UpdateNoteAsync(HttpContext, note);
            return NoContent();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id) {
            if (!await Manager.NoteBelongsToUserAsync(User, id)) {
                ModelState.AddModelError("Id", "User does not have a note that matches this id");
                return BadRequest(ModelState);
            }

            await Manager.DeleteNoteAsync(id);
            return NoContent();
        }

        [HttpPatch]
        [Route("{id}/pinned")]
        public async Task<IActionResult> Pinned(string id, [FromBody]bool pinned) {
            if (!await Manager.NoteBelongsToUserAsync(User, id)) {
                ModelState.AddModelError("Id", "User does not have a note that matches this id");
                return BadRequest(ModelState);
            }

            await Manager.UpdateNotePinnedAsync(id, pinned);
            return NoContent();
        }

        [HttpPost]
        [Route("{id}/Label")]
        public async Task<IActionResult> AddLabel(string id, [FromBody]NoteLabelRequest noteLabel) {
            if (!await Manager.NoteBelongsToUserAsync(User, id)) {
                ModelState.AddModelError("Id", "User does not have a note that matches this id");
                return BadRequest(ModelState);
            }

            if (!await Manager.LabelBelongsToUserAsync(User, noteLabel.LabelId)) {
                ModelState.AddModelError(nameof(noteLabel.LabelId), "User does not have a label that matches this id");
                return BadRequest(ModelState);
            }

            await Manager.AddLabelToNoteAsync(id, noteLabel.LabelId);
            return NoContent();
        }

        [HttpDelete]
        [Route("{id}/Label")]
        public async Task<IActionResult> RemoveLabel(string id, [FromBody]NoteLabelRequest noteLabel) {
            if (!await Manager.NoteBelongsToUserAsync(User, id)) {
                ModelState.AddModelError("Id", "User does not have a note that matches this id");
                return BadRequest(ModelState);
            }

            if (!await Manager.LabelBelongsToUserAsync(User, noteLabel.LabelId)) {
                ModelState.AddModelError(nameof(noteLabel.LabelId), "User does not have a label that matches this id");
                return BadRequest(ModelState);
            }

            await Manager.RemoveLabelFromNote(id, noteLabel.LabelId);
            return NoContent();
        }
    }
}
