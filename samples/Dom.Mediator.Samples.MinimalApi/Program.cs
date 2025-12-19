using Dom.Mediator;
using Dom.Mediator.Samples.MinimalApi.Infrastructure.Behaviours;
using Dom.Mediator.Samples.MinimalApi.Infrastructure.Endpoints;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JSON serialization to support string-based enums
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddMediator(config =>
{
    //Register command/query handlers
    config.RegisterHandlers(typeof(Program).Assembly);

    // Register the request/response behaviours
    config.AddBehaviour(typeof(LoggingBehaviour<,>));  // For queries/requests
    config.AddBehaviour(typeof(LoggingBehaviour<>));   // For commands
});

builder.Services.AddSingleton<TaskStore>();

var app = builder.Build();

//Enable Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.RegisterEndpoints();

app.Run();
