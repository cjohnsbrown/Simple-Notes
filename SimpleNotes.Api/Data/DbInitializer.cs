using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace SimpleNotes.Api.Data {
    public static class DbInitializer {

        /// <summary>
        /// Create the Application database if it does not exis
        /// } finally {
        /// command.Connection.Close();
        /// command.Connection.Open();t
        /// </summary>
        /// <param name="context"></param>
        public static void Init(ApplicationContext context) {
            context.Database.Migrate();
            AddTables(context.Database.GetDbConnection().CreateCommand());
        }

        /// <summary>
        ///  Add tables to the database that will not be handled by Entity framework
        /// </summary>
        /// <param name="command"></param>
        public static void AddTables(DbCommand command) {
            try {
                command.Connection.Open();
                string createTable = "CREATE TABLE IF NOT EXISTS ";
                string table = "Notes (Id TEXT PRIMARY KEY, Title TEXT, Content TEXT, Pinned BOOLEAN, ModifiedDate TEXT)";
                command.CommandText = createTable + table;
                command.ExecuteNonQuery();

                table = "Labels (Id TEXT PRIMARY KEY, Name TEXT)";
                command.CommandText = createTable + table;
                command.ExecuteNonQuery();

                table = "UserNotes (UserId TEXT, NoteId TEXT)";
                command.CommandText = createTable + table;
                command.ExecuteNonQuery();

                table = "UserLabels (UserId TEXT, LabelId TEXT)";
                command.CommandText = createTable + table;
                command.ExecuteNonQuery();

                table = "NoteLabels (NoteId TEXT, LabelId TEXT)";
                command.CommandText = createTable + table;
                command.ExecuteNonQuery();
            } finally {
                command.Connection.Close();
            }
        }

    }
}
