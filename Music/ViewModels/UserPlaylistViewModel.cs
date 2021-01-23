using Music.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Music.ViewModels
{
    public class UserPlaylistViewModel
    {

        // 1. playlist
        public Playlist Playlist { get; set; }

        // 2. songs in playlist
        public List<Song> PlaylistSongs { get; set; }

        // 3. user data
        public string UserId { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserEmail { get; set; }

        public UserPlaylistViewModel() { }
        public UserPlaylistViewModel(Playlist p, List<Song> sgs, string uId, string fname, string lname, string email)
        {
            Playlist = p;
            PlaylistSongs = sgs;
            UserId = uId;
            UserFirstName = fname;
            UserLastName = lname;
            UserEmail = email;

        }
    }
}
