using Carter;
using FluentValidation;
using MediatR;
using Vertical.API.Contracts;
using Vertical.API.Database;
using Vertical.API.Shared;

namespace Vertical.Features.Articles;

public static class ArticleStub
{
    public class Command : IRequest<Result<Guid>>
    {
        public Guid Id { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    internal sealed class Handler : IRequestHandler<Command, Result<Guid>>
    {
        private readonly IValidator<Command> _validator;
        private readonly ApplicationDbContext _dbContext;
        public Handler(IValidator<Command> validator, ApplicationDbContext dbContext)
        {
            _validator = validator;
            _dbContext = dbContext;
        }

        public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<Guid>(new Error(
                     "ArticleStub.Validation",
                     validationResult.ToString()));
            }
            
            // do stuff

            return Guid.NewGuid();
            
        }
    }
}

public class StubEndpoint :ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // app.MapPost("/api/articles", async (StubRequest request, ISender sender) =>
        // {
        //     var command = new ArticleStub.Command
        //     {
        //         Id = request.Id,
        //         ///more
        //     };
        //     var result = await sender.Send(command);
        //     if (result.IsFailure)
        //     {
        //         return Results.BadRequest(result.Error);
        //     }
        //     return Results.Ok(result.Value);
        // });
    }
}