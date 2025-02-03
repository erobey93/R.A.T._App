using RATAPPLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RATAPPLibrary.Data.Models
{
    public class Credentials
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // Password will be set externally
    }
}
