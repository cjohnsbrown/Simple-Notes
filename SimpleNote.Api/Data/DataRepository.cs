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

        public IEnumerable<Label> GetUserLabels(string userId) {
            using (var connection = new SQLiteConnection(ConnectionString)) {
                connection.Open();
                var param = new { Id = userId };
                return connection.Query<Label>(@"
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

        public IEnumerable<Label> GetNoteLabels(string noteId) {
            using (var connection = new SQLiteConnection(ConnectionString)) {
                connection.Open();
                var param = new { Id = noteId };
                return connection.Query<Label>(@"
                        SELECT * FROM Labels WHERE Id IN
                        (SELECT labels.Id FROM NoteLabels
                            JOIN Notes
                            On NoteLabels.NoteId = Notes.Id
                            JOIN Labels 
                            ON NoteLabels.LabelId = Labels.Id
                            WHERE Notes.Id = @Id)",
                            param);

            }
        }

        public IEnumerable<Note> GetUserNotes(string userId) {
            using (var connection = new SQLiteConnection(ConnectionString)) {
                connection.Open();
                var param = new { Id = userId };
                return connection.Query<Note>(@"
                        SELECT * FROM Notes WHERE Id IN
                        (SELECT notes.Id FROM UserNotes
                            JOIN AspNetUsers as users
                            ON UserNotes.UserId = users.Id
                            JOIN Notes 
                            ON UserNotes.NoteId = notes.Id
                            WHERE users.Id = @Id)",
                            param);

            }
        }
    }
}
