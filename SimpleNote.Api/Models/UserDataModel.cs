﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleNotes.Api.Models {
    public class UserDataModel {

        public IEnumerable<Note> Notes { get; set; }
        public IEnumerable<Label> Labels { get; set; }
    }
}