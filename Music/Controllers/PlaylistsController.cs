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
        private readonly AuthDbContext _dbAuth;
        private readonly UserManager<MusicUser> _userManager;

        public PlaylistsController(ApplicationDbContext db, AuthDbContext dbAuth,
            UserManager<MusicUser> userManager)
        {
            _db = db;
            _dbAuth = dbAuth;
            _userManager = userManager;

        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        #region API Calls
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetPlaylists()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            IEnumerable<Playlist> myPlaylists = await
                _db.Playlists.Where(p => p.MusicUserId == userId)
                .ToListAsync();

            return Json(new { data = myPlaylists });
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeletePlaylist(int? id)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (id == null) return NotFound();
                else
                {
                    // find playlist that we wanna delete
                    Playlist toDelete = await _db.Playlists.FirstOrDefaultAsync(p => p.Id == id);
                    if (toDelete == null)
                        return Json(new { success = false, message = "Error while trying to delete the playlist" });

                    else
                    {

                        // check if we are deleting our 'own' playlist
                        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        if (userId != toDelete.MusicUserId)
                            return RedirectToAction("Index");

                        // find all rows in PlaylistSongs where PlaylistId = id
                        SqlParameter idPlaylist = new SqlParameter("@PlaylistId", id);

                        _db.PlaylistSongs
                            .FromSqlRaw("delete from dbo.PlaylistSongs " +
                            "where PlaylistId = @PlaylistId", idPlaylist);
                        _db.Playlists.Remove(toDelete);
                        await _db.SaveChangesAsync();

                        return Json(new { success = true, message = "Playlist successfully deleted" });


                    }
                    
                }
            }

            else return RedirectToPage("Identity/Account/Login");
        }
        #endregion

        // CREATE - GET
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // CREATE - POST
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title)
        {
            if (ModelState.IsValid && title.ToUpper() != "FAVORITES")
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

        // EDITPLAYLIST - GET
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
                    // first we need to check if logged in user 'owns' that playlist
                    //MusicUser currUser = await _dbAuth.Users.FindAsync(playlistObj.MusicUserId);
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (userId != playlistObj.MusicUserId)
                        return RedirectToAction("Index");

                    List <PlaylistSong> playlistSongs = await _db
                        .PlaylistSongs
                        .Include(s => s.Song)
                        .Where(ps => ps.PlaylistId == id)
                        .ToListAsync();

                    playlistObj.PlaylistSongs = playlistSongs;

                    return View(new EditPlaylistViewModel(playlistObj));
                }
            }
        }

        //ADD SONGS - GET
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AddSongs(int? id)
        {
            if (id == null) return NotFound();

            Playlist playlistObj = await _db.Playlists.FindAsync(id);

            if (playlistObj == null) return NotFound();

            // check if it's our playlist
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != playlistObj.MusicUserId)
                return RedirectToAction("Index");

            SqlParameter idParam = new SqlParameter("@PlaylistId", id);

            List<Song> remainingSongs = await _db
                .Songs
                .FromSqlRaw("select * from dbo.Songs where Id not in" +
                "(select SongId from dbo.PlaylistSongs where PlaylistId = @PlaylistId)", idParam)
                .ToListAsync();

            return View(new AddSongsToPlaylistViewModel(playlistObj, remainingSongs));
        }

        // ADDSONGS - POST
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSongs(AddSongsToPlaylistViewModel addSongsToPlaylistViewModel)
        {
            if (ModelState.IsValid)
            {
                // get playlistId from form
                int playlistId = addSongsToPlaylistViewModel.PlaylistId;
                Playlist myPlaylist = await _db.Playlists.FindAsync(playlistId);

                // get all checked checkboxes from form and convert values to int
                List<int> songsToAddIds = new List<int>();
                foreach (string strId in addSongsToPlaylistViewModel.SongsToAddIds)
                    if (strId != "false") songsToAddIds.Add(Int32.Parse(strId));

                // sql request for songs that we are trying to add but already exist in
                // PlaylistSongs table
                // SqlParameter idPlaylist = new SqlParameter("@PlaylistId", playlistId);

                //List<string> pars = new List<string>(new string[songsToAddIds.Count]);
                //List<SqlParameter> listSongIds = new List<SqlParameter>();
                //for (int i = 0; i < songsToAddIds.Count; ++i)
                //{
                //    pars[i] = string.Format("@p{0}", i);
                //    listSongIds.Add(new SqlParameter(pars[i], songsToAddIds[i]));
                //}

                //string[] paramList = songsToAddIds.Select(
                //    (s, i) => "@tag" + i.ToString()).ToArray();

                //string inClause = string.Join(", ", paramList);

                //var rawCommand = string.Format("select * from dbo.PlaylistSongs" +
                //        "where PlaylistId = {0} " +
                //       "and SongId not in ({1})", playlistId, inClause);

                //List<PlaylistSong> songsTryingToAddAlreadyInPlaylist = await _db
                //    .PlaylistSongs
                //    .FromSqlRaw(rawCommand,
                //    idPlaylist,
                //    songsToAddIds.Select((val, i) => new SqlParameter("@tag" + i.ToString(), val)))
                // .ToListAsync();



                // creating final songs to add to db
                // those are from songsToAddIds but not in songsTryingToAddAlreadyInPlaylist
                //List<int> finalSongToAddIds = new List<int>();
                //foreach (int songId in songsToAddIds)
                //{
                //    Song tmpSong = await _db.Songs.FindAsync(songId);
                //    PlaylistSong tmp = new PlaylistSong
                //    {
                //        Playlist = myPlaylist,
                //        Song = tmpSong
                //    };
                //    if (!songsTryingToAddAlreadyInPlaylist.Contains(tmp))
                //    {
                //        finalSongToAddIds.Add(songId);
                //    }
                //}

                // if (finalSongToAddIds.Count > 0)
                if (songsToAddIds.Count > 0)
                {
                    //create combo songId playlistId to pass to DB
                    //foreach (int songId in finalSongToAddIds)
                    foreach (int songId in songsToAddIds)
                    {
                        Song mySong = await _db.Songs.FindAsync(songId);
                        PlaylistSong toAddPlaylistSong = new PlaylistSong
                        {
                            Playlist = myPlaylist,
                            Song = mySong
                        };

                        await _db.PlaylistSongs.AddAsync(toAddPlaylistSong);
                    }

                    await _db.SaveChangesAsync();

                    return RedirectToAction("EditPlaylist", new
                    {
                        id = playlistId
                    });
                }

                return RedirectToAction("EditPlaylist", new
                {
                    id = playlistId
                });

            }

            else return RedirectToAction("EditPlaylist", new
            {
                id = addSongsToPlaylistViewModel.PlaylistId
            }) ;
        }

        // REMOVEMULTIPLESONGS - GET
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> RemoveMultipleSongs(int? id)
        {
            if (id == null) return NotFound();

            Playlist playlistObj = await _db.Playlists.FindAsync(id);

            if (playlistObj == null) return NotFound();

            // check if our
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != playlistObj.MusicUserId)
                return RedirectToAction("Index");

            List<Song> songsInPlaylist = await _db
                .Songs
                .FromSqlRaw("select * from dbo.Songs where Id in " +
                "(select SongId from dbo.PlaylistSongs " +
                "where PlaylistId = @PlaylistId)", new SqlParameter("@PlaylistId", id))
                .ToListAsync();

            if (songsInPlaylist.Count > 0) 
                return View(new RemoveSongsFromPlaylistViewModel(playlistObj, songsInPlaylist));

            else return NotFound();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMultipleSongs(RemoveSongsFromPlaylistViewModel removeSongsFromPlaylistViewModel)
        {
            if (ModelState.IsValid)
            {
                // get playlist
                int playlistId = removeSongsFromPlaylistViewModel.PlaylistId;
                Playlist myPlaylist = await _db.Playlists.FindAsync(playlistId);

                // get songs to remove
                List<int> songsToRemoveIds = new List<int>();
                foreach (string strId in removeSongsFromPlaylistViewModel.SongsToRemoveIds)
                    if (strId != "false") songsToRemoveIds.Add(Int32.Parse(strId));

                // remove songs from playlistsongs if there are any
                if (songsToRemoveIds.Count > 0)
                {
                    foreach(int songId in songsToRemoveIds)
                    {
                        Song mySong = await _db.Songs.FindAsync(songId);
                        PlaylistSong ps = new PlaylistSong
                        {
                            Playlist = myPlaylist,
                            Song = mySong
                        };

                        _db.PlaylistSongs.Remove(ps);
                    }

                    await _db.SaveChangesAsync();

                    return RedirectToAction("EditPlaylist", new
                    {
                        id=playlistId
                    });
                }

                else return RedirectToAction("EditPlaylist", new
                {
                    id = removeSongsFromPlaylistViewModel.PlaylistId
                });
            }

            else return RedirectToAction("EditPlaylist", new {
                id = removeSongsFromPlaylistViewModel.PlaylistId
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSingleSong(EditPlaylistViewModel epvm)
        {
            if (ModelState.IsValid)
            {
                PlaylistSong ps = new PlaylistSong
                {
                    PlaylistId = epvm.playlistId,
                    SongId = epvm.songId
                };

                _db.PlaylistSongs.Remove(ps);
                await _db.SaveChangesAsync();


                return RedirectToAction("EditPlaylist", new
                {
                    id=epvm.playlistId
                });
            }

            else
            {
                return RedirectToAction("EditPlaylist", new 
                {
                    id=epvm.playlistId
                });
            }

        }

        [Authorize]
        public async Task<IActionResult> LookAt(int? id)
        {

            // we want more data than just the playlist: 
            // 1. playlist 
            // 2. songs in playlist
            // 3. user data
            // return as UserPlaylistViewModel

            // 1. playlist
            Playlist selectedPlaylist = await _db.Playlists.FindAsync(id);

            // 2. songs in playlist
            List<Song> songsInPlaylist = await _db
                .Songs
                .FromSqlRaw("select * from dbo.Songs where Id in " +
                "(select SongId from dbo.PlaylistSongs where PlaylistId = @PID)",
                new SqlParameter("@PID", id))
                .ToListAsync();

            // 3. user data
            MusicUser viewedUser = await _dbAuth.Users.FindAsync(selectedPlaylist.MusicUserId);

            UserPlaylistViewModel upvm = new UserPlaylistViewModel(selectedPlaylist, songsInPlaylist, viewedUser.Id, viewedUser.FirstName, viewedUser.LastName, viewedUser.Email);

            return View(upvm);
        }

    }
}
