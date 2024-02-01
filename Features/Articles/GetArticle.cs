using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Vertical.API.Contracts;
using Vertical.API.Database;
using Vertical.API.Entities;
using Vertical.API.Shared;

namespace Vertical.Features.Articles;

public static class GetArticle
{
    // Only need the Id here, the Response will have the rest of the fields
    public class Query : IRequest<Result<ArticleResponse>>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, Result<ArticleResponse>>
    {
        private readonly ApplicationDbContext _dbContext;
        public Handler( ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<ArticleResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            // What happens on DB Timeout? What happens on DB Error?
            var articleResponse = await _dbContext.Articles
                .Where(x => x.Id == request.Id)
                .Select(article => new ArticleResponse
                {
                    Id = article.Id,
                    Title = article.Title,
                    Content = article.Content,
                    Tags = article.Tags,
                    CreatedOnUtc = article.CreatedOnUtc,
                    PublishedOnUtc = article.PublishedOnUtc
                })
                .FirstOrDefaultAsync(cancellationToken);

            if(articleResponse is null)
            {
                return Result.Failure<ArticleResponse>(new Error(
                    "GetArticle.Null",
                    $"Article with Id {request.Id} not found"));
            }

            return articleResponse;
        }
    }
}

public class GetArticleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/articles/{id:guid}", async (Guid id, ISender sender) =>
        {
            var query = new GetArticle.Query
            {
                Id = id
            };

            var result = await sender.Send(query);
            
            return (result.IsFailure)
                ? Results.NotFound(result.Error)
                : TypedResults.Ok(result);

        });
    }
} 