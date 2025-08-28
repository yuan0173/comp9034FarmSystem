namespace COMP9034.Backend.Middlewares
{
    /// <summary>
    /// 请求日志中间件
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
            
            // 添加请求ID到响应头
            context.Response.Headers.Add("X-Request-ID", requestId);
            
            // 记录请求信息
            _logger.LogInformation(
                "[{RequestId}] {Method} {Path} - 开始处理请求",
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
                    "[{RequestId}] {Method} {Path} - 请求完成 {StatusCode} ({Duration}ms)",
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
    /// 请求日志中间件扩展
    /// </summary>
    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}