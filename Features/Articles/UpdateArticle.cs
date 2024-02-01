using Carter;
using FluentValidation;
using MediatR;
using Vertical.API.Contracts;
using Vertical.API.Database;
using Vertical.API.Shared;

namespace Vertical.Features.Articles;

public static class UpdateArticle
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x=>x.Id).NotEmpty().WithMessage("Article Id cannot be empty.");
            RuleFor(x => x.Title).NotEmpty();
            RuleFor(x => x.Content).NotEmpty();
        }
    }

    internal sealed class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IValidator<Command> _validator;

        public Handler(ApplicationDbContext dbContext, IValidator<Command> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if(!validationResult.IsValid)
            {
                return Result.Failure<Unit>(new Error(
                    "UpdateArticle.Validation",
                    validationResult.ToString()
                ));
            }
            var article = await _dbContext.Articles.FindAsync(request.Id);
            if (article is null)
                return Result.Failure<Unit>(new Error(
                    "UpdateArticle.NotFound",
                    $"Article with Id {request.Id} not found."));

            article.Title = request.Title;
            article.Tags = request.Tags;
            article.Content = request.Content;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}

public class UpdateArticleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/articles", async (UpdateArticleRequest request, ISender sender) =>
        {
            var command = new UpdateArticle.Command
            {
                Id = request.Id,
                Content = request.Content,
                Title = request.Title,
                Tags = request.Tags
            };

            var result = await sender.Send(command);
            return (result.IsFailure)
                ? Results.BadRequest(result.Error)
                : TypedResults.Ok(result);
        });
    }
}