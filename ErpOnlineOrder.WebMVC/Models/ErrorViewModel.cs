namespace ErpOnlineOrder.WebMVC.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public int StatusCode { get; set; } = 500;
        public string Title { get; set; } = "Đã xảy ra lỗi";
        public string Message { get; set; } = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại sau.";
        public List<string> ErrorDetails { get; set; } = new();
        public string? TechnicalDetails { get; set; }
        public string? Path { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string IconClass => StatusCode switch
        {
            400 => "fas fa-exclamation-triangle",
            401 => "fas fa-lock",
            403 => "fas fa-ban",
            404 => "fas fa-search",
            408 => "fas fa-clock",
            429 => "fas fa-tachometer-alt",
            500 => "fas fa-server",
            502 => "fas fa-plug",
            503 => "fas fa-tools",
            _ => "fas fa-exclamation-circle"
        };
        public string ColorClass => StatusCode switch
        {
            400 => "warning",
            401 => "info",
            403 => "danger",
            404 => "primary",
            408 => "warning",
            429 => "warning",
            500 => "danger",
            502 => "danger",
            503 => "warning",
            _ => "danger"
        };
        public static ErrorViewModel FromStatusCode(int statusCode, string? requestId = null)
        {
            return statusCode switch
            {
                400 => new ErrorViewModel
                {
                    StatusCode = 400,
                    Title = "Yêu cầu không hợp lệ",
                    Message = "Yêu cầu của bạn không đúng định dạng hoặc thiếu thông tin. Vui lòng kiểm tra lại.",
                    RequestId = requestId
                },
                401 => new ErrorViewModel
                {
                    StatusCode = 401,
                    Title = "Chưa đăng nhập",
                    Message = "Bạn cần đăng nhập để truy cập chức năng này.",
                    RequestId = requestId
                },
                403 => new ErrorViewModel
                {
                    StatusCode = 403,
                    Title = "Truy cập bị từ chối",
                    Message = "Bạn không có quyền truy cập vào chức năng này. Vui lòng liên hệ quản trị viên để được cấp quyền.",
                    RequestId = requestId
                },
                404 => new ErrorViewModel
                {
                    StatusCode = 404,
                    Title = "Không tìm thấy trang",
                    Message = "Trang bạn đang tìm kiếm không tồn tại hoặc đã bị di chuyển.",
                    RequestId = requestId
                },
                408 => new ErrorViewModel
                {
                    StatusCode = 408,
                    Title = "Hết thời gian chờ",
                    Message = "Yêu cầu đã hết thời gian chờ. Hệ thống có thể đang bận, vui lòng thử lại sau.",
                    RequestId = requestId
                },
                429 => new ErrorViewModel
                {
                    StatusCode = 429,
                    Title = "Quá nhiều yêu cầu",
                    Message = "Bạn đã gửi quá nhiều yêu cầu trong thời gian ngắn. Vui lòng chờ một lát rồi thử lại.",
                    RequestId = requestId
                },
                500 => new ErrorViewModel
                {
                    StatusCode = 500,
                    Title = "Lỗi máy chủ",
                    Message = "Đã xảy ra lỗi nội bộ. Đội kỹ thuật đã được thông báo và đang khắc phục.",
                    RequestId = requestId
                },
                502 => new ErrorViewModel
                {
                    StatusCode = 502,
                    Title = "Lỗi kết nối",
                    Message = "Không thể kết nối đến máy chủ. Vui lòng thử lại sau ít phút.",
                    RequestId = requestId
                },
                503 => new ErrorViewModel
                {
                    StatusCode = 503,
                    Title = "Dịch vụ tạm ngưng",
                    Message = "Hệ thống đang bảo trì hoặc quá tải. Vui lòng quay lại sau.",
                    RequestId = requestId
                },
                _ => new ErrorViewModel
                {
                    StatusCode = statusCode,
                    Title = "Đã xảy ra lỗi",
                    Message = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại sau.",
                    RequestId = requestId
                }
            };
        }
    }
}
