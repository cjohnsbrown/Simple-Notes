using SimpleNotes.Api.Data;
using SimpleNotes.Api.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleNotes.Api.Tests {
    public class DatabaseTests {

        public readonly string ConnectionString = "data source=mode=memory";

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
            Assert.True(await repo.NoteExistsAsync(id));
        }

        [Fact]
        public async Task CreateLabel() {
            string userId = "1234";
            Label label = new Label { Name = "Test Label" };

            DataRepository repo = new DataRepository(ConnectionString);
            string id = await repo.CreateLabelAsync(userId, label);
            Assert.True(await repo.LabelExistsAsync(id));
        }

        [Fact]
        public async Task GetNotesForUser() {
            string userId = Guid.NewGuid().ToString();
            Note note = new Note {
                Title = "Test Title",
                Content = "Test Note",
                Pinned = true,
                ModifiedDate = "1/1/2020"
            };

            DataRepository repo = new DataRepository(ConnectionString);

            // Add a note to the database that does not belong 
            await repo.CreateNoteAsync("OtherUser", note);

            IEnumerable<Note> notes = await repo.GetUserNotesAsync(userId);
            Assert.Empty(notes);

            // Add a note for the test user
            string id = await repo.CreateNoteAsync(userId, note);
            notes = await repo.GetUserNotesAsync(userId);

            // There should only be one note for the test user
            Assert.Single(notes);
            Assert.Equal(id, notes.First().Id);
        }


        [Fact]
        public async Task GetLabelsForUser() {
            string userId = Guid.NewGuid().ToString();
            Label label = new Label { Name = "Test Label" };
            DataRepository repo = new DataRepository(ConnectionString);

            await repo.CreateLabelAsync("OtherUser", label);

            // No labels should exist for this user
            IEnumerable<Label> labels = await repo.GetUserLabelsAsync(userId);
            Assert.Empty(labels);

            // Add a label for the test user
            string id = await repo.CreateLabelAsync(userId, label);
            labels = await repo.GetUserLabelsAsync(userId);

            // There should only be one label for the test user
            Assert.Single(labels);
            Assert.Equal(id, labels.First().Id);
        }

        [Fact]
        public async Task AddLabelToNote() {
            string userId = Guid.NewGuid().ToString();
            Label label = new Label { Name = "Test Label" };
            Note note = new Note {
                Title = "Test Title",
                Content = "Test Note",
                Pinned = true,
                ModifiedDate = "1/1/2020"
            };

            DataRepository repo = new DataRepository(ConnectionString);
            string noteId = await repo.CreateNoteAsync(userId, note);
            string labelId = await repo.CreateLabelAsync(userId, label);

            await repo.AddLabelToNoteAsync(noteId, labelId);
            IEnumerable<Note> notes = await repo.GetUserNotesAsync(userId);
            IEnumerable<string> labelIds = notes.First().LabelIds;
            Assert.Single(labelIds);
            Assert.Equal(labelId, labelIds.First());
        }
    }

}
