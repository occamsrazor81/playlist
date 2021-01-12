using Music.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Music.ViewModels
{
    public class RemoveSongsFromPlaylistViewModel
    {
        public Playlist Playlist { get; set; }

        public List<Song> SongsInPlaylist { get; set; }

        public List<string> SongsToRemoveIds { get; set; }

        public int PlaylistId { get; set; }

        public RemoveSongsFromPlaylistViewModel() { }

        public RemoveSongsFromPlaylistViewModel(Playlist p, List<Song> songsAlreadyIn)
        {
            Playlist = p;
            SongsInPlaylist = songsAlreadyIn;
            SongsToRemoveIds = new List<string>(new string[songsAlreadyIn.Count]);
        }
    }
}
