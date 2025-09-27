namespace COMP9034.Backend.DTOs
{
    /// <summary>
    /// Unified API response format
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Response message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Response data
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Error details
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Create success response
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="message">Message</param>
        /// <returns>API response</returns>
        public static ApiResponse<T> CreateSuccess(T data, string message = "Operation successful")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// Create error response
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="error">Error details</param>
        /// <returns>API response</returns>
        public static ApiResponse<T> CreateError(string message, string? error = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Error = error,
                Data = default
            };
        }
    }

    /// <summary>
    /// Paged response
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    public class PagedResponse<T> : ApiResponse<IEnumerable<T>>
    {
        /// <summary>
        /// Current page number
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total records count
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Total pages count
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Whether has next page
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Whether has previous page
        /// </summary>
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// Create paged response
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalRecords">Total records count</param>
        /// <param name="message">Message</param>
        /// <returns>Paged response</returns>
        public static PagedResponse<T> CreatePaged(
            IEnumerable<T> data, 
            int page, 
            int pageSize, 
            int totalRecords,
            string message = "Query successful")
        {
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            
            return new PagedResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Page = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };
        }
    }
}