using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RATAPPLibrary.Data.Models
{
    public class Club
    {
        public int Id { get; set; } // Assuming Id is an integer (common for primary keys)
        public string Name { get; set; } // Assuming Name is a string

        // Navigation property for many-to-many
        public ICollection<BreederClub> BreederClubs { get; set; }
    }
}