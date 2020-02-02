using System.Collections.Generic;

namespace SimpleNotes.Api.Models {
    public class Note {

        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool Pinned { get; set; }
        public string ModifiedDate { get; set; }
        public IEnumerable<string> LabelIds { get; set; }

    }

}
