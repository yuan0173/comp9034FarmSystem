namespace COMP9034.Backend.Middlewares
{
    /// <summary>
    /// Request logging middleware
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var startTime = DateTime.UtcNow;
            var requestId = Guid.NewGuid().ToString("N")[..8];
            
            // Add request ID to response headers
            context.Response.Headers.Add("X-Request-ID", requestId);
            
            // Log request information
            _logger.LogInformation(
                "[{RequestId}] {Method} {Path} - Started processing request",
                requestId,
                context.Request.Method,
                context.Request.Path
            );

            try
            {
                await _next(context);
            }
            finally
            {
                var duration = DateTime.UtcNow - startTime;
                
                _logger.LogInformation(
                    "[{RequestId}] {Method} {Path} - Completed {StatusCode} ({Duration}ms)",
                    requestId,
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    duration.TotalMilliseconds
                );
            }
        }
    }

    /// <summary>
    /// Request logging middleware extensions
    /// </summary>
    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
