using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Org.BouncyCastle.Cms;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmtpSettingsDto = ErpOnlineOrder.Application.DTOs.SettingsDTOs.SmtpSettingsDto;

namespace ErpOnlineOrder.Application.Services
{
   public class EmailService : IEmailService
   {
        private readonly ISettingService _settingService;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ICustomerManagementRepository _customerManagementRepository;

        public EmailService(
             ISettingService settingService,
             IOrderRepository orderRepository,
             IRoleRepository roleRepository,
             IUserRepository userRepository,
             ICustomerManagementRepository customerManagementRepository)
        {
            _settingService = settingService;
            _orderRepository = orderRepository;
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _customerManagementRepository = customerManagementRepository;
        }

        public async Task SendOrderNotificationForStaffAndAdminAsync(int OrderId, CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(OrderId);
                if ( order == null)
                    return;
                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (smtp == null)
                    return;
                var invoiceEmail = await getNotificationInvoiceAsync(order.Customer_id);

                var subject = $"[Thông báo] Đơn hàng mới đã được tạo - Đơn hàng #{order.Order_code}";
                var bodyForStaffAndAdmin = BuildOrderNotificationBodyForStaffAndAdmin(order);
                foreach ( var email in invoiceEmail)
                {
                    await SendEmailAsync(
                        settings : smtp,
                        toEmail: email,
                        subject: subject,
                        htmlBody: bodyForStaffAndAdmin,
                        cancellationToken: cancellationToken);
                }

            }
            catch (Exception ex)
            {
            }
            
        }

        public async Task SendOrderNotificationForCustomerAsync(int OrderId, CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(OrderId);
                if (order == null)
                    return;
                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (smtp == null)
                    return;
                var invoiceEmail = await getNotificationInvoiceAsync(order.Customer_id);
                var subject = $"[Thông báo] Đơn hàng mới đã được tạo - Đơn hàng #{order.Order_code}";
                var htmlBodyForCustomer = BuildOrderNotificationBodyCustomer(order);
                foreach (var email in invoiceEmail)
                {
                    await SendEmailAsync(
                        settings: smtp,
                        toEmail: email,
                        subject: subject,
                        htmlBody: htmlBodyForCustomer,
                        cancellationToken: cancellationToken);
                }
            }

            catch (Exception ex)
            {
            }
        }


        public async Task<HashSet<String>> getNotificationInvoiceAsync(int CustomerId)
        {
            var emails = new HashSet<String>();

            var admin = await _roleRepository.GetByNameAsync("ROLE_ADMIN");

            if ( admin != null) 
            {
                var adminUsers = await _userRepository.GetUsersByRoleAsync(admin.Id);
                foreach ( var user in adminUsers)
                {
                    if ( user.Is_active && !user.Is_deleted)
                    {
                        emails.Add( user.Email);
                    }
                }
            }

            var managentStaff = await _customerManagementRepository.GetByCustomerAsync(CustomerId);

            if (managentStaff != null)
            {
                foreach (var staff in managentStaff)
                {
                    if (staff.Staff != null &&  !staff.Staff.Is_deleted)
                    {
                        var StaffUser = await _userRepository.GetByIdAsync(staff.Staff.User_id);
                        if (StaffUser != null && StaffUser.Is_active && !StaffUser.Is_deleted)
                        {
                            emails.Add(StaffUser.Email);
                        }
                    }
                }
            }

            return emails;
        }

        public async Task getNotificationForCusInvoiceAsync(int CustomerId)
        {
            var email = new HashSet<String>();

            var managentStaff = await _customerManagementRepository.GetByCustomerAsync(CustomerId);
            if (managentStaff != null)
            {
                foreach (var customer in managentStaff)
                {
                    if (customer.Customer != null && !customer.Customer.Is_deleted)
                    {
                        var CustomerUser = await _userRepository.GetByIdAsync(customer.Customer.User_id);
                        if (CustomerUser != null && CustomerUser.Is_active && !CustomerUser.Is_deleted)
                        {
                            email.Add(CustomerUser.Email);
                        }
                    }
                }
            }
        }

        private static string BuildOrderNotificationBodyForStaffAndAdmin(Order order)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif;'>");
            sb.Append("<h2>Đơn hàng mới - Thông báo</h2>");
            sb.Append($"<p><strong>Mã đơn:</strong> {WebUtility.HtmlEncode(order.Order_code)}</p>");
            sb.Append($"<p><strong>Ngày đặt:</strong> {order.Order_date:dd/MM/yyyy HH:mm}</p>");
            sb.Append($"<p><strong>Trạng thái:</strong> {WebUtility.HtmlEncode(order.Order_status ?? "Pending")}</p>");

            if (order.Customer != null)
            {
                sb.Append($"<p><strong>Khách hàng:</strong> {WebUtility.HtmlEncode(order.Customer.Full_name ?? order.Customer.Customer_code)}</p>");
                sb.Append($"<p><strong>Điện thoại:</strong> {WebUtility.HtmlEncode(order.Customer.Phone_number ?? "")}</p>");
                sb.Append($"<p><strong>Địa chỉ giao:</strong> {WebUtility.HtmlEncode(order.Shipping_address ?? order.Customer.Address ?? "")}</p>");
            }

            if (!string.IsNullOrWhiteSpace(order.note))
                sb.Append($"<p><strong>Ghi chú:</strong> {WebUtility.HtmlEncode(order.note)}</p>");

            sb.Append("<h3>Chi tiết đơn hàng</h3>");
            sb.Append("<table border='1' cellpadding='8' cellspacing='0' style='border-collapse: collapse; width: 100%;'>");
            sb.Append("<tr style='background: #f0f0f0;'><th>STT</th><th>Sản phẩm</th><th>SL</th><th>Đơn giá</th><th>Thành tiền</th></tr>");

            int stt = 1;
            foreach (var od in order.Order_Details ?? [])
            {
                var productName = od.Product?.Product_name ?? $"Product #{od.Product_id}";
                sb.Append($"<tr><td>{stt}</td><td>{WebUtility.HtmlEncode(productName)}</td><td>{od.Quantity}</td><td>{od.Unit_price:N0}</td><td>{od.Total_price:N0}</td></tr>");
                stt++;
            }

            sb.Append("</table>");
            sb.Append($"<p><strong>Tổng tiền:</strong> {order.Total_price:N0} VNĐ</p>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        private static string BuildOrderNotificationBodyCustomer(Order order)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif;'>");
            sb.Append("<h2>Đơn hàng mới - Thông báo</h2>");
            sb.Append($"<p><strong>Mã đơn:</strong> {WebUtility.HtmlEncode(order.Order_code)}</p>");
            sb.Append($"<p><strong>Ngày đặt:</strong> {order.Order_date:dd/MM/yyyy HH:mm}</p>");
            sb.Append($"<p><strong>Trạng thái:</strong> {WebUtility.HtmlEncode(order.Order_status ?? "Pending")}</p>");

            if (order.Customer != null)
            {
                sb.Append($"<p><strong>Khách hàng:</strong> {WebUtility.HtmlEncode(order.Customer.Full_name ?? order.Customer.Customer_code)}</p>");
                sb.Append($"<p><strong>Điện thoại:</strong> {WebUtility.HtmlEncode(order.Customer.Phone_number ?? "")}</p>");
                sb.Append($"<p><strong>Địa chỉ giao:</strong> {WebUtility.HtmlEncode(order.Shipping_address ?? order.Customer.Address ?? "")}</p>");
            }

            if (!string.IsNullOrWhiteSpace(order.note))
                sb.Append($"<p><strong>Ghi chú:</strong> {WebUtility.HtmlEncode(order.note)}</p>");

            sb.Append("<h3>Chi tiết đơn hàng</h3>");
            sb.Append("<table border='1' cellpadding='8' cellspacing='0' style='border-collapse: collapse; width: 100%;'>");
            sb.Append("<tr style='background: #f0f0f0;'><th>STT</th><th>Sản phẩm</th><th>SL</th><th>Đơn giá</th><th>Thành tiền</th></tr>");

            int stt = 1;
            foreach (var od in order.Order_Details ?? [])
            {
                var productName = od.Product?.Product_name ?? $"Product #{od.Product_id}";
                sb.Append($"<tr><td>{stt}</td><td>{WebUtility.HtmlEncode(productName)}</td><td>{od.Quantity}</td><td>{od.Unit_price:N0}</td><td>{od.Total_price:N0}</td></tr>");
                stt++;
            }

            sb.Append("</table>");
            sb.Append($"<p><strong>Tổng tiền:</strong> {order.Total_price:N0} VNĐ</p>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        private async Task SendEmailAsync(SmtpSettingsDto settings, string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(settings.FromName, settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                var secureSocketOptions = settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
                await smtp.ConnectAsync(settings.Host, settings.Port, secureSocketOptions, cancellationToken);
                if (!string.IsNullOrEmpty(settings.Password))
                    await smtp.AuthenticateAsync(settings.FromEmail, settings.Password, cancellationToken);
                await smtp.SendAsync(message, cancellationToken);
            }
            finally
            {
                await smtp.DisconnectAsync(true, cancellationToken);
            }
        }

    }
}



