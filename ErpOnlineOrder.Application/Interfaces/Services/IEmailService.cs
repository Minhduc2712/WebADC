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

        //    Task SendOrderConfirmationAsync(string toEmail, string subject, string body);
        //    Task SendOrderCancellationAsync(string toEmail, string subject, string body);
        //    Task SendOrderCompletionAsync(string toEmail, string subject, string body);
        //    Task SendOrderRejectionAsync(string toEmail, string subject, string body);
        //    Task SendOrderApprovalAsync(string toEmail, string subject, string body);
        //    Task SendOrderPaymentAsync(string toEmail, string subject, string body);
        //    Task SendOrderDeliveryAsync(string toEmail, string subject, string body);
    }
}
