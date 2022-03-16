using Microsoft.EntityFrameworkCore;

namespace LoginRegistration.Models
{
    public class LoginRegistrationContext : DbContext
    {
        public LoginRegistrationContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users {get;set;}
    }
}