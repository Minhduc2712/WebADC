using ErpOnlineOrder.Application.DTOs.EmailDTOs;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    /// <summary>
    /// Hàng đợi email in-memory dựa trên System.Threading.Channels.
    /// Singleton — tồn tại suốt vòng đời ứng dụng.
    /// </summary>
    public interface IEmailQueue
    {
        /// <summary>Đẩy một tác vụ email vào cuối hàng đợi (không chặn luồng API).</summary>
        ValueTask EnqueueAsync(EmailMessage message, CancellationToken cancellationToken = default);

        /// <summary>Dùng bởi EmailWorker để lấy tác vụ kế tiếp (blocking đến khi có việc).</summary>
        IAsyncEnumerable<EmailMessage> ReadAllAsync(CancellationToken cancellationToken);
    }
}
