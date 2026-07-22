using System.Text.Json;
using Forge.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Forge.Api.Tests.Middleware;

public class ProblemDetailsMiddlewareTests
{
    [Fact]
    public async Task StatusCodeMiddleware_WritesProblemDetails_WhenResourceIsNotFound()
    {
        var context = CreateHttpContext();
        var middleware = new ProblemDetailsStatusCodeMiddleware(next =>
        {
            next.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        var problemDetails = await ReadProblemDetailsAsync(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        Assert.Equal("Resource not found", problemDetails.Title);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
    }

    [Fact]
    public async Task ExceptionMiddleware_WritesValidationProblemDetails_WhenArgumentExceptionIsThrown()
    {
        var context = CreateHttpContext();
        var middleware = new ProblemDetailsExceptionMiddleware(
            _ => throw new ArgumentException("Workout name is required."),
            NullLogger<ProblemDetailsExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        var problemDetails = await ReadProblemDetailsAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("Validation error", problemDetails.Title);
        Assert.Equal("Workout name is required.", problemDetails.Detail);
    }

    [Fact]
    public async Task ExceptionMiddleware_WritesConflictProblemDetails_WhenDomainConflictIsThrown()
    {
        var context = CreateHttpContext();
        var middleware = new ProblemDetailsExceptionMiddleware(
            _ => throw new InvalidOperationException("Exercise cannot be deleted because it is linked to a workout."),
            NullLogger<ProblemDetailsExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        var problemDetails = await ReadProblemDetailsAsync(context);

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        Assert.Equal("Domain conflict", problemDetails.Title);
        Assert.Equal("Exercise cannot be deleted because it is linked to a workout.", problemDetails.Detail);
    }

    [Fact]
    public async Task ExceptionMiddleware_WritesUnexpectedProblemDetails_WhenUnexpectedExceptionIsThrown()
    {
        var context = CreateHttpContext();
        var middleware = new ProblemDetailsExceptionMiddleware(
            _ => throw new Exception("Database is offline."),
            NullLogger<ProblemDetailsExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        var problemDetails = await ReadProblemDetailsAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("Unexpected error", problemDetails.Title);
        Assert.Equal("An unexpected error occurred.", problemDetails.Detail);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        return new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            },
            Request =
            {
                Path = "/api/test"
            }
        };
    }

    private static async Task<ProblemDetails> ReadProblemDetailsAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            context.Response.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return problemDetails ?? throw new InvalidOperationException("ProblemDetails response was empty.");
    }
}
