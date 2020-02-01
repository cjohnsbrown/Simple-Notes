using Microsoft.AspNetCore.Identity;
using SimpleNotes.Api.Data;
using SimpleNotes.Api.Models;
using System;
using System.Collections.Generic;
using SimpleNotes.Cryptography;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleNotes.Api.Services {

    /// <summary>
    /// Manage note data stored the database. This layer also
    /// handle the encryption/decryption
    /// </summary>
    public interface INotesManager {
        Task<string> CreateNoteAsync(ClaimsPrincipal principal, string userKey, Note note);
        Task<UserDataResponse> GetUserDataAsync(ClaimsPrincipal principal, string userKey);
        Task<string> CreateLabelAsync(ClaimsPrincipal principal, string userKey, Label label);
        Task AddLabelToNoteAsync(string noteId, string labelId);
        Task UpdateNoteAsync(ClaimsPrincipal principal, string userKey, Note note);
        Task UpdateNotePinnedAsync(string noteId, bool pinned);
        Task DeleteNoteAsync(string noteId);
        Task<bool> NoteBelongsToUserAsync(ClaimsPrincipal principal, string noteId);
    }

    public class NotesManager : INotesManager {

        IDataRepository Repository { get; }
        UserManager<ApplicationUser> UserManager { get; }

        public NotesManager(IDataRepository repository, UserManager<ApplicationUser> userManager) {
            Repository = repository;
            UserManager = userManager;
        }

        public async Task<UserDataResponse> GetUserDataAsync(ClaimsPrincipal principal, string userKey) {
            ApplicationUser user = await UserManager.GetUserAsync(principal);
            UserDataResponse model = new UserDataResponse();
            model.Notes = await Repository.GetUserNotesAsync(user.Id);
            model.Labels = await Repository.GetUserLabelsAsync(user.Id);
            string secretKey = Crypto.Decrypt(userKey, user.SecretKey);

            foreach (var note in model.Notes) {
                note.Title = Crypto.Decrypt(secretKey, note.Title);
                note.Content = Crypto.Decrypt(secretKey, note.Content);
            }

            foreach (var label in model.Labels) {
                label.Name = Crypto.Decrypt(secretKey, label.Name);
            }

            return model;
        }

        public async Task<string> CreateNoteAsync(ClaimsPrincipal principal, string userKey, Note note) {
            ApplicationUser user = await UserManager.GetUserAsync(principal);
            string secretKey = Crypto.Decrypt(userKey, user.SecretKey);

            note.Title = Crypto.Encrypt(secretKey, note.Title);
            note.Content = Crypto.Encrypt(secretKey, note.Content);
            return await Repository.CreateNoteAsync(user.Id, note);
        }

        public async Task<string> CreateLabelAsync(ClaimsPrincipal principal, string userKey, Label label) {
            ApplicationUser user = await UserManager.GetUserAsync(principal);
            string secretKey = Crypto.Decrypt(userKey, user.SecretKey);

            label.Name = Crypto.Encrypt(secretKey, label.Name);
            return await Repository.CreateLabelAsync(user.Id, label);
        }

        public async Task AddLabelToNoteAsync(string noteId, string labelId) {
            await Repository.AddLabelToNoteAsync(noteId, labelId);
        }

        public async Task UpdateNoteAsync(ClaimsPrincipal principal, string userKey, Note note) {
            ApplicationUser user = await UserManager.GetUserAsync(principal);
            string secretKey = Crypto.Decrypt(userKey, user.SecretKey);

            note.Title = Crypto.Encrypt(secretKey, note.Title);
            note.Content = Crypto.Encrypt(secretKey, note.Content);
            await Repository.UpdateNoteAsync(note);

        }

        public async Task UpdateNotePinnedAsync(string noteId, bool pinned) {
            await Repository.UpdateNotePinnedAsync(noteId, pinned); 
        }

        public async Task DeleteNoteAsync(string noteId) {
            await Repository.DeleteNoteAsync(noteId);
        }

        public async Task<bool> NoteBelongsToUserAsync(ClaimsPrincipal principal, string noteId) {
            ApplicationUser user = await UserManager.GetUserAsync(principal);
            return await Repository.NoteBelongsToUserAsync(user.Id, noteId);
        }

        public async Task<bool> LabelNameExistsAsync(ClaimsPrincipal principal, string userKey, string labelName) {
            ApplicationUser user = await UserManager.GetUserAsync(principal);
            string secretKey = Crypto.Decrypt(userKey, user.SecretKey);

            var labels =  await Repository.GetUserLabelsAsync(user.Id);
            return labels.Select(label => Crypto.Decrypt(secretKey, label.Name))
                         .Any(name => name.Equals(labelName, StringComparison.InvariantCultureIgnoreCase));
                                   
        }

        public async Task<bool> LabelBelongdsToUserAsync(ClaimsPrincipal principal, string labelId) {
            ApplicationUser user = await UserManager.GetUserAsync(principal);
            return await Repository.LabelBelongsToUser(user.Id, labelId);
        }

        public async Task DeleteLabelAsync(string labelId) {
            await Repository.DeleteLabelAsync(labelId);
        }

        public async Task UpdateLabelAsync(ClaimsPrincipal principal, string userKey, Label label) {
            ApplicationUser user = await UserManager.GetUserAsync(principal);
            string secretKey = Crypto.Decrypt(userKey, user.SecretKey);

            label.Name = Crypto.Encrypt(secretKey, label.Name);
            await Repository.UpdateLabelAsync(label);
        }
    }
}
