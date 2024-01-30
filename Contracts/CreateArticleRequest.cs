namespace Vertical.API.Contracts
{
    /// <summary>
    /// While nearly identical to the <see cref="Article"/> entity, this class is used to
    /// decouple the API from the database.
    /// This allows the sibling objects to evolve independently, the API will use the Request
    /// and the database will use the Command.
    /// Another approach would be to place CreateArticleRequest in the  CreateArticle class,
    /// but this could incurr code duplication. Arguably we're duplicating code here, but
    /// it's a small amount.
    /// </summary>
    public class CreateArticleRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }
}