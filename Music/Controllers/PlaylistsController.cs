using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Music.Areas.Identity.Data;
using Music.Data;
using Music.Models;
using Music.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Music.Controllers
{
    public class PlaylistsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<MusicUser> _userManager;

        public PlaylistsController(ApplicationDbContext db, UserManager<MusicUser> userManager)
        {
            _db = db;
            _userManager = userManager;

        }

        [Authorize]
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            IEnumerable<Playlist> myPlaylists = 
                _db.Playlists.Where(p => p.MusicUserId == userId);
                                
            return View(myPlaylists);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Playlist playlistObj = new Playlist();
                playlistObj.MusicUserId = userId;
                playlistObj.Title = title;

                await _db.Playlists.AddAsync(playlistObj);
                await _db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            else return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditPlaylist(int? id)
        {
            if (id == null) return NotFound();
            else
            {
                Playlist playlistObj = await _db.Playlists.FindAsync(id);
                if (playlistObj == null) return NotFound();
                else
                {
                    List<PlaylistSong> playlistSongs = await _db
                        .PlaylistSongs
                        .Include(s => s.Song)
                        .Where(ps => ps.PlaylistId == id)
                        .ToListAsync();

                    playlistObj.PlaylistSongs = playlistSongs;
                
                    return View(playlistObj);
                }
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AddSongs(int? id)
        {
            if (id == null) return NotFound();

            Playlist playlistObj = await _db.Playlists.FindAsync(id);

            SqlParameter idParam = new SqlParameter("@PlaylistId", id);

            List<Song> remainingSongs = await _db
                .Songs
                .FromSqlRaw("select * from dbo.Songs where Id not in" +
                "(select SongId from dbo.PlaylistSongs where PlaylistId = @PlaylistId)", idParam)
                .ToListAsync();

            return View(new AddSongsToPlaylistViewModel(playlistObj, remainingSongs));
        }

    }
}
