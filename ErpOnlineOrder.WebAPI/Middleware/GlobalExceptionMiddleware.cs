using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using ErpOnlineOrder.Application.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ErpOnlineOrder.WebApi.Middleware
{
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
                _logger.LogError(ex, "Đã xảy ra lỗi hệ thống: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var message = "Đã có lỗi xảy ra. Vui lòng thử lại.";

            // Bóc tách lỗi từ Entity Framework / SQL Server
            if (ex is DbUpdateException dbEx && dbEx.InnerException != null)
            {
                var innerMessage = dbEx.InnerException.Message;
                if (innerMessage.Contains("UNIQUE KEY constraint") || innerMessage.Contains("duplicate key"))
                    message = "Dữ liệu đã tồn tại. Vui lòng kiểm tra và thử lại.";
                else if (innerMessage.Contains("FOREIGN KEY constraint"))
                    message = "Không thể thực hiện thao tác vì dữ liệu đang được sử dụng bởi các bản ghi khác.";
                else if (innerMessage.Contains("cannot insert the value NULL"))
                    message = "Thiếu thông tin bắt buộc. Vui lòng điền đầy đủ các trường.";
            }
            else if (ex is SqlException sqlEx)
            {
                message = sqlEx.Number switch
                {
                    2627 or 2601 => "Dữ liệu đã tồn tại. Vui lòng kiểm tra và thử lại.",
                    547 => "Không thể thực hiện thao tác vì dữ liệu đang được sử dụng.",
                    515 => "Thiếu thông tin bắt buộc. Vui lòng điền đầy đủ các trường.",
                    -2 => "Hệ thống bận. Vui lòng thử lại sau.",
                    _ => $"Lỗi cơ sở dữ liệu: {sqlEx.Message}"
                };
            }
            else if (ex is ArgumentException || ex is InvalidOperationException)
            {
                message = ex.Message;
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                message = "Lỗi máy chủ nội bộ.";
            }

            var response = ApiResponse<object>.Fail(message);
            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}