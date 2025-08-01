using Dom.Mediator.Abstractions;

namespace Dom.Mediator.Samples.MinimalApi;

public static class Endpoints
{
    public static void RegisterEndpoints(this WebApplication app)
    {
        app.MapPost("/tasks", async (CreateTaskCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.ToIResult();
        });

        app.MapGet("/tasks", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAllTasksQuery());
            return result.ToIResult();
        });
    }
}
