using System;

namespace COMP9034.Backend.Common.Exceptions
{
    /// <summary>
    /// Business logic exception
    /// </summary>
    public class BusinessException : Exception
    {
        public string ErrorCode { get; }
        public object? Details { get; }

        public BusinessException(string message, string errorCode = "BUSINESS_ERROR")
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public BusinessException(string message, string errorCode, object? details)
            : base(message)
        {
            ErrorCode = errorCode;
            Details = details;
        }

        public BusinessException(string message, Exception innerException, string errorCode = "BUSINESS_ERROR")
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}