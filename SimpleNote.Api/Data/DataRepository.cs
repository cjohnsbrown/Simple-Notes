using Microsoft.Extensions.Configuration;
using SimpleNotes.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SQLite;
using Dapper;

namespace SimpleNotes.Api.Data {
    public class DataRepository : IDataRepository {

        private string ConnectionString { get; }

        public DataRepository(IConfiguration config) {
            ConnectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Label>> GetUserLabelsAsync(string userId) {
            using (var connection = new SQLiteConnection(ConnectionString)) {
                connection.Open();
                var param = new { Id = userId };
                return await connection.QueryAsync<Label>(@"
                        SELECT * FROM Labels WHERE Id IN
                        (SELECT labels.Id FROM UserLabels
                            JOIN AspNetUsers as users
                            ON UserLabels.UserId = users.Id
                            JOIN Labels 
                            ON UserLabels.LabelId = Labels.Id
                            WHERE users.Id = @Id)",
                            param);

            }
        }

        public async Task<IEnumerable<Note>> GetUserNotesAsync(string userId) {
            using (var connection = new SQLiteConnection(ConnectionString)) {
                connection.Open();
                var param = new { Id = userId };
                var notes  = await connection.QueryAsync<Note>(@"
                        SELECT * FROM Notes WHERE Id IN
                        (SELECT notes.Id FROM UserNotes
                            JOIN AspNetUsers as users
                            ON UserNotes.UserId = users.Id
                            JOIN Notes 
                            ON UserNotes.NoteId = notes.Id
                            WHERE users.Id = @Id)",
                            param);

                // Get labels added to each note
                foreach(Note note in notes) {
                    param = new { Id = note.Id };
                    note.LabelIds = await connection.QueryAsync<string>(@"
                    SELECT labels.Id FROM NoteLabels
                        JOIN Notes
                        On NoteLabels.NoteId = Notes.Id
                        JOIN Labels 
                        ON NoteLabels.LabelId = Labels.Id
                        WHERE Notes.Id = @Id",
                        param);
                }

                return notes;
            }
        }

    }
}
