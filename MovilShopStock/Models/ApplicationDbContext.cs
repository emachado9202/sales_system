﻿using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.ModelConfiguration.Conventions;
using EFCache;
using Microsoft.AspNet.Identity.EntityFramework;
using MovilShopStock.Models.Catalog;
using MovilShopStock.Models.Handlers;

namespace MovilShopStock.Models
{
    [DbConfigurationType(typeof(Configuration))]
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

            modelBuilder.Entity<TransferBusinessProduct>()
                    .HasRequired(u => u.ProductTo)
                    .WithMany(ul => ul.TransferBusinessProducts)
                    .HasForeignKey(ul => ul.ProductTo_Id)
                    .WillCascadeOnDelete(false);

            modelBuilder.Entity<TransferBusinessProduct>()
                    .HasRequired(u => u.User)
                    .WithMany(ul => ul.TransferBusinessProducts)
                    .HasForeignKey(ul => ul.User_Id)
                    .WillCascadeOnDelete(false);

            modelBuilder.Entity<TransferMoneyUser>()
                    .HasRequired(u => u.UserTo)
                    .WithMany(ul => ul.TransferMoneyUsers)
                    .HasForeignKey(ul => ul.UserTo_Id)
                    .WillCascadeOnDelete(false);

            modelBuilder.Entity<TransferMoneyUser>()
                    .HasRequired(u => u.BusinessTo)
                    .WithMany(ul => ul.BusinessTos)
                    .HasForeignKey(ul => ul.BusinessTo_Id)
                    .WillCascadeOnDelete(false);
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<StockIn> StockIns { get; set; }
        public DbSet<StockOut> StockOuts { get; set; }
        public DbSet<Business> Businesses { get; set; }
        public DbSet<BusinessUser> BusinessUsers { get; set; }
        public DbSet<TransferBusinessProduct> TransferBusinessProducts { get; set; }
        public DbSet<TransferMoneyUser> TransferMoneyUsers { get; set; }
        public DbSet<ActivityLogType> ActivityLogTypes { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
    }

    public class Configuration : DbConfiguration
    {
        public Configuration()
        {
            var transactionHandler = new CacheTransactionHandler(CacheManager.Stock);

            AddInterceptor(transactionHandler);

            Loaded +=
              (sender, args) => args.ReplaceService<DbProviderServices>(
                (s, _) => new CachingProviderServices(s, transactionHandler));
        }
    }
}