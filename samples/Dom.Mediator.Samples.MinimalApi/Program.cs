using Dom.Mediator;
using Dom.Mediator.Samples.MinimalApi.Infrastructure.Behaviours;
using Dom.Mediator.Samples.MinimalApi.Infrastructure.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediator(config =>
{
    //Register command/query handlers
    config.RegisterHandlers(typeof(Program).Assembly);

    // Register the request/response behaviours
    config.AddRequestResponseBehaviour(typeof(LoggingBehaviour<,>));
    config.AddCommandBehaviour(typeof(LoggingCommandBehaviour<>));
});

builder.Services.AddSingleton<TaskStore>();

var app = builder.Build();

//Enable Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.RegisterEndpoints();

app.Run();
