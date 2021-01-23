using Music.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Music.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public List<Playlist> Playlists { get; set; }

        public UserViewModel() { }

        public UserViewModel(string id, string fname, string lname, string email)
        {
            Id = id;
            FirstName = fname;
            LastName = lname;
            Email = email;
        }

    }
}
