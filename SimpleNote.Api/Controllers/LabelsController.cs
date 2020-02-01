using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleNotes.Api.Models;
using SimpleNotes.Api.Services;
using SimpleNotes.Cryptography;

namespace SimpleNotes.Api.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class LabelsController : ControllerBase {

        private INotesManager Manager { get; }


        public LabelsController(INotesManager manager) {
            Manager = manager;
        }

        // POST: api/Labels
        [HttpPost]
        public async Task<IActionResult> CreateLabel([FromBody] Label label) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            string userKey = HttpContext.Session.GetString(Crypto.UserKey);
            if (await Manager.LabelNameExistsAsync(User, userKey, label.Name)) {
                ModelState.AddModelError(nameof(label.Name), $"A label named '{label.Name}' already exists");
                return BadRequest(ModelState);
            }

            var item = new ItemCreatedResponse();
            item.Id = await Manager.CreateLabelAsync(User, userKey, label);

            return Created(string.Empty, item);
        }

        // PUT: api/Labels/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateLabel(string id, [FromBody]Label label) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            if (! await Manager.LabelBelongdsToUserAsync(User, label.Id)) {
                ModelState.AddModelError(nameof(label.Id), "User does not have a label that matches the given id");
                return BadRequest(ModelState);
            }

            string userKey = HttpContext.Session.GetString(Crypto.UserKey);
            if (await Manager.LabelNameExistsAsync(User, userKey, label.Name)) {
                ModelState.AddModelError(nameof(label.Name), $"A label named '{label.Name}' already exists");
                return BadRequest(ModelState);
            }

            await Manager.UpdateLabelAsync(User, userKey, label);
            return NoContent();
        }

        // DELETE: api/Labels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id) {
            if (!await Manager.LabelBelongdsToUserAsync(User, id)) {
                ModelState.AddModelError("Id", "User does not have a label that matches the given id");
                return BadRequest(ModelState);
            }

            await Manager.DeleteLabelAsync(id);
            return NoContent();
        }
    }
}
