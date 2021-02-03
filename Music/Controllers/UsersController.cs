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
    public class UsersController : Controller
    {

        private readonly AuthDbContext _authDb;
        private readonly ApplicationDbContext _appDb;
        private readonly UserManager<MusicUser> _userManager;

        public UsersController(AuthDbContext auth_db, ApplicationDbContext app_db, UserManager<MusicUser> userManager)
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

        public async Task<IActionResult> FindConnections()
        {
            // we want all users that have in their FAVORITES at least 
            // one same song that we have in our FAVORITES
            // so we wanna return that users and also every song that are 
            // in both theirs and ours FAVORITES

            string myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // prvo dohvacamo svoje favorites
            List<Song> myFavorites = await _appDb
                .Songs
                .FromSqlRaw("select mySong.* from dbo.Songs as mySong " +
                "where mySong.Id in  (" +
                "select playlistsong.SongId from dbo.PlaylistSongs as playlistsong " +
                "join dbo.Playlists as playlist " +
                "on playlistsong.PlaylistId = playlist.Id " +
                "where playlist.Title = 'FAVORITES' " +
                "and playlist.MusicUserId = @me)",
                new SqlParameter("@me", myId))
                .ToListAsync();

            // ocistimo listu tako da sadrzi samo ID pjesama
            List<int> myFavoritesIds = new List<int>();
            myFavorites.ForEach(favoriteSong => myFavoritesIds.Add(favoriteSong.Id));

            // trebamo sve pjesme  
            // iz playlista zvanih FAVORITES
            // od korisnika koji nemaju myId
            // i da sadrze bar jednu od pjesama sa liste

            List<PlaylistSong> playlistSongs = await _appDb
                         .PlaylistSongs
                         .Include(ps => ps.Song)
                         .Include(ps => ps.Playlist)
                         .Where(ps => myFavoritesIds.Contains(ps.SongId) &&
                                ps.Playlist.Title == "FAVORITES" &&
                                ps.Playlist.MusicUserId != myId)
                         .ToListAsync();

            // key = playlistID, value = songs in others' FAVORITES
            //Dictionary<int, List<Song>> othersDict = new Dictionary<int, List<Song>>();
            //playlistSongs.ForEach(ps =>
            //{
            //    if (othersDict.ContainsKey(ps.PlaylistId))
            //        othersDict[ps.PlaylistId].Add(ps.Song);


            //    else
            //        othersDict[ps.PlaylistId] = new List<Song>() { ps.Song };

            //});

            //Dictionary<string, Dictionary<int, List<Song>>> othersDict = new Dictionary<string, Dictionary<int, List<Song>>>();
            //playlistSongs.ForEach(ps =>
            //{
            //if (othersDict.ContainsKey(ps.Playlist.MusicUserId))
            //{
            //    if (othersDict[ps.Playlist.MusicUserId].ContainsKey(ps.PlaylistId))
            //        othersDict[ps.Playlist.MusicUserId][ps.PlaylistId].Add(ps.Song);

            //    else othersDict[ps.Playlist.MusicUserId].Add(ps.PlaylistId,
            //        new List<Song>() { ps.Song });
            //}

            //else
            //{
            //        Dictionary<int, List<Song>> subDict = new Dictionary<int, List<Song>>();
            //        subDict.Add(ps.PlaylistId, new List<Song>() { ps.Song });

            //        othersDict.Add(ps.Playlist.MusicUserId, subDict);
            //    }

            //});


            List<string> userIds = new List<string>();
            playlistSongs.ForEach(ps =>
            {
                if (!userIds.Contains(ps.Playlist.MusicUserId))
                    userIds.Add(ps.Playlist.MusicUserId);
            });

            List<MusicUser> otherUsers = await _authDb
                .Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            

            // zelimo Dictionary<key=PlaylistID, value=Tuple(UserData, List<Song>)>
            Dictionary<int, (UserViewModel, List<Song>)> othersDict = new Dictionary<int, (UserViewModel, List<Song>)>();


            playlistSongs.ForEach(ps =>
            {
                // samo treba dodati pjesmu u listu jer je UserData vec postavljen
                if (othersDict.ContainsKey(ps.PlaylistId))
                    othersDict[ps.PlaylistId].Item2.Add(ps.Song);

                // inace:
                // 1) treba dohvatiti podatke o useru
                // 2) ostaviti samo id, fname, lname, email
                else
                {
                    MusicUser otherUser = otherUsers.Find(u => u.Id == ps.Playlist.MusicUserId);
                    othersDict.Add(ps.PlaylistId, (
                        new UserViewModel(otherUser.Id, otherUser.FirstName, otherUser.LastName, otherUser.Email),
                        new List<Song>() { ps.Song }
                        ));
                }

            });


            //playlistSongs.ForEach(async(ps) =>
            //{
            //    // samo treba dodati pjesmu u listu jer je UserData vec postavljen
            //    if(othersDict.ContainsKey(ps.PlaylistId))
            //        othersDict[ps.PlaylistId].Item2.Add(ps.Song);

            //    // inace:
            //    // 1) treba dohvatiti podatke o useru
            //    // 2) ostaviti samo id, fname, lname, email
            //    else
            //    {
            //        MusicUser otherUser = await _authDb.Users.FindAsync(ps.Playlist.MusicUserId);
            //        othersDict.Add(ps.PlaylistId, (
            //            new UserViewModel(otherUser.Id, otherUser.FirstName, otherUser.LastName, otherUser.Email),
            //            new List<Song>() { ps.Song }
            //            ));
            //    }

            //});

            List<SimilarUsersViewModel> similarUsers = new List<SimilarUsersViewModel>();
            foreach (KeyValuePair<int, (UserViewModel, List<Song>)> kvp in othersDict)
                similarUsers.Add(
                    new SimilarUsersViewModel(kvp.Key, kvp.Value.Item1, kvp.Value.Item2));

            return View(similarUsers);
        }
    }
}
