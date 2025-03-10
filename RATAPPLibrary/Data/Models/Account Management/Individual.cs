namespace RATAPPLibrary.Data.Models
{
    public class Individual
    {
        public int Id { get; set; }  // Primary key
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string Phone { get; set; }
        public required string Email { get; set; }
        public string City { get; set; }
        public string State { get; set; }

    }
}