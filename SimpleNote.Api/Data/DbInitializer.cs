using Microsoft.EntityFrameworkCore;

namespace SimpleNotes.Api.Data {
    public static class DbInitializer {

        /// <summary>
        /// Create the Application database if it does not exist
        /// </summary>
        /// <param name="context"></param>
        public static void Init(ApplicationContext context) {
            context.Database.Migrate();
            string createTable = "CREATE TABLE IF NOT EXISTS ";
            string table = "Notes (Id TEXT PRIMARY KEY, Title TEXT, Content TEXT, Pinned BOOLEAN, Modified DATETIME)";
            context.Database.ExecuteSqlRaw(createTable + table);
            table = "Labels (Id TEXT PRIMARY KEY, Name TEXT)";
            context.Database.ExecuteSqlRaw(createTable + table);
            table = "UserNotes (UserId TEXT, NoteId TEXT)";
            context.Database.ExecuteSqlRaw(createTable + table);
            table = "UserLabels (UserId TEXT, LabelId TEXT)";
            context.Database.ExecuteSqlRaw(createTable + table);
            table = "NoteLabels (NoteId TEXT, LabelId TEXT)";
            context.Database.ExecuteSqlRaw(createTable + table);
        }

    }
}
