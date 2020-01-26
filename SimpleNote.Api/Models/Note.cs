using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleNotes.Api.Models {
    public class Note {

        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<Label> Labels { get; set; }

    }
}
