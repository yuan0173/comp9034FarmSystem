using COMP9034.Backend.DTOs;
using System.Net;
using System.Text.Json;

namespace COMP9034.Backend.Middlewares
{
    /// <summary>
    /// Global exception handling middleware
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
                _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
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
                    response = ApiResponse<object>.CreateError("Invalid request parameters", exception.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                    
                case UnauthorizedAccessException:
                    response = ApiResponse<object>.CreateError("Unauthorized access", exception.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;
                    
                case KeyNotFoundException:
                    response = ApiResponse<object>.CreateError("Requested resource not found", exception.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                    
                case TimeoutException:
                    response = ApiResponse<object>.CreateError("Request timeout", exception.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    break;
                    
                default:
                    response = ApiResponse<object>.CreateError("Internal server error", 
                        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" 
                            ? exception.Message 
                            : "Server error occurred, please try again later");
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
    /// Global exception middleware extensions
    /// </summary>
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}