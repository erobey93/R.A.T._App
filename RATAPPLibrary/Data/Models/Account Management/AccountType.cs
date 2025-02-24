namespace RATAPPLibrary.Data.Models
{
    public class AccountType
    {
        public virtual int Id { get; set; } // Primary key
        public virtual required string Name { get; set; } // The type of account (e.g., Admin, User, etc.)
    }
}
