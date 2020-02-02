using Microsoft.Data.Sqlite;
using SimpleNotes.Api.Data;
using SimpleNotes.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleNotes.Api.Tests {
    public class DatabaseTests {

        public readonly string ConnectionString = "data source=mode=memory";

        public DatabaseTests() {
            using (var connection = new SqliteConnection(ConnectionString)) {
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

        [Fact]
        public async Task UpdateNote() {
            string userId = Guid.NewGuid().ToString();
            Note note = new Note {
                Title = "Test Title",
                Content = "Test Note",
                Pinned = true,
                ModifiedDate = "1/1/2020"
            };

            DataRepository repo = new DataRepository(ConnectionString);
            string noteId = await repo.CreateNoteAsync(userId, note);

            note.Title = "Changed Title";
            note.Content = "Changed Content";
            note.ModifiedDate = "1 minute ago";
            await repo.UpdateNoteAsync(note);

            IEnumerable<Note> notes = await repo.GetUserNotesAsync(userId);
            Assert.Single(notes);
            Note updatedNote = notes.First();
            Assert.Equal(noteId, updatedNote.Id);
            Assert.Equal(note.Title, updatedNote.Title);
            Assert.Equal(note.Content, updatedNote.Content);
            Assert.Equal(note.ModifiedDate, updatedNote.ModifiedDate);
        }

        [Fact]
        public async Task UpdateLabel() {
            string userId = Guid.NewGuid().ToString();
            Label label = new Label { Name = "Test Label" };

            DataRepository repo = new DataRepository(ConnectionString);
            string id = await repo.CreateLabelAsync(userId, label);

            label.Name = "Changed label";
            await repo.UpdateLabelAsync(label);

            IEnumerable<Label> labels = await repo.GetUserLabelsAsync(userId);
            Assert.Single(labels);
            Label updatedLabel = labels.First();
            Assert.Equal(label.Name, updatedLabel.Name);
        }

        [Fact]
        public async Task DeleteNote() {
            string userId = "1234";
            Note note = new Note {
                Title = "Test Title",
                Content = "Test Note",
                Pinned = true,
                ModifiedDate = "1/1/2020"
            };

            DataRepository repo = new DataRepository(ConnectionString);
            string noteId = await repo.CreateNoteAsync(userId, note);

            int count = await repo.DeleteNoteAsync(noteId);
            Assert.Equal(1, count);
            Assert.False(await repo.NoteExistsAsync(noteId));
        }

        [Fact]
        public async Task DeleteLabel() {
            string userId = "1234";
            Label label = new Label { Name = "Test Label" };
            DataRepository repo = new DataRepository(ConnectionString);
            string id = await repo.CreateLabelAsync(userId, label);

            int count = await repo.DeleteLabelAsync(id);

            Assert.Equal(1, count);
            Assert.False(await repo.LabelExistsAsync(id));
        }

        [Fact]
        public async Task DeleteUserData() {
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

            await repo.DeleteUserDataAsync(userId);

            IEnumerable<Note> notes = await repo.GetUserNotesAsync(userId);
            Assert.Empty(notes);
            IEnumerable<Label> labels = await repo.GetUserLabelsAsync(userId);
            Assert.Empty(labels);

            Assert.False(await repo.NoteExistsAsync(noteId));
            Assert.False(await repo.LabelExistsAsync(labelId));
        }

        [Fact]
        public async Task NoteBelongsToUser() {
            string userId = Guid.NewGuid().ToString();
            Note note = new Note {
                Title = "Test Title",
                Content = "Test Note",
                Pinned = true,
                ModifiedDate = "1/1/2020"
            };

            DataRepository repo = new DataRepository(ConnectionString);
            string noteId = await repo.CreateNoteAsync(userId, note);

            Assert.True(await repo.NoteBelongsToUserAsync(userId, noteId));
        }

        [Fact]
        public async Task NoteDoesNotBelongToUser() {
            string userId = Guid.NewGuid().ToString();
            Note note = new Note {
                Title = "Test Title",
                Content = "Test Note",
                Pinned = true,
                ModifiedDate = "1/1/2020"
            };

            DataRepository repo = new DataRepository(ConnectionString);
            string noteId = await repo.CreateNoteAsync(userId, note);

            Assert.False(await repo.NoteBelongsToUserAsync("Wrong User", noteId));
        }

        [Fact]
        public async Task LabelBelongsToUser() {
            string userId = Guid.NewGuid().ToString();
            Label label = new Label { Name = "Test Label" };

            DataRepository repo = new DataRepository(ConnectionString);
            string labelId = await repo.CreateLabelAsync(userId, label);

            Assert.True(await repo.LabelBelongsToUser(userId, labelId));
        }

        [Fact]
        public async Task LabelDoeNoteBelongsToUser() {
            string userId = Guid.NewGuid().ToString();
            Label label = new Label { Name = "Test Label" };

            DataRepository repo = new DataRepository(ConnectionString);
            string labelId = await repo.CreateLabelAsync(userId, label);

            Assert.False(await repo.LabelBelongsToUser("Wrong User", labelId));
        }

        [Fact]
        public async Task RemoveLabelFromNote() {
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
            int deleteCount = await repo.RemoveLabelFromNote(noteId, labelId);
            Assert.Equal(1, deleteCount);
        }
    }

} 