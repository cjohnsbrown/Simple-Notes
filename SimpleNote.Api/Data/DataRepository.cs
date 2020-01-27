using Dapper;
using Microsoft.Extensions.Configuration;
using SimpleNotes.Api.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleNotes.Api.Data {
    public class DataRepository : IDataRepository {

        private string ConnectionString { get; }

        public DataRepository(IConfiguration config) {
            ConnectionString = config.GetConnectionString("DefaultConnection");
        }

        public DataRepository(string connectionString) {
            ConnectionString = connectionString;
        }

        public async Task<IEnumerable<Label>> GetUserLabelsAsync(string userId) {
            using (var connection = new SQLiteConnection(ConnectionString)) {
                connection.Open();
                string query = @"SELECT * FROM Labels WHERE Id IN
                                    (SELECT LabelId FROM UserLabels WHERE UserId = @Id)";
                var param = new { Id = userId };
                return await connection.QueryAsync<Label>(query, param);
            }
        }

        public async Task<IEnumerable<Note>> GetUserNotesAsync(string userId) {
            using (var connection = new SQLiteConnection(ConnectionString)) {
                connection.Open();
                string query = @"SELECT * FROM Notes WHERE Id IN
                                    (SELECT NoteId FROM UserNotes WHERE UserId = @Id)";
                var param = new { Id = userId };
                var notes = await connection.QueryAsync<Note>(query, param);

                // Get labels added to each note
                query = @"SELECT LabelId FROM NoteLabels
                          WHERE NoteId = @Id";
                foreach (Note note in notes) {
                    param = new { Id = note.Id };
                    note.LabelIds = await connection.QueryAsync<string>(query, param);
                }
                return notes;
            }
        }

        public async Task<string> CreateNoteAsync(string userId, Note note) {
            note.Id = Guid.NewGuid().ToString();
            using (var connection = new SQLiteConnection(ConnectionString)) {
                connection.Open();
                var transaction = await connection.BeginTransactionAsync();
                string sql = @"INSERT INTO Notes VALUES (@Id, @Title, @Content, @Pinned, @ModifiedDate)";
                await connection.ExecuteAsync(sql, note, transaction);

                sql = "INSERT INTO UserNotes VALUES (@UserId, @NoteId)";
                var param = new { UserId = userId, NoteId = note.Id };
                await connection.ExecuteAsync(sql, param, transaction);

                await transaction.CommitAsync();
                return note.Id;
            }

        }

        public async Task<string> CreateLabelAsync(string userId, Label label) {
            label.Id = Guid.NewGuid().ToString();
            using (var connection = new SQLiteConnection(ConnectionString)) {
                connection.Open();
                var transaction = await connection.BeginTransactionAsync();
                string sql = "INSERT INTO Labels VALUES (@Id, @Name)";
                await connection.ExecuteAsync(sql, label, transaction);

                sql = "INSERT INTO UserLabels VALUES (@UserId, @LabelId)";
                var param = new { UserId = userId, LabelID = label.Id };
                await connection.ExecuteAsync(sql, param, transaction);

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
                var label = await connection.QueryAsync("SELECT Id FROM Labels WHERE Id = @Id LIMIT 1", parm);

                if (note.Any() && label.Any()) {
                    var insertParm = new { NoteId = noteId, LabelId = labelId };
                    string sql = "INSERT INTO NoteLabels VALUES (@NoteId, @LabelId)";
                    await connection.ExecuteAsync(sql, insertParm);
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


        public async Task<Label> GetLabelAsync(string id) {
            using (var connection = new SQLiteConnection(ConnectionString)) {
                await connection.OpenAsync();
                var param = new { Id = id };
                return await connection.QueryFirstAsync<Label>("SELECT * FROM Labels WHERE Id = @Id", param);
            }
        }

    }
}
