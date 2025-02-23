using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RATAPPLibrary.Data.Models.Breeding
{
    public class BreederClub
    {
        public int BreederId { get; set; }
        public Breeder? Breeder { get; set; } // Navigation property to Breeder

        public int ClubId { get; set; }
        public virtual Club? Club { get; set; } // Navigation property to Club
    }
}
