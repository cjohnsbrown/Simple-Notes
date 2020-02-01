using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleNotes.Api.Models {

    public class NoteLabelRequest {
        public string LabelId { get; set; }
    }

    public class SetPinnedRequest {
        public bool Pinned { get; set; }
    }

}
