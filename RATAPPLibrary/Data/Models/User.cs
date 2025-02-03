using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RATAPPLibrary.Data.Models
{
    public class User
    {
        public int Id { get; set; } // Primary key

        // Foreign key and navigation property for credentials
        public int CredentialsId { get; set; }
        public Credentials Credentials { get; set; }

        // Foreign key and navigation property for account type
        public int AccountTypeId { get; set; }
        public AccountType AccountType { get; set; }

        // Foreign key and navigation property for individual
        public int IndividualId { get; set; }
        public Individual Individual { get; set; }

        public string ImagePath { get; set; } // ImagePath for users or pets
    }
}
