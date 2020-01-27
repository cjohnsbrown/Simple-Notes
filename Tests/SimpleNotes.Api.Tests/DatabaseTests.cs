using SimpleNotes.Api.Data;
using SimpleNotes.Api.Models;
using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using Xunit;

namespace SimpleNotes.Api.Tests {
    public class DatabaseTests {

        public readonly string ConnectionString = "data source=testDB;mode=memory";

        public DatabaseTests() {
            using (var connection = new SQLiteConnection(ConnectionString)) {
                DbInitializer.AddTables(connection.CreateCommand());
            }
        }

        [Fact]
        public async Task CreateNote() {
            string userId = "1234";
            Note note = new Note {
                Title = "Test Title",
                Content = "Test Note",
                Pinned = true,
                ModifiedDate = "1/1/2020"
            };

            DataRepository repo = new DataRepository(ConnectionString);
            string id = await repo.CreateNoteAsync(userId, note);
            Note storedNote = await repo.GetNoteAsync(id);
            Assert.NotNull(storedNote);
        }

        [Fact]
        public async Task CreateLabel() {
            string userId = "1234";
            Label label = new Label { Name = "Test Label" };

            DataRepository repo = new DataRepository(ConnectionString);
            string id = await repo.CreateLabelAsync(userId, label);
            Label storedLabel = await repo.GetLabelAsync(id);
            Assert.NotNull(storedLabel);
        }
    }

}
