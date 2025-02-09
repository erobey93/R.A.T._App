using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RATAPP.Classes
{
    public class AnimalSelectedEventArgs : EventArgs
    {
        public string AnimalName { get; }
        public string AnimalID { get; }
        public string Species { get; }
        public string Sex { get; }
        public string DOB { get; }
        public string Genotype { get; }

        public AnimalSelectedEventArgs(string animalName, string animalID, string species, string sex, string dob, string genotype)
        {
            AnimalName = animalName;
            AnimalID = animalID;
            Species = species;
            Sex = sex;
            DOB = dob;
            Genotype = genotype;
        }
    }
}
