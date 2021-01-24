using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Music.Data;
using Music.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Music.Controllers
{
    public class SongsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public SongsController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }


        // ADD NEW SONG -> GET
        [Authorize]
        public IActionResult Add()
        {
            return View();
        }




        // GET - EDIT
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            else
            {
                Song songToUpdate = await _db.Songs.FirstOrDefaultAsync(u => u.Id == id);
                return View(songToUpdate);
            }
        }

        // POST -EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(Song songToUpdate)
        {
            bool tryCreateResult = Uri.TryCreate(songToUpdate.Link, UriKind.Absolute, out Uri uriResult);
            if (ModelState.IsValid && tryCreateResult && uriResult != null)
            {
                string capitalizeTitle = System.Threading.Thread.CurrentThread.CurrentCulture.
                   TextInfo.ToTitleCase(songToUpdate.Title.ToLower());
                string capitalizeAuthor = System.Threading.Thread.CurrentThread.CurrentCulture.
                    TextInfo.ToTitleCase(songToUpdate.Artist.ToLower());
                string capitalizeCategory = System.Threading.Thread.CurrentThread.CurrentCulture.
                    TextInfo.ToTitleCase(songToUpdate.Category.ToLower());

                songToUpdate.Title = capitalizeTitle;
                songToUpdate.Artist = capitalizeAuthor;
                songToUpdate.Category = capitalizeCategory;

                _db.Songs.Update(songToUpdate);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else return View(songToUpdate);
        }

        #region API Calls

        [HttpGet]
        public async Task<IActionResult> GetAllSongs()
        {
            var songs = _db.Songs.ToList();
            return Json(new { data = await _db.Songs.ToListAsync() });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (id == null) return NotFound();
                else
                {
                    Song songToDelete = await _db.Songs.FirstOrDefaultAsync(s => s.Id == id);
                    if (songToDelete == null)
                    {
                        return Json(new { success = false, message = "Error while deleting" });
                    }

                    else
                    {

                        // delete row in PlaylistSongs where SongId = songToDelete.Id
                        SqlParameter songId = new SqlParameter("@SongId", songToDelete.Id);
                        _db.PlaylistSongs
                            .FromSqlRaw("delete from dbo.PlaylistSongs " +
                            "where SongId = @SongId", songId);

                        _db.Songs.Remove(songToDelete);
                        await _db.SaveChangesAsync();
                        return Json(new { success = true, message = "Song successfully deleted" });
                    }

                }
            }

            else return Json(new { success = false, message = "Request failed (unauthorized access)" });
        }


        // LIKE -> GET
        [HttpPost]
        public async Task<IActionResult> Like(int? id)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (id == null) return NotFound();
                else
                {
                    // pogledaj jel pjesma vec likeana od tog korisnika
                    // odnosno treba provjeriti nalazi li se u playlisti FAVORITES
                    // pjesma koja ima Id = id
                    // da bi to provjerili trebamo povezati tablice PlaylistSongs
                    // i Playlist (u Playlist pise ime playliste)
                    // dakle trebamo dohvatiti id playliste koja se zove FAVORITES
                    // i koju je kreirao trenutni user

                    // ako je pjesma vec likeana onda samo posalji poruku da je vec likeana

                    // ako nije onda
                    // dodaj pjesmu u tablicu PlaylistSongs 
                    // i to u posebnu playlistu zvanu FAVORITES
                    // takoder je potrebno u playlistu FAVORITES dodati tu pjesmu
                    // ako playlista FAVORITES ne postoji potrebno ju je napraviti
                    // playlistu FAVORITES NE SMIJE BITI MOGUĆE obrisati ni mijenjati

                    // takoder trebamo obratit pozornost na playlistu BLACKLIST
                    // koja sadrzi dislikeane pjesme
                    // ako BLACKLISTA vec sadrzi pjesmu sa Id = id
                    // onda je moramo maknut iz BLACKLISTE
                    // (jer ne moze biti i u FAVORITES i BLACKLIST istovremeno)



                    // dohvati FAVORITES playlistu
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    Playlist favPlaylist = await
                        _db.Playlists.SingleOrDefaultAsync(p => 
                        p.MusicUserId == userId && p.Title == "FAVORITES");

                    // dohvati BLACKLIST playlistu
                    Playlist blacklist = await
                        _db.Playlists.SingleOrDefaultAsync(p =>
                        p.MusicUserId == userId && p.Title == "BLACKLIST");

                    // ako BLACKLIST postoji onda moramo provjeriti jel pjesmu unutra
                    // inace ne trebamo nista napraviti
                    if(blacklist != null)
                    {
                        // provjeravamo jel pjesma u BLACKLISTi
                        PlaylistSong alreadyBlacklisted = await
                            _db.PlaylistSongs.SingleOrDefaultAsync(ps => 
                            ps.PlaylistId == blacklist.Id && ps.SongId == id);

                        if(alreadyBlacklisted != null)
                        _db.PlaylistSongs.Remove(alreadyBlacklisted);
                    }

                    // int favId = -1;
                    // trebamo kreirati playlistu FAVORITES za ovog usera
                    if (favPlaylist == null)
                    {
                        Playlist createFav = new Playlist();
                        //favId = await _db.Playlists.MaxAsync(p => p.Id) + 1;
                        //createFav.Id = favId;
                        createFav.Title = "FAVORITES";
                        createFav.MusicUserId = userId;

                        await _db.Playlists.AddAsync(createFav);

                        // nakon sto smo kreirali "FAVORITES" trebamo dodat pjesmu u nju jer
                        // ako playlista nije postojala ocito nit pjesma nije vec u njoj
                        // dodajemo par playlist - song u db i spremamo
                        Song favSong = await _db.Songs.FindAsync(id);
                        PlaylistSong newPS = new PlaylistSong
                        {
                            Playlist = createFav,
                            Song = favSong
                        };

                        await _db.PlaylistSongs.AddAsync(newPS);
                        await _db.SaveChangesAsync();

                        return Json( new 
                        { 
                            success = true, 
                            mark = "success",
                            message = "Playlist FAVORITES created and song added to FAVORITES",
                            color= "#28a745"
                        });

                    }

                    else
                    {
                        // znamo da playlista FAVORITES vec postoji pa trebamo provjeriti
                        // jel likeana pjesma vec u njoj

                        PlaylistSong targetPS = await _db.PlaylistSongs
                            .SingleOrDefaultAsync(
                            ps => ps.PlaylistId == favPlaylist.Id
                            && ps.SongId == id
                            );

                        if (targetPS == null)
                        {
                            // dodaj par playlist-song u db
                            Song newFavSong = await _db.Songs.FindAsync(id);
                            PlaylistSong newPS = new PlaylistSong
                            {
                                Playlist = favPlaylist,
                                Song = newFavSong
                            };

                            await _db.PlaylistSongs.AddAsync(newPS);
                            await _db.SaveChangesAsync();

                            return Json(new
                            {
                                success = true,
                                mark = "success",
                                message = "Song added to FAVORITES",
                                color = "#28a745"
                            });

                        }

                        else return Json(new {
                            success=true,
                            mark="info",
                            message="Song is already in your FAVORITES",
                            color= "#17a2b8"
                        });

                    }
                }

            }

                return Json(new { success = false, message = "Request failed (unauthorized user)" });
        }



        // DISLIKE -> POST
        [HttpPost]
        public async Task<IActionResult> Dislike(int? id)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (id == null) return NotFound();
                else
                {
                    
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    Playlist blacklist = await
                        _db.Playlists.SingleOrDefaultAsync(p =>
                        p.MusicUserId == userId && p.Title == "BLACKLIST");

                    Playlist favorites = await
                        _db.Playlists.SingleOrDefaultAsync(p => 
                        p.MusicUserId == userId && p.Title == "FAVORITES");

                    if(favorites != null)
                    {
                        PlaylistSong alreadyFavorites = await
                            _db.PlaylistSongs.SingleOrDefaultAsync(ps =>
                            ps.SongId == id && ps.PlaylistId == favorites.Id);

                        if (alreadyFavorites != null)
                            _db.PlaylistSongs.Remove(alreadyFavorites);
                    }


                    if (blacklist == null)
                    {
                        Playlist createBlacklist = new Playlist();
                        createBlacklist.Title = "BLACKLIST";
                        createBlacklist.MusicUserId = userId;

                        await _db.Playlists.AddAsync(createBlacklist);

                        Song blackSong = await _db.Songs.FindAsync(id);
                        PlaylistSong newPS = new PlaylistSong
                        {
                            Playlist = createBlacklist,
                            Song = blackSong
                        };

                        await _db.PlaylistSongs.AddAsync(newPS);
                        await _db.SaveChangesAsync();

                        return Json(new
                        {
                            success = true,
                            mark = "success",
                            message = "Playlist BLACKLIST created and song added to BLACKLIST",
                            color = "#dc3545"
                        });

                    }

                    else
                    {

                        PlaylistSong targetPS = await _db.PlaylistSongs
                            .SingleOrDefaultAsync(
                            ps => ps.PlaylistId == blacklist.Id
                            && ps.SongId == id
                            );

                        if (targetPS == null)
                        {
                            Song newBlackSong = await _db.Songs.FindAsync(id);
                            PlaylistSong newPS = new PlaylistSong
                            {
                                Playlist = blacklist,
                                Song = newBlackSong
                            };

                            await _db.PlaylistSongs.AddAsync(newPS);
                            await _db.SaveChangesAsync();

                            return Json(new
                            {
                                success = true,
                                mark = "success",
                                message = "Song added to BLACKLIST",
                                color = "#dc3545"
                            });

                        }

                        else return Json(new
                        {
                            success = true,
                            mark = "info",
                            message = "Song is already in your BLACKLIST",
                            color = "#17a2b8"
                        });

                    }
                }

            }

            return Json(new { success = false, message = "Request failed (unauthorized user)" });
        }



        #endregion

        // ADD NEW SONG -> POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Add(Song newSong)
        {
            bool tryCreateResult = Uri.TryCreate(newSong.Link, UriKind.Absolute, out Uri uriResult);
            if (ModelState.IsValid && tryCreateResult && uriResult != null) 
            {
                string capitalizeTitle = System.Threading.Thread.CurrentThread.CurrentCulture.
                    TextInfo.ToTitleCase(newSong.Title.ToLower());
                string capitalizeAuthor = System.Threading.Thread.CurrentThread.CurrentCulture.
                    TextInfo.ToTitleCase(newSong.Artist.ToLower());
                string capitalizeCategory = System.Threading.Thread.CurrentThread.CurrentCulture.
                    TextInfo.ToTitleCase(newSong.Category.ToLower());

                newSong.Title = capitalizeTitle;
                newSong.Artist = capitalizeAuthor;
                newSong.Category = capitalizeCategory;

                await _db.Songs.AddAsync(newSong);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else return View();
        }

        
    }
}
