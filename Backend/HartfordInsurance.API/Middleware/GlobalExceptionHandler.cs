using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace HartfordInsurance.API.Exceptions;

/// <summary>
/// Centralized exception handler — catches all unhandled exceptions from every layer
/// and returns a consistent JSON error response with the correct HTTP status code.
///
/// Exception → HTTP status mapping:
///   KeyNotFoundException       → 404 Not Found
///   ArgumentException          → 400 Bad Request
///   UnauthorizedAccessException→ 403 Forbidden
///   Any other Exception        → 500 Internal Server Error
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = context.TraceIdentifier;
        _logger.LogError(exception, "Unhandled exception (TraceId: {TraceId}): {Message}", traceId, exception.Message);

        var (statusCode, message) = exception switch
        {
            KeyNotFoundException            => (HttpStatusCode.NotFound,          exception.Message),
            ArgumentException               => (HttpStatusCode.BadRequest,        exception.Message),
            UnauthorizedAccessException     => (HttpStatusCode.Unauthorized,      "You do not have permission to access this resource."),
            TimeoutException                => (HttpStatusCode.RequestTimeout,     "The request timed out. Please try again later."),
            Microsoft.EntityFrameworkCore.DbUpdateException => (HttpStatusCode.Conflict, "A database error occurred while saving changes. Please check your data."),
            _                               => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again.")
        };

        var response = new
        {
            success    = false,
            statusCode = (int)statusCode,
            error      = message,
            traceId    = traceId
        };

        context.Response.StatusCode  = (int)statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response),
            cancellationToken);

        return true;
    }
}