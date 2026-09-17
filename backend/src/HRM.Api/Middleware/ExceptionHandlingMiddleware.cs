using System.Net;
using System.Text.Json;
using HRM.Api.DTOs.Responses;
using HRM.Application.Exceptions;

namespace HRM.Api.Middleware;

/// <summary>
/// Maps Application-layer exceptions to the Error schema + status codes
/// declared in openapi.yaml (400/404/409), instead of letting them surface
/// as unhandled 500s.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (statusCode, error) = ex switch
            {
                NotFoundException notFound => (HttpStatusCode.NotFound, new ErrorResponse { Code = "NOT_FOUND", Message = notFound.Message }),
                ConflictException conflict => (HttpStatusCode.Conflict, new ErrorResponse { Code = conflict.Code, Message = conflict.Message }),
                ValidationException validation => (HttpStatusCode.BadRequest, new ErrorResponse { Code = "VALIDATION_ERROR", Message = validation.Message }),
                _ => (HttpStatusCode.InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An unexpected error occurred." })
            };

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(ex, "Unhandled exception");
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(error));
        }
    }
}
