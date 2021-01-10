using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Music.Areas.Identity.Data;
using Music.Data;
using Music.Models;
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
    }
}
