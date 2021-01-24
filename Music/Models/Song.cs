using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Music.Models
{
    public class Song
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Artist { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        [Range(1800, 2100, ErrorMessage = "There were no songs prior to 1800.")]
        [DisplayName("Year Published")]
        public int YearPublished { get; set; }

        [Required]
        public string Link { get; set; }

        public List<PlaylistSong> PlaylistSongs { get; set; }

 
    }
}
