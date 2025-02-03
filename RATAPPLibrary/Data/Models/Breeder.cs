using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RATAPPLibrary.Data.Models
{
    public class Breeder
    {
        public int Id { get; set; } // Assuming Id is an integer
        public int UserId { get; set; } // Assuming UserId is an integer
        public string LogoPath { get; set; } // LogoPath for breeder

        // Navigation Property for EF - LINQ one to one or one to many 
        public User User { get; set; }

        // Navigation property for many-to-many
        public ICollection<BreederClub> BreederClubs { get; set; }
    }
}
