namespace HRM.Application.Exceptions;

/// <summary>Maps to HTTP 409 + the Error schema in openapi.yaml.</summary>
public class ConflictException : Exception
{
    public string Code { get; }

    public ConflictException(string message, string code = "CONFLICT") : base(message)
    {
        Code = code;
    }
}
