using DishDashRESTAPIDatabase.Models;
using Microsoft.EntityFrameworkCore;

namespace DishDashRESTAPIDatabase
{
    public class DishDashDb : DbContext
    {
        public DishDashDb(DbContextOptions<DishDashDb> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Recipe> Recipes => Set<Recipe>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure many-to-many relationship between Users and Recipes for favorites
            modelBuilder.Entity<User>()
                .HasMany(u => u.FavoriteRecipes)
                .WithMany()
                .UsingEntity(j => j.ToTable("UserFavoriteRecipes"));
        }
    }
}
