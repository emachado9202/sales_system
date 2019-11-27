using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using MovilShopStock.Models.Catalog;

namespace MovilShopStock.Models
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Migrations.Configuration>("DefaultConnection"));
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<IdentityUserLogin>().HasKey<string>(l => l.UserId);
            modelBuilder.Entity<IdentityRole>().HasKey<string>(r => r.Id);
            modelBuilder.Entity<IdentityUserRole>().HasKey(r => new { r.RoleId, r.UserId });

            modelBuilder.Entity<User>()
                    .HasMany(u => u.StockIns)
                    .WithRequired(ul => ul.User)
                    .HasForeignKey(ul => ul.User_Id)
                    .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                    .HasMany(u => u.StockOuts)
                    .WithRequired(ul => ul.User)
                    .HasForeignKey(ul => ul.User_Id)
                    .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                    .HasMany(u => u.StockOuts)
                    .WithOptional(ul => ul.Receiver)
                    .HasForeignKey(ul => ul.Receiver_Id)
                    .WillCascadeOnDelete(false);

            modelBuilder.Entity<StockOut>()
                    .HasRequired(u => u.Product)
                    .WithMany(ul => ul.StockOuts)
                    .HasForeignKey(ul => ul.Product_Id)
                    .WillCascadeOnDelete(false);
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<StockIn> StockIns { get; set; }
        public DbSet<StockOut> StockOuts { get; set; }
    }
}