using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SimpleNotes.Api.Data;
using SimpleNotes.Api.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleNotes.Api.Services {

    /// <summary>
    /// Manage note data stored the database. This layer also
    /// handle the encryption/decryption
    /// </summary>
    public interface INotesManager {
        Task<string> CreateNoteAsync(HttpContext context, Note note);
        Task<UserDataResponse> GetUserDataAsync(HttpContext context);
        Task<string> CreateLabelAsync(HttpContext context, Label label);
        Task AddLabelToNoteAsync(string noteId, string labelId);
        Task UpdateNoteAsync(HttpContext context, Note note);
        Task UpdateNotePinnedAsync(string noteId, bool pinned);
        Task DeleteNoteAsync(string noteId);
        Task<bool> NoteBelongsToUserAsync(ClaimsPrincipal principal, string noteId);
        Task<bool> LabelNameExistsAsync(HttpContext context, string labelName);
        Task<bool> LabelBelongsToUserAsync(ClaimsPrincipal principal, string labelId);
        Task DeleteLabelAsync(string labelId);
        Task UpdateLabelAsync(HttpContext context, Label label);
    }

    public class NotesManager : INotesManager {

        IDataRepository Repository { get; }
        UserManager<ApplicationUser> UserManager { get; }
        ICryptoService Crypto { get; }

        public NotesManager(IDataRepository repository, UserManager<ApplicationUser> userManager, ICryptoService crypto) {
            Repository = repository;
            UserManager = userManager;
            Crypto = crypto;
        }

        public async Task<UserDataResponse> GetUserDataAsync(HttpContext context) {
            string userKey = context.Session.GetString(ICryptoService.UserKey);
            ApplicationUser user = await UserManager.GetUserAsync(context.User);
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

        public async Task<string> CreateNoteAsync(HttpContext context, Note note) {
            ApplicationUser user = await UserManager.GetUserAsync(context.User);
            string userKey = context.Session.GetString(ICryptoService.UserKey);
            string secretKey = Crypto.Decrypt(userKey, user.SecretKey);

            note.Title = Crypto.Encrypt(secretKey, note.Title);
            note.Content = Crypto.Encrypt(secretKey, note.Content);
            return await Repository.CreateNoteAsync(user.Id, note);
        }

        public async Task<string> CreateLabelAsync(HttpContext context, Label label) {
            ApplicationUser user = await UserManager.GetUserAsync(context.User);
            string userKey = context.Session.GetString(ICryptoService.UserKey);
            string secretKey = Crypto.Decrypt(userKey, user.SecretKey);

            label.Name = Crypto.Encrypt(secretKey, label.Name);
            return await Repository.CreateLabelAsync(user.Id, label);
        }

        public async Task AddLabelToNoteAsync(string noteId, string labelId) {
            await Repository.AddLabelToNoteAsync(noteId, labelId);
        }

        public async Task UpdateNoteAsync(HttpContext context, Note note) {
            ApplicationUser user = await UserManager.GetUserAsync(context.User);
            string userKey = context.Session.GetString(ICryptoService.UserKey);
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

        public async Task<bool> LabelNameExistsAsync(HttpContext context, string labelName) {
            ApplicationUser user = await UserManager.GetUserAsync(context.User);
            string userKey = context.Session.GetString(ICryptoService.UserKey);
            string secretKey = Crypto.Decrypt(userKey, user.SecretKey);

            var labels =  await Repository.GetUserLabelsAsync(user.Id);
            return labels.Select(label => Crypto.Decrypt(secretKey, label.Name))
                         .Any(name => name.Equals(labelName, StringComparison.InvariantCultureIgnoreCase));
                                   
        }

        public async Task<bool> LabelBelongsToUserAsync(ClaimsPrincipal principal, string labelId) {
            ApplicationUser user = await UserManager.GetUserAsync(principal);
            return await Repository.LabelBelongsToUser(user.Id, labelId);
        }

        public async Task DeleteLabelAsync(string labelId) {
            await Repository.DeleteLabelAsync(labelId);
        }

        public async Task UpdateLabelAsync(HttpContext context, Label label) {
            ApplicationUser user = await UserManager.GetUserAsync(context.User);
            string userKey = context.Session.GetString(ICryptoService.UserKey);
            string secretKey = Crypto.Decrypt(userKey, user.SecretKey);

            label.Name = Crypto.Encrypt(secretKey, label.Name);
            await Repository.UpdateLabelAsync(label);
        }
    }
}
