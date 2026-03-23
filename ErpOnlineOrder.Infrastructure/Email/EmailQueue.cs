using ErpOnlineOrder.Application.DTOs.EmailDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace ErpOnlineOrder.Infrastructure.Email
{
    /// <summary>
    /// Hàng đợi email in-memory sử dụng System.Threading.Channels.
    /// Bounded capacity = 1000: nếu hàng đợi đầy, EnqueueAsync sẽ await giải phóng slot
    /// thay vì mất email hoặc throw exception.
    /// </summary>
    public sealed class EmailQueue : IEmailQueue
    {
        private readonly Channel<EmailMessage> _channel;

        public EmailQueue()
        {
            var options = new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,   // chỉ EmailWorker đọc
                SingleWriter = false,  // nhiều service có thể ghi
            };
            _channel = Channel.CreateBounded<EmailMessage>(options);
        }

        public ValueTask EnqueueAsync(EmailMessage message, CancellationToken cancellationToken = default)
            => _channel.Writer.WriteAsync(message, cancellationToken);

        public async IAsyncEnumerable<EmailMessage> ReadAllAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var message in _channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return message;
            }
        }
    }
}
