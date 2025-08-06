
using Dom.Mediator;

public record ErrorDetail(string field, string message);

public class Error
{ 
    public string Code { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    public List<ErrorDetail> Details { get; } = new List<ErrorDetail>();
    public string Type { get; init; }

    public Error(string code, string description, string type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public void AddDetail(string errorCode, string errorMessage)
    {
        Details.Add(new ErrorDetail(errorCode, errorMessage));
    }
}
