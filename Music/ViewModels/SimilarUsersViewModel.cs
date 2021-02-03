using Music.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Music.ViewModels
{
    public class SimilarUsersViewModel
    {
        public UserViewModel UserData { get; set; }
        public int PlaylistId { get; set; }

        public List<Song> SongsInCommon { get; set; }

        public SimilarUsersViewModel() {}
        public SimilarUsersViewModel(int pId, UserViewModel uvm, List<Song> songList)
        {
            PlaylistId = pId;
            UserData = uvm;
            SongsInCommon = songList;
        }
    }
}
