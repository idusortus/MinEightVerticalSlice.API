using System.ComponentModel.DataAnnotations;
using Carter;
using Carter.OpenApi;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Vertical.API.Contracts;
using Vertical.API.Database;
using Vertical.API.Entities;
using Vertical.API.Shared;

namespace Vertical.Features.Articles;

/// <summary>
/// Contains the Command, the Command Handler and the Endpoint for a feature, in this case creating an article.
/// 
/// The Command is the data that is passed to the Command Handler, which is the logic that handles the command.
/// The Endpoint is the HTTP endpoint that is exposed to the outside world, in this case a POST to /api/articles.
/// 
/// 
/// Vertical Slices are tightly coupled by design, because they are related. 
/// Vertical Slices should NOT be tightly coupled to other Vertical Slices.
/// </summary>
public static class CreateArticle
{
    public class Command : IRequest<Result<Guid>>
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Title).NotEmpty();
            RuleFor(x => x.Content).NotEmpty();
        }
    }   

    /// <summary>
    /// The Command Handler is the logic that handles the Command. It is where we do things like save to the database.
    /// This class is internal and sealed because it is only used by this feature. It is not used by any other feature.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Command, Result<Guid>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IValidator<Command> _validator;

        public Handler(ApplicationDbContext dbContext, IValidator<Command> validator)
        {
            _validator = validator;
            _dbContext = dbContext;
        }

        public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                // With the use of Result, Error, ResultT, we can return a Result with an Error. 
                // This is a better approach than throwing an exception.
                return Result.Failure<Guid>(new Error(
                    "CreateArticle.Validation",
                    validationResult.ToString()
                ));
            }

            var article = new Article
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Content = request.Content,
                Tags = request.Tags,
                CreatedOnUtc = DateTime.UtcNow
            };

            _dbContext.Articles.Add(article);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return article.Id;
        }
    }
}
public class CreateArticleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // ISender is a MediatR interface that allows us to send a Command to a Command Handler.
        app.MapPost("api/articles", async (CreateArticleRequest request, ISender sender) =>
        {
            // Could use Mapster, or another automapper, to map from CreateArticleRequest to CreateArticle.Command
            // Ex: var command = request.Adapt<CreateArticle.Command>();
            var command = new CreateArticle.Command
            {
                Title = request.Title,
                Content = request.Content,
                Tags = request.Tags
            };

            var result = await sender.Send(command);

            return (result.IsFailure)
                ? Results.BadRequest(result.Error)
                : TypedResults.Created(result.Value.ToString());
        });
    }
}