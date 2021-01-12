using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Music.Data;
using Music.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
                        return Json(new { success = true, message = "Song successfuly deleted" });
                    }

                }
            }

            else return Json(new { success = false, message = "Request failed (unauthorized access)" });
        }

        // ADD NEW SONG -> POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Add(Song newSong)
        {
            bool tryCreateResult = Uri.TryCreate(newSong.Link, UriKind.Absolute, out Uri uriResult);
            if (ModelState.IsValid && tryCreateResult && uriResult != null) 
            {
                await _db.Songs.AddAsync(newSong);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else return View();
        }

        #endregion
    }
}
