using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Vertical.API.Entities;

namespace Vertical.API.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // This works for now, but it is rather hard to read and I added mystery Bangs to make it work...
        // TODO: Reevaluate this implementation
        // https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions?tabs=data-annotations#built-in-converters
        modelBuilder.Entity<Article>()
            .Property(article=>article.Tags)
            .HasConversion( 
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!)!,
                new ValueComparer<ICollection<string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => (ICollection<string>)c.ToList()));                    

        // this is functional but it adds it to the DB as CSV string
        // modelBuilder.Entity<Article>()
        //     .Property(article => article.Tags)
        //     .HasConversion(
        //         tags => string.Join(',', tags),
        //         tags => tags.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
        //     );

        // Throws an error: Invalid token type: 'StartObject'.
        // modelBuilder.Entity<Article>(builder => builder.OwnsOne(a => a.Tags, b => b.ToJson())); 

        // // Complex objects as JSON
        // // But what if you want to store the author as JSON? This is also possible with Entity Framework. 
        // // https://steven-giesel.com/blogPost/5bf635cb-3533-4207-905f-81eb86512219
        // modelBuilder.Entity<BlogPost>()
        //     .OwnsOne(p => p.Author, b => b.ToJson());

    }

    public DbSet<Article> Articles { get; set; }  

}
