using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    public class BaseController : Controller
    {
        protected int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        protected string GetDetailedErrorMessage(Exception ex)
        {
            // Tầng MVC chỉ nên xử lý lỗi HTTP, Argument, Timeout hoặc lỗi chung.
            // Các lỗi DB (SqlException) đã được WebAPI bắt và bóc tách thành message gửi về thông qua ApiClient.
            
            if (ex is HttpRequestException httpEx)
            {
                return $"Không thể kết nối đến máy chủ API: {httpEx.Message}";
            }
            else if (ex is TimeoutException)
            {
                return "Hệ thống phản hồi chậm. Vui lòng thử lại sau.";
            }
            
            return !string.IsNullOrEmpty(ex.Message) ? ex.Message : "Đã có lỗi xảy ra. Vui lòng thử lại.";
        }

        protected void SetSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        protected void SetErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        protected void SetErrorMessage(string message, List<string> details)
        {
            TempData["ErrorMessage"] = message;
            if (details != null && details.Count > 0)
            {
                TempData["ErrorDetails"] = JsonSerializer.Serialize(details);
            }
        }

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

        protected void SetWarningMessage(string message)
        {
            TempData["WarningMessage"] = message;
        }

        protected void SetInfoMessage(string message)
        {
            TempData["InfoMessage"] = message;
        }

        protected List<string> GetModelStateErrors()
        {
            return ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(msg => !string.IsNullOrEmpty(msg))
                .ToList();
        }

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
