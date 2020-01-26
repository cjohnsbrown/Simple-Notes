using SimpleNotes.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleNotes.Api.Data {
    public interface IDataRepository {

        Task<IEnumerable<Label>> GetUserLabelsAsync(string userId);
        Task<IEnumerable<Label>> GetNoteLabelsAsync(string noteId);
       Task<IEnumerable<Note>> GetUserNotesAsync(string userId);
    }
}
