using Carter;
using Carter.OpenApi;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Vertical.API.Database;
using Vertical.Features.Articles;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

try
{
    Log.Information("Starting ...");

    var assembly = typeof(Program).Assembly; // this is the assembly for this project, we use it to scan for types, used by MediatR and FluentValidation

    var builder = WebApplication.CreateBuilder(args);

    // Full setup of serilog. We read log settings from appsettings.json
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());
        
    {
        builder.Services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen()
            .AddProblemDetails();
    }

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));

    builder.Services.AddCarter(); // simplifies Mapping ENdpoints defined within Vertical Slice Features

    builder.Services.AddValidatorsFromAssembly(assembly);

    var app = builder.Build();
    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        // app.ApplyMigrations();
    }

    // add the Feature endpoints
    // CreateArticle.MapEndpoint(app); // this is what it would look like without Carter - would need one of these for each feature
    app.MapCarter(); // this is what it looks like with Carter - one line to rule them all, it will scan for all public class Endpoint : ICarterModule. Can I use map.group with carter?
    app.UseHttpsRedirection();
    app.Run();
}
catch (Exception t)
{
    Log.Fatal(t, "Application bombed.");
}
finally
{
    Log.CloseAndFlush();
}