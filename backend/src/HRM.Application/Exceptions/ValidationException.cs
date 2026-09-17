namespace HRM.Application.Exceptions;

/// <summary>Maps to HTTP 400 + the Error schema in openapi.yaml.</summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
