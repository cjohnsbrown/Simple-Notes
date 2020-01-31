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
        Task<UserDataModel> GetUserDataAsync(ClaimsPrincipal principal, string userKey);
    }

    public class NotesManager : INotesManager {

        IDataRepository Repository { get; }
        UserManager<ApplicationUser> UserManager { get; }

        public NotesManager(IDataRepository repository, UserManager<ApplicationUser> userManager) {
            Repository = repository;
            UserManager = userManager;
        }

        public async Task<UserDataModel> GetUserDataAsync(ClaimsPrincipal principal, string userKey) {
            ApplicationUser user = await UserManager.GetUserAsync(principal);
            UserDataModel model = new UserDataModel();
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



    }
}
