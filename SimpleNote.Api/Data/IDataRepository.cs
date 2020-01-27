using SimpleNotes.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleNotes.Api.Data {
    public interface IDataRepository {

        Task<IEnumerable<Label>> GetUserLabelsAsync(string userId);
        Task<IEnumerable<Note>> GetUserNotesAsync(string userId);
        Task<string> CreateNoteAsync(string userId, Note note);
        Task<string> CreateLabelAsync(string userId, Label label);
        Task AddLabelToNoteAsync(string noteId, string labelId);
        Task<bool> NoteExistsAsync(string id);
        Task<bool> LabelExistsAsync(string id);
        Task UpdateNoteAsync(Note note);
    }
}
