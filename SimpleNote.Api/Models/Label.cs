using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleNotes.Api.Models {
    public class Label {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
