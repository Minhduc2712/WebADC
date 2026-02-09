using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    
    /// Base controller với xử lý lỗi chi tiết và hỗ trợ thông báo
    
    public class BaseController : Controller
    {
                
        protected string GetDetailedErrorMessage(Exception ex)
        {
            if (ex is Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                if (dbEx.InnerException != null)
                {
                    var innerMessage = dbEx.InnerException.Message;

                    if (innerMessage.Contains("UNIQUE KEY constraint") || innerMessage.Contains("duplicate key"))
                    {
                        return "Dữ liệu đã tồn tại. Vui lòng kiểm tra và thử lại.";
                    }
                    else if (innerMessage.Contains("FOREIGN KEY constraint") || innerMessage.Contains("conflicted with the FOREIGN KEY constraint"))
                    {
                        return "Không thể thực hiện thao tác vì dữ liệu đang được sử dụng bởi các bản ghi khác.";
                    }
                    else if (innerMessage.Contains("CHECK constraint") || innerMessage.Contains("conflicted with the CHECK constraint"))
                    {
                        return "Dữ liệu không hợp lệ. Vui lòng kiểm tra các ràng buộc.";
                    }
                    else if (innerMessage.Contains("cannot insert the value NULL") || innerMessage.Contains("cannot be null"))
                    {
                        return "Thiếu thông tin bắt buộc. Vui lòng điền đầy đủ các trường.";
                    }
                    else if (innerMessage.Contains("String or binary data would be truncated"))
                    {
                        return "Dữ liệu quá dài. Vui lòng rút ngắn nội dung.";
                    }
                    else if (innerMessage.Contains("timeout") || innerMessage.Contains("Timeout"))
                    {
                        return "Hệ thống bận. Vui lòng thử lại sau.";
                    }
                    else if (innerMessage.Contains("connection") || innerMessage.Contains("Connection"))
                    {
                        return "Không thể kết nối đến cơ sở dữ liệu. Vui lòng thử lại sau.";
                    }
                    else
                    {
                        return $"Lỗi cơ sở dữ liệu: {innerMessage}";
                    }
                }
                else
                {
                    return "Lỗi khi lưu dữ liệu vào cơ sở dữ liệu.";
                }
            }
            // Xử lý các loại exception khác
            else if (ex is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                return "Dữ liệu đã được thay đổi bởi người dùng khác. Vui lòng tải lại trang và thử lại.";
            }
            else if (ex is InvalidOperationException)
            {
                // Chỉ trả về thông báo mặc định nếu message rỗng
                return !string.IsNullOrEmpty(ex.Message)
                    ? ex.Message
                    : "Thao tác không hợp lệ. Vui lòng kiểm tra dữ liệu và thử lại.";
            }
            else if (ex is ArgumentException argEx)
            {
                // Giữ nguyên thông báo cụ thể từ service
                return !string.IsNullOrEmpty(argEx.Message)
                    ? argEx.Message
                    : "Dữ liệu không hợp lệ. Vui lòng kiểm tra và thử lại.";
            }
            else if (ex is TimeoutException)
            {
                return "Hệ thống phản hồi chậm. Vui lòng thử lại sau.";
            }
            else if (ex is Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                // Xử lý các lỗi SQL Server cụ thể
                switch (sqlEx.Number)
                {
                    case 2627: // Unique constraint violation
                        return "Dữ liệu đã tồn tại. Vui lòng kiểm tra và thử lại.";
                    case 547: // Foreign key constraint violation
                        return "Không thể thực hiện thao tác vì dữ liệu đang được sử dụng bởi các bản ghi khác.";
                    case 2601: // Duplicate key
                        return "Dữ liệu đã tồn tại. Vui lòng kiểm tra và thử lại.";
                    case 515: // Cannot insert null
                        return "Thiếu thông tin bắt buộc. Vui lòng điền đầy đủ các trường.";
                    case 8152: // String or binary data would be truncated
                        return "Dữ liệu quá dài. Vui lòng rút ngắn nội dung.";
                    case -2: // Timeout
                        return "Hệ thống bận. Vui lòng thử lại sau.";
                    default:
                        return $"Lỗi SQL Server: {sqlEx.Message}";
                }
            }
            else
            {
                // Trả về thông báo lỗi gốc nếu không thể xử lý
                return ex.Message;
            }
        }

        /// Đặt thông báo thành công
        
        protected void SetSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        
        /// Đặt thông báo lỗi
        
        protected void SetErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        
        /// Đặt thông báo lỗi với danh sách chi tiết
        
        protected void SetErrorMessage(string message, List<string> details)
        {
            TempData["ErrorMessage"] = message;
            if (details != null && details.Count > 0)
            {
                TempData["ErrorDetails"] = JsonSerializer.Serialize(details);
            }
        }

        
        /// Đặt thông báo lỗi từ Exception với danh sách chi tiết
        
        protected void SetErrorFromException(Exception ex, string? contextMessage = null)
        {
            var errorMessage = GetDetailedErrorMessage(ex);
            var details = new List<string>();

            // Thêm context nếu có
            if (!string.IsNullOrEmpty(contextMessage))
            {
                TempData["ErrorMessage"] = contextMessage;
                details.Add(errorMessage);
            }
            else
            {
                TempData["ErrorMessage"] = errorMessage;
            }

            // Thêm inner exception nếu có
            if (ex.InnerException != null && ex.InnerException.Message != ex.Message)
            {
                details.Add($"Chi tiết: {ex.InnerException.Message}");
            }

            if (details.Count > 0)
            {
                TempData["ErrorDetails"] = JsonSerializer.Serialize(details);
            }
        }

        
        /// Đặt thông báo cảnh báo
        
        protected void SetWarningMessage(string message)
        {
            TempData["WarningMessage"] = message;
        }

        
        /// Đặt thông báo thông tin
        
        protected void SetInfoMessage(string message)
        {
            TempData["InfoMessage"] = message;
        }

        
        /// Lấy tất cả lỗi từ ModelState dưới dạng danh sách
        
        protected List<string> GetModelStateErrors()
        {
            return ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(msg => !string.IsNullOrEmpty(msg))
                .ToList();
        }

        
        /// Đặt thông báo lỗi từ ModelState errors
        
        protected void SetErrorFromModelState(string? headerMessage = null)
        {
            var errors = GetModelStateErrors();
            if (errors.Count > 0)
            {
                TempData["ErrorMessage"] = headerMessage ?? "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại các trường bên dưới.";
                TempData["ErrorDetails"] = JsonSerializer.Serialize(errors);
            }
        }
    }
}
