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
                bool noteExists = await NoteExistsAsync(noteId);
                bool labelExists = await LabelExistsAsync(labelId);

                if (noteExists && labelExists) {
                    var insertParm = new { NoteId = noteId, LabelId = labelId };
                    string sql = "INSERT INTO NoteLabels VALUES (@NoteId, @LabelId)";
                    await connection.ExecuteAsync(sql, insertParm);
                }
            }
        }

        public async Task<bool> NoteExistsAsync(string id) {
            using (var connection = new SQLiteConnection(ConnectionString)) {
                await connection.OpenAsync();
                var param = new { Id = id };
                var note = await connection.QueryFirstOrDefaultAsync<string>("SELECT Id FROM Notes WHERE Id = @Id", param);
                return note !=null;
            }
        }


        public async Task<bool> LabelExistsAsync(string id) {
            using (var connection = new SQLiteConnection(ConnectionString)) {
                await connection.OpenAsync();
                var param = new { Id = id };
                var label = await connection.QueryFirstOrDefaultAsync<string>("SELECT Id FROM Labels WHERE Id = @Id", param);
                return label != null;
            }
        }

        public async Task UpdateNoteAsync(Note note) {
            if (!await NoteExistsAsync(note.Id)) {
                return;
            }

            using (var connection = new SQLiteConnection(ConnectionString)) {
                await connection.OpenAsync();
                string sql = @"UPDATE Notes SET
                               Title = @Title, Content = @Content, 
                               ModifiedDate = @ModifiedDate
                               WHERE Id = @Id";

                await connection.ExecuteAsync(sql, note);
            }

        }

        public async Task UpdateNotePinnedAsync(Note note) {
            if (!await NoteExistsAsync(note.Id)) {
                return;
            }

            using (var connection = new SQLiteConnection(ConnectionString)) {
                await connection.OpenAsync();
                string sql = @"UPDATE Notes SET
                               Pinned = @Pinned
                               WHERE Id = @Id";

                await connection.ExecuteAsync(sql, note);
            }
        }

        public async Task UpdateLabelAsync(Label label) {
            if (!await LabelExistsAsync(label.Id)) {
                return;
            }

            using (var connection = new SQLiteConnection(ConnectionString)) {
                await connection.OpenAsync();
                string sql = @"UPDATE Labels SET Name = @Name WHERE Id = @Id";
                await connection.ExecuteAsync(sql, label);
            }
        }

        public async Task DeleteUserDataAsync(string userId) {
            using (var connection = new SQLiteConnection(ConnectionString)) {
                await connection.OpenAsync();
                var transaction = await connection.BeginTransactionAsync();
                var param = new { Id = userId };

                // Delete all notes that belong to the given user
                string sql = "SELECT NoteId FROM UserNotes WHERE UserId = @Id";
                IEnumerable<string> noteIds = await connection.QueryAsync<string>(sql, param);
                foreach(string id in noteIds) {
                    await DeleteNoteAsync(id, connection);
                }

                // Delete all labels that belong to the given user
                sql = "SELECT LabelId FROM UserLabels WHERE UserId = @Id";
                IEnumerable<string> labelIds = await connection.QueryAsync<string>(sql, param);
                foreach(string id in labelIds) {
                    await DeleteLabelAsync(id, connection);
                }

                await transaction.CommitAsync();
            }
        }

        public async Task<int> DeleteNoteAsync(string id) {
            if (!await NoteExistsAsync(id)) {
                return 0 ;
            }

            using (var connection = new SQLiteConnection(ConnectionString)) {
                await connection.OpenAsync();
                var transaction = await connection.BeginTransactionAsync();
                int deleteCount = await DeleteNoteAsync(id, connection);
                await transaction.CommitAsync();
                return deleteCount;
            }
        }

        public async Task<int> DeleteLabelAsync(string id) {
            if (!await LabelExistsAsync(id)) {
                return 0;
            }

            using (var connection = new SQLiteConnection(ConnectionString)) {
                await connection.OpenAsync();
                var transaction = await connection.BeginTransactionAsync();
                int deleteCount = await DeleteLabelAsync(id, connection);
                await transaction.CommitAsync();
                return deleteCount;
            }
        }

        private async Task<int> DeleteNoteAsync(string id, SQLiteConnection connection) {
            var param = new { Id = id };
            await connection.ExecuteAsync("DELETE FROM UserNotes WHERE NoteId = @Id", param);
            await connection.ExecuteAsync("DELETE FROM NoteLabels WHERE NoteId = @Id", param);
            return await connection.ExecuteAsync("DELETE FROM Notes WHERE Id = @Id", param);
        }

        private async Task<int> DeleteLabelAsync(string id, SQLiteConnection connection) {
            var param = new { Id = id };
            await connection.ExecuteAsync("DELETE FROM UserLabels WHERE LabelId = @Id", param);
            await connection.ExecuteAsync("DELETE FROM NoteLabels WHERE LabelId = @Id", param);
            return await connection.ExecuteAsync("DELETE FROM Labels WHERE Id = @Id", param);
        }
    }
}
