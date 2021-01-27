using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    public class UsersController : Controller
    {

        private readonly AuthDbContext _authDb;
        private readonly ApplicationDbContext _appDb;
        private readonly UserManager<MusicUser> _userManager;

        public UsersController(AuthDbContext auth_db ,ApplicationDbContext app_db, UserManager<MusicUser> userManager)
        {
            _authDb = auth_db;
            _appDb = app_db;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            // get all other users and return them as a list
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<MusicUser> others = await _authDb.Users
                .Where(mu => mu.Id != myId).ToListAsync();

            // others list contains all the info on users which we don't need 
            // so instead of passing that to View we will create new ModelView class
            // which will contain just the neccessary info: 
            // id, first name, last name and email 
            List<UserViewModel> otherUsers = new List<UserViewModel>();
            others.ForEach(
                mUser => otherUsers.Add(
                    new UserViewModel(mUser.Id, mUser.FirstName, mUser.LastName, mUser.Email)
                    )
                );

            return View(otherUsers);
        }

        [Authorize]
        public async Task<IActionResult> Profile(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException($"'{nameof(id)}' cannot be null or empty", nameof(id));
            }
            // we need to get all playlists that 
            // 'belong' to user with Id = id
            MusicUser mu = await _authDb.Users.FindAsync(id);
            UserViewModel selectedUser = new UserViewModel(
                mu.Id, mu.FirstName, mu.LastName, mu.Email);

            List<Playlist> selectedUserPlaylists = await
                _appDb.Playlists
                .Where(p => p.MusicUserId == id && !p.isPrivate)
                .ToListAsync();

            selectedUser.Playlists = selectedUserPlaylists;

            return View(selectedUser);
        }
    }
}
