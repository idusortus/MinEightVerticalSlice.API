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
        // This looks more like JSON but it is rather hard to read
        modelBuilder.Entity<Article>()
            .Property(article=>article.Tags)
            .HasConversion( 
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null),
                new ValueComparer<ICollection<string>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => (ICollection<string>)c.ToList()));                    

        // this works but it adds it as CSV string
        // modelBuilder.Entity<Article>()
        //     .Property(article => article.Tags)
        //     .HasConversion(
        //         tags => string.Join(',', tags),
        //         tags => tags.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
        //     );
        // modelBuilder.Entity<Article>(builder => builder.OwnsOne(a => a.Tags, b => b.ToJson())); // Throws an error: Invalid token type: 'StartObject'.

        // // Complex objects as JSON
        // // But what if you want to store the author as JSON? This is also possible with Entity Framework. And not only since version 8,
        // // but since version 7 (well, technically since forever, as you can use ValueConverters):
        // modelBuilder.Entity<BlogPost>()
        //     .OwnsOne(p => p.Author, b => b.ToJson());

    }

    public DbSet<Article> Articles { get; set; }  

}
