using Music.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Music.ViewModels
{
    public class EditPlaylistViewModel
    {
        public int playlistId { get; set; }
        public int songId { get; set; }

        public Playlist editPlaylist { get; set; }

        public EditPlaylistViewModel()
        {

        }

        public EditPlaylistViewModel(Playlist pl)
        {
            editPlaylist = pl;
        }
    }
}
