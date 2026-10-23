namespace DzentoPOS2.Shared.Results;

public class Result
{
    public bool Success { get; protected set; }

    public string? Message { get; protected set; }

    public static Result Succeeded(string? message = null)
    {
        return new Result
        {
            Success = true,
            Message = message
        };
    }

    public static Result Failed(string message)
    {
        return new Result
        {
            Success = false,
            Message = message
        };
    }
}