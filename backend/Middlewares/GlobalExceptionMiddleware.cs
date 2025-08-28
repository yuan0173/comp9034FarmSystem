using COMP9034.Backend.DTOs;
using System.Net;
using System.Text.Json;

namespace COMP9034.Backend.Middlewares
{
    /// <summary>
    /// 全局异常处理中间件
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
                _logger.LogError(ex, "发生未处理的异常: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = new ApiResponse<object>();
            
            switch (exception)
            {
                case ArgumentNullException:
                case ArgumentException:
                    response = ApiResponse<object>.CreateError("请求参数无效", exception.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                    
                case UnauthorizedAccessException:
                    response = ApiResponse<object>.CreateError("未授权访问", exception.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;
                    
                case KeyNotFoundException:
                    response = ApiResponse<object>.CreateError("请求的资源未找到", exception.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                    
                case TimeoutException:
                    response = ApiResponse<object>.CreateError("请求超时", exception.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    break;
                    
                default:
                    response = ApiResponse<object>.CreateError("内部服务器错误", 
                        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" 
                            ? exception.Message 
                            : "服务器发生错误，请稍后重试");
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    /// <summary>
    /// 全局异常中间件扩展
    /// </summary>
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}