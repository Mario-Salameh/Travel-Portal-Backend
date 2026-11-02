public class ExceptionEndpointFilter(ILogger<ExceptionEndpointFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Wraps the context into main try-catch form
        try { return await next(context); }

        // If there as an application error (an already configured error by developer) then put's its message in the output
        catch (ApplicationException e)
        {
            var request = context.HttpContext.Request;
            logger.LogWarning(e, "Request {Method} {Path} returned 400. TraceId: {TraceId}",
                request.Method,
                request.Path,
                context.HttpContext.TraceIdentifier);

            return Results.BadRequest(e.Message);
        }

        // If it's something else, log it in the system and telling the user to contact System Administrators immedietly
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception in endpoint");
            return Results.BadRequest("There was a problem performing the requested operation. Please contact the system administrator.");
        }
    }
}
