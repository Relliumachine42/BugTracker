﻿using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels
{
    public class ProjectMembersViewModel
    {
        public Project? Project { get; set; }
        public MultiSelectList? UsersList { get; set; }
        public IEnumerable<string>? SelectedMembers { get; set; }

    }
}
