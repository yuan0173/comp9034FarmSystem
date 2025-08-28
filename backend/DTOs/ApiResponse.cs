namespace COMP9034.Backend.DTOs
{
    /// <summary>
    /// 统一API响应格式
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// 响应消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 响应数据
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// 错误详情
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 创建成功响应
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="message">消息</param>
        /// <returns>API响应</returns>
        public static ApiResponse<T> CreateSuccess(T data, string message = "操作成功")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// 创建错误响应
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="error">错误详情</param>
        /// <returns>API响应</returns>
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
    /// 分页响应
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class PagedResponse<T> : ApiResponse<IEnumerable<T>>
    {
        /// <summary>
        /// 当前页码
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// 创建分页响应
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="totalRecords">总记录数</param>
        /// <param name="message">消息</param>
        /// <returns>分页响应</returns>
        public static PagedResponse<T> CreatePaged(
            IEnumerable<T> data, 
            int page, 
            int pageSize, 
            int totalRecords,
            string message = "查询成功")
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