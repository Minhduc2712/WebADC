using ErpOnlineOrder.Application.DTOs.EmailDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ErpOnlineOrder.Infrastructure.Email
{
    /// <summary>
    /// BackgroundService chạy ngầm liên tục, lấy từng EmailMessage từ IEmailQueue
    /// rồi dispatch sang IEmailService để gửi thực tế.
    /// Tạo IServiceScope riêng cho mỗi email để dùng Scoped services (DbContext, Repositories).
    /// </summary>
    public sealed class EmailWorker : BackgroundService
    {
        private readonly IEmailQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EmailWorker> _logger;

        public EmailWorker(
            IEmailQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<EmailWorker> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EmailWorker started.");

            await foreach (var message in _queue.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await ProcessAsync(message, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "EmailWorker: lỗi xử lý email {ActionType} PrimaryId={PrimaryId}",
                        message.ActionType, message.PrimaryId);
                }
            }

            _logger.LogInformation("EmailWorker stopped.");
        }

        private async Task ProcessAsync(EmailMessage msg, CancellationToken ct)
        {
            // Tạo scope mới cho mỗi email — đảm bảo DbContext / Repositories sạch sẽ
            await using var scope = _scopeFactory.CreateAsyncScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            switch (msg.ActionType)
            {
                case EmailActionType.OrderNotificationStaffAndAdmin:
                    await emailService.SendOrderNotificationForStaffAndAdminAsync(msg.PrimaryId!.Value, ct);
                    break;

                case EmailActionType.OrderNotificationCustomer:
                    await emailService.SendOrderNotificationForCustomerAsync(msg.PrimaryId!.Value, ct);
                    break;

                case EmailActionType.OrderConfirmedNotificationCustomer:
                    await emailService.SendOrderConfirmedNotificationForCustomerAsync(msg.PrimaryId!.Value, ct);
                    break;

                case EmailActionType.OrderWaitingCustomerNotification:
                    await emailService.SendOrderWaitingCustomerNotificationAsync(msg.PrimaryId!.Value, ct);
                    break;

                case EmailActionType.OrderUpdatedNotificationCustomer:
                    await emailService.SendOrderUpdatedNotificationForCustomerAsync(msg.PrimaryId!.Value, ct);
                    break;

                case EmailActionType.OrderRejectedByCustomerNotification:
                    await emailService.SendOrderRejectedByCustomerNotificationAsync(msg.PrimaryId!.Value, ct);
                    break;

                case EmailActionType.WarehouseExportNotificationStaffAndAdmin:
                    await emailService.SendWarehouseExportNotificationForStaffAndAdminAsync(msg.PrimaryId!.Value, ct);
                    break;

                case EmailActionType.ExportDeliveryStatusToCustomer:
                    await emailService.SendExportDeliveryStatusToCustomerAsync(msg.PrimaryId!.Value, ct);
                    break;

                case EmailActionType.CustomerInvoiceRequestNotification:
                    await emailService.SendCustomerInvoiceRequestNotificationAsync(
                        msg.PrimaryId!.Value, msg.SecondaryId!.Value, ct);
                    break;

                case EmailActionType.CustomerRegistrationNotification:
                    await emailService.SendCustomerRegistrationNotificationAsync(msg.PrimaryId!.Value, ct);
                    break;

                case EmailActionType.StaffReplacementNotification:
                    await emailService.SendStaffReplacementNotificationAsync(
                        msg.PrimaryId!.Value, msg.SecondaryId!.Value, msg.TertiaryId!.Value, ct);
                    break;

                case EmailActionType.ProductAssignedToCustomer:
                    await emailService.SendProductAssignedToCustomerAsync(
                        msg.PrimaryId!.Value, msg.IdList!, ct);
                    break;

                case EmailActionType.PasswordReset:
                    await emailService.SendPasswordResetEmailAsync(
                        msg.PrimaryId!.Value, msg.Payload!, ct);
                    break;

                default:
                    _logger.LogWarning("EmailWorker: không nhận ra ActionType {ActionType}", msg.ActionType);
                    break;
            }

            _logger.LogDebug("EmailWorker: đã gửi email {ActionType} PrimaryId={PrimaryId}",
                msg.ActionType, msg.PrimaryId);
        }
    }
}
