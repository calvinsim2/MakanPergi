using MakanPergi.Entities;
using MakanPergi.Entities.Views;
using Microsoft.EntityFrameworkCore;

namespace MakanPergi.Data
{
    public class MakanPergiDbContext : DbContext, IDbContext
    {
        public MakanPergiDbContext(DbContextOptions<MakanPergiDbContext> options) : base(options) { }

        public DbSet<Food> Food { get; set; }
        public DbSet<Cuisine> Cuisine { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Shop> Shop { get; set; }
        public DbSet<VW_FoodFullView> FoodFullView { get; set; }
        public DbContext Instance => this;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Food>()
                .HasMany(f => f.Categories)
                .WithMany(c => c.Foods)
                .UsingEntity<FoodCategory>(cf => cf.ToTable("FoodCategory"));

            modelBuilder.Entity<Food>()
                .HasMany(f => f.ShopsOffered)
                .WithMany(c => c.Foods)
                .UsingEntity<FoodFromShop>(cf => cf.ToTable("FoodFromShop"));

            modelBuilder
                .Entity<VW_FoodFullView>()
                .HasNoKey() // Since views don't have primary keys
                .ToView("FoodFullView");
        }
    }
}
