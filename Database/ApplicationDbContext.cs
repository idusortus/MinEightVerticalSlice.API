using Microsoft.EntityFrameworkCore;
using Vertical.API.Entities;

namespace Vertical.API.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //     modelBuilder.Entity<Article>(builder => builder.OwnsOne(a => a.Tags, tagsBuilder => tagsBuilder.ToJson())); // Not sure what Tagsbuilder is

        // Complex objects as JSON
        // But what if you want to store the author as JSON? This is also possible with Entity Framework. And not only since version 8,
        // but since version 7 (well, technically since forever, as you can use ValueConverters):
        // modelBuilder.Entity<BlogPost>()
        //     .OwnsOne(p => p.Author, b => b.ToJson());

    // }

    public DbSet<Article> Articles { get; set; }  

}
