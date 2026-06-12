namespace ToursAndTravelsManagement.Middlewares;

public class GlobalErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.Redirect("/Home/Error");
        }

        if (context.Response.StatusCode >= 400 && context.Response.StatusCode < 500)
        {
            context.Response.Redirect("/Home/Error");
        }
    }
}
