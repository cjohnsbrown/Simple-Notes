using SimpleNotes.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleNotes.Api.Data {
    public interface IDataRepository {

        IEnumerable<Label> GetUserLabels(string userId);
        IEnumerable<Label> GetNoteLabels(string noteId);
        IEnumerable<Note> GetUserNotes(string userId);
    }
}
