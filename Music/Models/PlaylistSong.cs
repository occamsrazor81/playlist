using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Music.Models
{
    public class PlaylistSong
    {
        public int PlaylistId { get; set; }
        public int SongId { get; set; }

        public Playlist Playlist { get; set; }

        public Song Song { get; set; }
    }
}
