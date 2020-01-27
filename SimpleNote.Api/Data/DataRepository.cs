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
                var notes = await connection.QueryAsync<Note>(@"
                        SELECT * FROM Notes WHERE Id IN
                        (SELECT notes.Id FROM UserNotes
                            JOIN AspNetUsers as users
                            ON UserNotes.UserId = users.Id
                            JOIN Notes 
                            ON UserNotes.NoteId = notes.Id
                            WHERE users.Id = @Id)",
                            param);

                // Get labels added to each note
                foreach (Note note in notes) {
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

        public async Task<string> CreateNoteAsync(string userId, Note note) {
            note.Id = Guid.NewGuid().ToString();
            using (var connection = new SQLiteConnection(ConnectionString)) {
                connection.Open();
                var transaction = await connection.BeginTransactionAsync();

                SQLiteCommand command = new SQLiteCommand("INSERT INTO Notes VALUES (?,?,?,?,?)", connection);
                command.Parameters.Add(note.Id);
                command.Parameters.Add(note.Title);
                command.Parameters.Add(note.Content);
                command.Parameters.Add(note.Pinned);
                command.Parameters.Add(note.ModifiedDate);
                await command.ExecuteNonQueryAsync();

                command = new SQLiteCommand("INSERT INTO UserNotes VALUES (?,?)", connection);
                command.Parameters.Add(userId);
                command.Parameters.Add(note.Id);
                await command.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
                return note.Id;
            }

        }

        public async Task<string> CreateLabelAsync(string userId, Label label) {
            label.Id = Guid.NewGuid().ToString();
            using (var connection = new SQLiteConnection(ConnectionString)) {
                connection.Open();
                var transaction = await connection.BeginTransactionAsync();

                SQLiteCommand command = new SQLiteCommand("INSERT INTO Labels VALUES (?,?)", connection);
                command.Parameters.Add(label.Id);
                command.Parameters.Add(label.Name);
                await command.ExecuteNonQueryAsync();

                command = new SQLiteCommand("INSERT INTO UserLabels VALUES (?,?)", connection);
                command.Parameters.Add(userId);
                command.Parameters.Add(label.Id);
                await command.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
                return label.Id;
            }
        }

        public async Task AddLabelToNoteAsync(string noteId, string labelId) {
            using (var connection = new SQLiteConnection(ConnectionString)) {
                await connection.OpenAsync();

                // Verify note and label both exist
                var parm = new { Id = noteId };
                var note = await connection.QueryAsync("SELECT Id FROM Notes WHERE Id = @Id LIMIT 1", parm);
                parm = new { Id = labelId };
                var label = await connection.QueryAsync("SELECT Id FROM Label WHERE Id = @Id LIMIT 1", parm);

                if (note.Any() && label.Any()) {
                    SQLiteCommand command = new SQLiteCommand("INSERT INTO NoteLabels VALUES (?,?)", connection);
                    command.Parameters.Add(noteId);
                    command.Parameters.Add(labelId);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<Note> GetNoteAsync(string id) {
            using (var connection = new SQLiteConnection(ConnectionString)) {
                await connection.OpenAsync();
                var param = new { Id = id };
                return await connection.QueryFirstAsync<Note>("SELECT * FROM Notes WHERE Id = @Id", param);
            }
        }

    }
}
