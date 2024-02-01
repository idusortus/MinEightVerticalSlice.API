using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vertical.API.Contracts;
using Vertical.API.Database;
using Vertical.API.Entities;
using Vertical.API.Shared;

namespace Vertical.API.Features.Articles;

public static class GetAllArticles
{
    public class Query : IRequest<Result<AllArticleResponses>>
    {
        // nothing is required here afaik
    }

    public sealed class Handler : IRequestHandler<Query, Result<AllArticleResponses>>
    {
        private readonly ApplicationDbContext _dbContext;

        public Handler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<AllArticleResponses>> Handle(Query request, CancellationToken cancellationToken)
        {
            var articles = await _dbContext.Articles.ToListAsync();
            Console.WriteLine("Nothing");
            IEnumerable<ArticleResponse> allArticles = articles
                .Select(article => new ArticleResponse
                {
                    Id = article.Id,
                    Title = article.Title,
                    Content = article.Content,
                    Tags = article.Tags,
                    CreatedOnUtc = article.CreatedOnUtc,
                    PublishedOnUtc = article.PublishedOnUtc
                })
                .ToList();

            return new AllArticleResponses { Responses = allArticles };
        }
    }
}

public class GetAllArticlesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/articles", async (ISender sender) =>
        {
            var query = new GetAllArticles.Query();
            var result = await sender.Send(query);
            return (result.IsFailure)
                ? Results.BadRequest(result.Error)
                : TypedResults.Ok(result); 
        });
    }
}