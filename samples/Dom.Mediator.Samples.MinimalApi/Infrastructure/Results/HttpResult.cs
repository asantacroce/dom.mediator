namespace Dom.Mediator.Samples.MinimalApi.Infrastructure.Results;

public class HttpResult : IResult
{
    private readonly Result _result;

    public HttpResult(Result result) => _result = result;

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = GetStatusCode(_result.Error);

        object payload = _result.IsSuccess
            ? new { }
            : new { _result.Error };

        await httpContext.Response.WriteAsJsonAsync(payload);
    }

    private static int GetStatusCode(Error? error)
    {
        if (error == null) return StatusCodes.Status200OK;

        return error.Type switch
        {
            "Internal" => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest
        };
    }
}

public class HttpResult<T> : IResult
{
    private readonly Result<T> _result;

    public HttpResult(Result<T> result) => _result = result;

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = GetStatusCode(_result.Error);

        object payload = _result.IsSuccess
            ? _result.Value!
            : new { _result.Error };

        await httpContext.Response.WriteAsJsonAsync(payload);
    }

    private static int GetStatusCode(Error? error)
    {
        if (error == null) return StatusCodes.Status200OK;

        return error.Type switch
        {
            "Internal" => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest
        };
    }
}
