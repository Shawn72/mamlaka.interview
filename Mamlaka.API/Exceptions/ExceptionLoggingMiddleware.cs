using System.Diagnostics;

namespace Mamlaka.API.Exceptions;
public class ExceptionLoggingMiddleware
{
    // Name of the Response Header, Custom Headers starts with "x-"  
    private const string RESPONSE_HEADER_RESPONSE_TIME = "x-response-time-ms";
    // Handle to the next Middleware in the pipeline  
    private readonly RequestDelegate _next;
    public ExceptionLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        // Start the Timer using Stopwatch  
        Stopwatch watch = new();
        watch.Start();
        context.Response.OnStarting(() =>
        {
            // Stop the timer information and calculate the time   
            watch.Stop();

            // Add the Response time information in the Response headers.   
            context.Response.Headers[RESPONSE_HEADER_RESPONSE_TIME] = watch.ElapsedMilliseconds.ToString();

            return Task.CompletedTask;
        });

        // Call the next delegate/middleware in the pipeline   
        await _next(context);
    }
}
