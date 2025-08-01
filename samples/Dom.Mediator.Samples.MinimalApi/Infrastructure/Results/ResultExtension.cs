namespace Dom.Mediator.Samples.MinimalApi.Infrastructure.Results;

public static class ResultExtensions
{
    public static IResult ToIResult<T>(this Result<T> result) => new HttpResult<T>(result);
    public static IResult ToIResult(this Result result) => new HttpResult(result);
}