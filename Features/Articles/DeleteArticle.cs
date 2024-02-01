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

public static class DeleteArticle
{
    /// <summary>
    /// Returns MediatR.Unit, which is a void type. This is because we don't need to return anything from this command.
    /// 204 is somethimes used as a delete OK response but it can cause problems with HATEOS/Docker
    /// http://blog.ploeh.dk/2013/04/30/rest-lesson-learned-avoid-204-responses/
    /// </summary>
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Article Id cannot be empty.");
        }
    }

    internal sealed class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly ApplicationDbContext _dbContext;
        public Handler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var article = await _dbContext.Articles
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (article is null)
            {
                return Result.Failure<Unit>(new Error(
                    "DeleteArticle.NotFound",
                    $"Article with Id {request.Id} not found"));
            }

            _dbContext.Articles.Remove(article);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}

public class DeleteArticleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/articles/{id:guid}", async (Guid id, ISender sender) =>
        {
            var command = new DeleteArticle.Command
            {
                Id = id
            };

            var result = await sender.Send(command);

            return (result.IsFailure)
                ? Results.NotFound(result.Error)
                : Results.Ok(result);
        });
    }
}
