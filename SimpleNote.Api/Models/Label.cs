using System.ComponentModel.DataAnnotations;

namespace SimpleNotes.Api.Models {
    public class Label {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
