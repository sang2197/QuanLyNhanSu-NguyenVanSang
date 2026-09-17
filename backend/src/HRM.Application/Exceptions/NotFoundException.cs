namespace HRM.Application.Exceptions;

/// <summary>Maps to HTTP 404 + the Error schema in openapi.yaml.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
