using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Music.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the MusicUser class
    public class MusicUser : IdentityUser
    {
        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        public string FirstName { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        public string LastName { get; set; }
    }

    // current users:
    // 1: stvar.principa@gmail.com - 123456*
    // 2: abx@example.com - 12345678*
}
