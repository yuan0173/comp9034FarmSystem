using System;

namespace COMP9034.Backend.Common.Results
{
    /// <summary>
    /// Standard API response wrapper
    /// </summary>
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public string? ErrorCode { get; set; }
        public object? Errors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResult<T> SuccessResult(T data, string message = "Operation completed successfully")
        {
            return new ApiResult<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResult<T> ErrorResult(string message, string? errorCode = null, object? errors = null)
        {
            return new ApiResult<T>
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode,
                Errors = errors
            };
        }
    }

    /// <summary>
    /// Standard API response without data
    /// </summary>
    public class ApiResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public object? Errors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResult SuccessResult(string message = "Operation completed successfully")
        {
            return new ApiResult
            {
                Success = true,
                Message = message
            };
        }

        public static ApiResult ErrorResult(string message, string? errorCode = null, object? errors = null)
        {
            return new ApiResult
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode,
                Errors = errors
            };
        }
    }
}