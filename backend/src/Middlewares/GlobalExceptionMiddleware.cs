using COMP9034.Backend.Common.Exceptions;
using COMP9034.Backend.Common.Results;
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

            ApiResult<object> response;

            switch (exception)
            {
                case BusinessException businessEx:
                    response = ApiResult<object>.ErrorResult(businessEx.Message, businessEx.ErrorCode, businessEx.Details);
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case ValidationException validationEx:
                    response = ApiResult<object>.ErrorResult("Validation failed", "VALIDATION_ERROR", validationEx.Errors);
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case ArgumentNullException argNullEx:
                    response = ApiResult<object>.ErrorResult("Required parameter is missing", "ARGUMENT_NULL", argNullEx.ParamName);
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case ArgumentException argEx:
                    response = ApiResult<object>.ErrorResult("Invalid parameter value", "ARGUMENT_INVALID", argEx.ParamName);
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case UnauthorizedAccessException:
                    response = ApiResult<object>.ErrorResult("Unauthorized access", "UNAUTHORIZED");
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;

                case KeyNotFoundException:
                    response = ApiResult<object>.ErrorResult("Requested resource not found", "NOT_FOUND");
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case TimeoutException:
                    response = ApiResult<object>.ErrorResult("Request timeout", "TIMEOUT");
                    context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    break;

                case InvalidOperationException invalidOpEx:
                    response = ApiResult<object>.ErrorResult("Invalid operation", "INVALID_OPERATION", invalidOpEx.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    break;

                case NotSupportedException notSupportedEx:
                    response = ApiResult<object>.ErrorResult("Operation not supported", "NOT_SUPPORTED", notSupportedEx.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                    break;

                default:
                    var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
                    response = ApiResult<object>.ErrorResult(
                        "Internal server error",
                        "INTERNAL_ERROR",
                        isDevelopment ? new { Message = exception.Message, StackTrace = exception.StackTrace } : null);
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
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