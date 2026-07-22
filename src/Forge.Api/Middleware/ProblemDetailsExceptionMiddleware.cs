using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Middleware;

public sealed class ProblemDetailsExceptionMiddleware(
    RequestDelegate next,
    ILogger<ProblemDetailsExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            var (statusCode, title) = exception switch
            {
                ArgumentException => (StatusCodes.Status400BadRequest, "Validation error"),
                InvalidOperationException => (StatusCodes.Status409Conflict, "Domain conflict"),
                _ => (StatusCodes.Status500InternalServerError, "Unexpected error")
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Unhandled exception while processing {Path}.", context.Request.Path);
            }

            var detail = exception is ArgumentException or InvalidOperationException
                ? exception.Message
                : "An unexpected error occurred.";

            await WriteProblemDetailsAsync(context, statusCode, title, detail);
        }
    }

    internal static async Task WriteProblemDetailsAsync(
        HttpContext context,
        int statusCode,
        string title,
        string? detail = null)
    {
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            problemDetails,
            ProblemDetailsJsonContext.Default.ProblemDetails,
            context.RequestAborted);
    }
}
