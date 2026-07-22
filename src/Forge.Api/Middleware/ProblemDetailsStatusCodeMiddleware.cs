namespace Forge.Api.Middleware;

public sealed class ProblemDetailsStatusCodeMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        if (context.Response.HasStarted || context.Response.StatusCode != StatusCodes.Status404NotFound)
        {
            return;
        }

        await ProblemDetailsExceptionMiddleware.WriteProblemDetailsAsync(
            context,
            StatusCodes.Status404NotFound,
            "Resource not found",
            "The requested resource was not found.");
    }
}
