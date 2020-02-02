using System.Collections.Generic;

namespace SimpleNotes.Api.Models {

    public class UserDataResponse {

        public IEnumerable<Note> Notes { get; set; }
        public IEnumerable<Label> Labels { get; set; }
    }

    public class ItemCreatedResponse {
        public string Id { get; set; }
    }

}
