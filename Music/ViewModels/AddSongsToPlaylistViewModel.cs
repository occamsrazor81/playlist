using Music.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Music.ViewModels
{
    public class AddSongsToPlaylistViewModel
    {
        public Playlist Playlist { get; set; }

        public List<Song> RemainingSongs { get; set; }

        public List<string> SongsToAddIds { get; set; }

        public int PlaylistId { get; set; }

        public AddSongsToPlaylistViewModel() { }

        public AddSongsToPlaylistViewModel(Playlist p, List<Song> remSongs) 
        {
            Playlist = p;
            RemainingSongs = remSongs;
            SongsToAddIds = new List<string>(new string[remSongs.Count]);
        }
    }
}
