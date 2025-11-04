using Microsoft.EntityFrameworkCore;

namespace MVC___MSSQL_Classifieds_Portal.Models
{
    public class ClassifieldsContext: DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Listing> Listings { get; set; }

        public ClassifieldsContext(DbContextOptions<ClassifieldsContext> option)
            : base(option)
        {

        }
    }
}
