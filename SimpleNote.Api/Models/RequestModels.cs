namespace SimpleNotes.Api.Models {

    public class NoteLabelRequest {
        public string LabelId { get; set; }
    }

    public class SetPinnedRequest {
        public bool Pinned { get; set; }
    }

}
