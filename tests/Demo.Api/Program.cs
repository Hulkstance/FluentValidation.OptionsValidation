using Demo.Api;
using FluentValidation;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton);

builder.Services
    .AddOptions<ExampleOptions>()
    .BindConfiguration(ExampleOptions.SectionName)
    .ValidateFluentValidation()
    .ValidateOnStart();

var app = builder.Build();

app.MapGet("/", (IOptions<ExampleOptions> options) => options.Value);

app.Run();
