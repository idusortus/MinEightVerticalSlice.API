# Design Notes


### Storing complex objects as JSON, from https://steven-giesel.com/blogPost/5bf635cb-3533-4207-905f-81eb86512219

#### Entity Framework 8

```csharp
//What is possible since the latest version of Entity Framework (8) is to store lists of simple types as JSON. So, if you have a model like this:
public class BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; }
    public List<string> Tags { get; set; }
}

var post = new BlogPost
{
    Title = "Entity Framework 8",
    Tags = new List<string> { "Entity Framework", "EF8", "JSON" }
};
// Then the tags will be stored as: '["Entity Framework", "EF8", "JSON"]' in the database. Okay - that is very handy. But there are other news to Entity Framework 8: You can also store complex objects as owned types! So if you have a model like this:

public class BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; }
    public List<string> Tags { get; set; }
    public Author Author { get; set; }
}

public class Author
{
    public string Name { get; set; }
    public string Email { get; set; }
}
//Then in Entity Framework 8 you can configure this in your mapping as:

modelBuilder.Entity<BlogPost>()
    .OwnsOne(p => p.Author);
//Which creates column names like 'Author_Name' and 'Author_Email' in the database. Nice: Value objects are now first class citizens in Entity Framework.

//Complex objects as JSON
//But what if you want to store the author as JSON? This is also possible with Entity Framework. And not only since version 8, but since version 7 (well, technically since
//forever, as you can use ValueConverters):
modelBuilder.Entity<BlogPost>()
    .OwnsOne(p => p.Author, b => b.ToJson());