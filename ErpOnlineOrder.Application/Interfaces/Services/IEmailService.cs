using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
   public interface IEmailService
   {
       Task SendOrderNotificationForStaffAndAdminAsync(int OrderId, CancellationToken cancellationToken = default);
       Task SendOrderNotificationForCustomerAsync(int OrderId, CancellationToken cancellationToken = default);
       Task SendOrderConfirmedNotificationForCustomerAsync(int OrderId, CancellationToken cancellationToken = default);
       Task SendOrderWaitingCustomerNotificationAsync(int OrderId, CancellationToken cancellationToken = default);
       Task SendWarehouseExportNotificationForStaffAndAdminAsync(int warehouseExportId, CancellationToken cancellationToken = default);
       Task SendExportDeliveryStatusToCustomerAsync(int warehouseExportId, CancellationToken cancellationToken = default);
        Task SendCustomerInvoiceRequestNotificationAsync(int exportId, int invoiceCount, CancellationToken cancellationToken = default);
       Task SendOrderUpdatedNotificationForCustomerAsync(int orderId, CancellationToken cancellationToken = default);
        Task SendOrderRejectedByCustomerNotificationAsync(int orderId, CancellationToken cancellationToken = default);
        Task SendCustomerRegistrationNotificationAsync(int customerId, CancellationToken cancellationToken = default);
        Task SendStaffReplacementNotificationAsync(int customerId, int oldStaffId, int newStaffId, CancellationToken cancellationToken = default);
        Task SendStaffAssignmentNotificationAsync(int customerId, int staffId, CancellationToken cancellationToken = default);
        Task SendProductAssignedToCustomerAsync(int customerId, List<int> productIds, CancellationToken cancellationToken = default);
        Task SendPackageAssignedToCustomerAsync(int customerId, List<int> packageIds, CancellationToken cancellationToken = default);
        Task SendPasswordResetEmailAsync(int userId, string resetLink, CancellationToken cancellationToken = default);
        Task SendInvoiceToCustomerAsync(int invoiceId, CancellationToken cancellationToken = default);
        Task SendOrgUpdateRequestAsync(int customerId, string requestPayload, CancellationToken cancellationToken = default);
        Task SendExportCancelledNotificationCustomerAsync(int exportId, CancellationToken cancellationToken = default);
        Task SendAdminPasswordResetNotificationAsync(int userId, CancellationToken cancellationToken = default);


        //    Task SendOrderConfirmationAsync(string toEmail, string subject, string body);
        //    Task SendOrderCancellationAsync(string toEmail, string subject, string body);
        //    Task SendOrderCompletionAsync(string toEmail, string subject, string body);
        //    Task SendOrderRejectionAsync(string toEmail, string subject, string body);
        //    Task SendOrderApprovalAsync(string toEmail, string subject, string body);
        //    Task SendOrderPaymentAsync(string toEmail, string subject, string body);
        //    Task SendOrderDeliveryAsync(string toEmail, string subject, string body);
    }
}
