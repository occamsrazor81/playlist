using Microsoft.EntityFrameworkCore;
using Music.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Music.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Song> Songs { get; set; }
        public DbSet<Playlist> Playlists { get; set; } 
        public DbSet<PlaylistSong> PlaylistSongs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PlaylistSong>().HasKey(ps => new { ps.PlaylistId, ps.SongId });
            modelBuilder.Entity<PlaylistSong>().
                HasOne(ps => ps.Playlist).WithMany(p => p.PlaylistSongs).HasForeignKey(ps => ps.PlaylistId);

            modelBuilder.Entity<PlaylistSong>().
                HasOne(ps => ps.Song).WithMany(s => s.PlaylistSongs).HasForeignKey(ps => ps.SongId);
        }
    }
}
