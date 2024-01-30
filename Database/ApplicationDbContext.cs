using Microsoft.EntityFrameworkCore;
using Vertical.API.Entities;

namespace Vertical.API.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// OnModelCreating is where we configure the database. We use it to configure things like relationships and indexes.
    /// We also use it to configure things like Owned Types. Owned Types are types that are owned by another type. In this case, Article.Tags is an Owned Type.
    /// builder.OwnsOne is a Fluent API method that configures an Owned Type. It takes two parameters, the first is the property that is the Owned Type, the second is a lambda that configures the Owned Type.
    /// ToJson is an extension method that ???? that converts a List<string> to a JSON string. We use it to convert Article.Tags to a JSON string so that it can be stored in the database.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>(builder =>
            builder.OwnsOne(a => a.Tags, tagsBuilder => tagsBuilder.ToJson()));
    }

    public DbSet<Article> Articles { get; set; }
     

}
