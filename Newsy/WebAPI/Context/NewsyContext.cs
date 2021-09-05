using Domain;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace WebAPI.Context
{
    public class NewsyContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Article> Articles { get; set; }

        public NewsyContext([NotNull] DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Relations
            // User - Article (Many - One)
            modelBuilder.Entity<User>().HasMany(user => user.Articles).WithOne(article => article.Author).OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region User specific properties
            // Unique e-mail
            modelBuilder.Entity<User>().HasIndex(user => user.Email).IsUnique();
            #endregion
        }
    }
}
