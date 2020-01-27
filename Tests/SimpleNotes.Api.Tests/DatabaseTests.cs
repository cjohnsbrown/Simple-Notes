using SimpleNotes.Api.Data;
using SimpleNotes.Api.Models;
using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using Xunit;

namespace SimpleNotes.Api.Tests {
    public class DatabaseTests : IClassFixture<DatabaseFixture> {

        DatabaseFixture Fixture { get; }

        public DatabaseTests(DatabaseFixture fixture) {
            Fixture = fixture;
        }
        
        [Fact]
        public async Task CreateNotes() {
            string userId = "1234";
            Note note = new Note {
                Title = "Test Title",
                Content = "Test Note",
                Pinned = true,
                ModifiedDate = "1/1/2020"
            };

            DataRepository repo = new DataRepository(Fixture.ConnectionString);
            string id = await repo.CreateNoteAsync(userId, note);
            Note storedNote = await repo.GetNoteAsync(id);
            Assert.NotNull(storedNote);
        }
    }


    public class DatabaseFixture : IDisposable {

        public readonly string ConnectionString = "data source=testDB;mode=memory";

        public DatabaseFixture() {
            using (var connection = new SQLiteConnection(ConnectionString)) {
                DbInitializer.AddTables(connection.CreateCommand());
            }
        }

        public void Dispose() {
        }
    }
}
