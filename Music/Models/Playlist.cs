using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Music.Models
{
    public class Playlist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        [ForeignKey("MusicUser")]
        public string MusicUserId { get; set; }

        public List<PlaylistSong> PlaylistSongs { get; set; }

    }
}
