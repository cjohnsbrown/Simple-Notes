using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleNotes.Api.Models;
using SimpleNotes.Api.Services;

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
            if (label == null) {
                return NotFound();
            }

            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            if (await Manager.LabelNameExistsAsync(HttpContext, label.Name)) {
                ModelState.AddModelError(nameof(label.Name), $"A label named '{label.Name}' already exists");
                return BadRequest(ModelState);
            }

            var item = new ItemCreatedResponse();
            item.Id = await Manager.CreateLabelAsync(HttpContext, label);

            return Created(string.Empty, item);
        }

        // PUT: api/Labels/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateLabel(string id, [FromBody]Label label) {
            if (string.IsNullOrEmpty(id) || label == null) {
                return NotFound();
            }

            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            if (! await Manager.LabelBelongsToUserAsync(User, label.Id)) {
                ModelState.AddModelError(nameof(label.Id), "User does not have a label that matches the given id");
                return BadRequest(ModelState);
            }

            if (await Manager.LabelNameExistsAsync(HttpContext, label.Name)) {
                ModelState.AddModelError(nameof(label.Name), $"A label named '{label.Name}' already exists");
                return BadRequest(ModelState);
            }

            await Manager.UpdateLabelAsync(HttpContext, label);
            return NoContent();
        }

        // DELETE: api/Labels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id) {
            if (string.IsNullOrEmpty(id)) {
                return NotFound();
            }

            if (!await Manager.LabelBelongsToUserAsync(User, id)) {
                ModelState.AddModelError("Id", "User does not have a label that matches the given id");
                return BadRequest(ModelState);
            }

            await Manager.DeleteLabelAsync(id);
            return NoContent();
        }
    }
}
