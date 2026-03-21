using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Collections.Generic;
using System.Net;
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
        private readonly IWarehouseExportRepository _warehouseExportRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ICustomerManagementRepository _customerManagementRepository;
        private readonly IInvoiceRepository _invoiceRepository;

        public EmailService(
            ISettingService settingService,
            IOrderRepository orderRepository,
            IWarehouseExportRepository warehouseExportRepository,
            IRoleRepository roleRepository,
            IUserRepository userRepository,
            ICustomerManagementRepository customerManagementRepository,
            IInvoiceRepository invoiceRepository)
        {
            _settingService = settingService;
            _orderRepository = orderRepository;
            _warehouseExportRepository = warehouseExportRepository;
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _customerManagementRepository = customerManagementRepository;
            _invoiceRepository = invoiceRepository;
        }

        public async Task SendWarehouseExportNotificationForStaffAndAdminAsync(int warehouseExportId, CancellationToken cancellationToken = default)
        {
            try
            {
                var export = await _warehouseExportRepository.GetByIdAsync(warehouseExportId);
                if (export == null) return;

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                var staffAdminEmails = await GetStaffAndAdminEmailsAsync(export.Customer_id);

                // Thêm Email của bộ phận Kho vào danh sách nhận
                if (export.Warehouse != null && !string.IsNullOrWhiteSpace(export.Warehouse.Warehouse_email))
                {
                    staffAdminEmails.Add(export.Warehouse.Warehouse_email.Trim());
                }
                if (staffAdminEmails.Count == 0) return;

            var invoices = await _invoiceRepository.GetByWarehouseExportIdAsync(export.Id);
            var invoiceCodes = string.Join(", ", invoices.Select(i => i.Invoice_code));

                var subject = $"[Vận chuyển] Phiếu xuất kho mới #{export.Warehouse_export_code}";
            var body = BuildWarehouseExportNotificationBody(export, invoiceCodes);

                foreach (var email in staffAdminEmails)
                {
                    if (string.IsNullOrWhiteSpace(email)) continue;
                    await SendEmailAsync(smtp!, email.Trim(), subject, body, cancellationToken);
                }
            }
            catch
            {
                // Email gửi thất bại - có thể do SMTP chưa cấu hình hoặc lỗi kết nối
            }
        }

        public async Task SendOrderNotificationForStaffAndAdminAsync(int OrderId, CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(OrderId);
                if (order == null) return;

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                var staffAdminEmails = await GetStaffAndAdminEmailsAsync(order.Customer_id);
                if (staffAdminEmails.Count == 0) return;

                var subject = $"[Thông báo] Đơn hàng mới đã được tạo - Đơn hàng #{order.Order_code}";
                var body = BuildOrderNotificationBodyForStaffAndAdmin(order);

                foreach (var email in staffAdminEmails)
                {
                    if (string.IsNullOrWhiteSpace(email)) continue;
                    await SendEmailAsync(smtp!, email.Trim(), subject, body, cancellationToken);
                }
            }
            catch (System.Exception)
            {
                // Email gửi thất bại - có thể do SMTP chưa cấu hình hoặc lỗi kết nối
            }
        }

        public async Task SendOrderNotificationForCustomerAsync(int OrderId, CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(OrderId);
                if (order == null) return;

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                var customerEmail = GetCustomerEmail(order);
                if (string.IsNullOrWhiteSpace(customerEmail)) return;

                var subject = $"[Xác nhận] Đơn hàng của bạn đã được tạo - Đơn hàng #{order.Order_code}";
                var body = BuildOrderNotificationBodyCustomer(order);

                await SendEmailAsync(smtp!, customerEmail, subject, body, cancellationToken);
            }
            catch (System.Exception)
            {
                // Email gửi thất bại - có thể do SMTP chưa cấu hình hoặc lỗi kết nối
            }
        }

        public async Task SendOrderConfirmedNotificationForCustomerAsync(int OrderId, CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(OrderId);
                if (order == null) return;

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                var customerEmail = GetCustomerEmail(order);
                if (string.IsNullOrWhiteSpace(customerEmail)) return;

                var subject = $"[Thông báo] Đơn hàng của bạn đã được duyệt - Đơn hàng #{order.Order_code}";
                var body = BuildOrderConfirmedBodyCustomer(order);

                await SendEmailAsync(smtp!, customerEmail, subject, body, cancellationToken);
            }
            catch (System.Exception)
            {
                // Email gửi thất bại
            }
        }

        public async Task SendExportDeliveryStatusToCustomerAsync(int warehouseExportId, CancellationToken cancellationToken = default)
        {
            try
            {
                var export = await _warehouseExportRepository.GetByIdAsync(warehouseExportId);
                if (export == null || export.Order == null) return;

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                var customerEmail = GetCustomerEmail(export.Order);
                if (string.IsNullOrWhiteSpace(customerEmail)) return;

                var statusText = export.Delivery_status == "Delivered" ? "đã giao thành công" : "đang được giao đến bạn";
                var subject = $"[Vận chuyển] Đơn hàng #{export.Order.Order_code} {statusText}";
                var body = BuildDeliveryStatusBodyCustomer(export);

                await SendEmailAsync(smtp!, customerEmail, subject, body, cancellationToken);
            }
            catch
            {
                // Lỗi gửi email
            }
        }

        public async Task SendOrderWaitingCustomerNotificationAsync(int OrderId, CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(OrderId);
                if (order == null) return;

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                var customerEmail = GetCustomerEmail(order);
                if (string.IsNullOrWhiteSpace(customerEmail)) return;

                var subject = $"[Cần xác nhận] Đơn hàng #{order.Order_code} giao một phần";
                var body = BuildOrderWaitingCustomerBodyCustomer(order);

                await SendEmailAsync(smtp!, customerEmail, subject, body, cancellationToken);
            }
            catch (System.Exception)
            {
                // Email gửi thất bại
            }
        }

        public async Task SendCustomerInvoiceRequestNotificationAsync(int exportId, int invoiceCount, CancellationToken cancellationToken = default)
        {
            try
            {
                var export = await _warehouseExportRepository.GetByIdAsync(exportId);
                if (export == null) return;

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                var staffAdminEmails = await GetStaffAndAdminEmailsAsync(export.Customer_id);
                if (staffAdminEmails.Count == 0) return;

                var subject = $"[Yêu cầu xuất hóa đơn] Khách hàng yêu cầu HĐ cho phiếu {export.Warehouse_export_code}";
                var body = BuildCustomerInvoiceRequestNotificationBody(export, invoiceCount);

                foreach (var email in staffAdminEmails)
                {
                    if (string.IsNullOrWhiteSpace(email)) continue;
                    await SendEmailAsync(smtp!, email.Trim(), subject, body, cancellationToken);
                }
            }
            catch (System.Exception) { /* Bỏ qua lỗi gửi email */ }
        }

        private static bool IsSmtpValid(SmtpSettingsDto? smtp)
        {
            return smtp != null && !string.IsNullOrWhiteSpace(smtp.FromEmail);
        }

        private static string? GetCustomerEmail(Order order)
        {
            var user = order.Customer?.User;
            if (user == null || user.Is_deleted) return null;
            return string.IsNullOrWhiteSpace(user.Email) ? null : user.Email.Trim();
        }

        private async Task<HashSet<string>> GetStaffAndAdminEmailsAsync(int customerId)
        {
            var emails = new HashSet<string>();

            var adminRole = await _roleRepository.GetByNameAsync("ROLE_ADMIN");
            if (adminRole != null)
            {
                var adminUsers = await _userRepository.GetUsersByRoleAsync(adminRole.Id);
                foreach (var user in adminUsers)
                {
                    if (user.Is_active && !user.Is_deleted && !string.IsNullOrWhiteSpace(user.Email))
                        emails.Add(user.Email.Trim());
                }
            }

            var managementStaff = await _customerManagementRepository.GetByCustomerBasicAsync(customerId);
            if (managementStaff != null)
            {
                foreach (var mgmt in managementStaff)
                {
                    if (mgmt.Staff != null && !mgmt.Staff.Is_deleted)
                    {
                        var staffUser = await _userRepository.GetByIdBasicAsync(mgmt.Staff.User_id);
                        if (staffUser != null && staffUser.Is_active && !staffUser.Is_deleted && !string.IsNullOrWhiteSpace(staffUser.Email))
                            emails.Add(staffUser.Email.Trim());
                    }
                }
            }

            return emails;
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
            sb.Append("<h2>Xác nhận đơn hàng</h2>");
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

        private static string BuildOrderConfirmedBodyCustomer(Order order)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif;'>");
            sb.Append("<h2>Đơn hàng đã được duyệt</h2>");
            sb.Append("<p>Đơn hàng của bạn đã được duyệt và đang được xử lý.</p>");
            sb.Append($"<p><strong>Mã đơn:</strong> {WebUtility.HtmlEncode(order.Order_code)}</p>");
            sb.Append($"<p><strong>Ngày đặt:</strong> {order.Order_date:dd/MM/yyyy HH:mm}</p>");
            sb.Append($"<p><strong>Trạng thái:</strong> {WebUtility.HtmlEncode(order.Order_status ?? "Confirmed")}</p>");

            if (order.Customer != null)
            {
                sb.Append($"<p><strong>Khách hàng:</strong> {WebUtility.HtmlEncode(order.Customer.Full_name ?? order.Customer.Customer_code)}</p>");
                sb.Append($"<p><strong>Địa chỉ giao:</strong> {WebUtility.HtmlEncode(order.Shipping_address ?? order.Customer.Address ?? "")}</p>");
            }

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
            sb.Append("<p>Cảm ơn bạn đã đặt hàng!</p>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        private static string BuildOrderWaitingCustomerBodyCustomer(Order order)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif;'>");
            sb.Append("<h2>Cần xác nhận giao hàng một phần</h2>");
            sb.Append("<p>Kính gửi Quý khách,</p>");
            sb.Append("<p>Đơn hàng của quý khách hiện không đủ toàn bộ số lượng sản phẩm. Chúng tôi đã tách các sản phẩm thiếu sang một đơn chờ riêng.</p>");
            sb.Append("<p>Vui lòng đăng nhập vào hệ thống để <strong>Xác nhận</strong> quý khách có muốn nhận phần hàng có sẵn dưới đây trước hay không.</p>");
            sb.Append($"<p><strong>Mã đơn (phần có sẵn):</strong> {WebUtility.HtmlEncode(order.Order_code)}</p>");
            sb.Append($"<p><strong>Ngày đặt:</strong> {order.Order_date:dd/MM/yyyy HH:mm}</p>");

            if (order.Customer != null)
            {
                sb.Append($"<p><strong>Khách hàng:</strong> {WebUtility.HtmlEncode(order.Customer.Full_name ?? order.Customer.Customer_code)}</p>");
            }

            sb.Append("<h3>Chi tiết hàng có sẵn</h3>");
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
            sb.Append($"<p><strong>Tổng tiền phần này:</strong> {order.Total_price:N0} VNĐ</p>");
            sb.Append("<p>Trân trọng!</p>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        private static string BuildWarehouseExportNotificationBody(Warehouse_export export, string invoiceCodes)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif;'>");
            sb.Append("<h2>Phiếu xuất kho mới</h2>");
            sb.Append($"<p><strong>Mã phiếu xuất:</strong> {WebUtility.HtmlEncode(export.Warehouse_export_code)}</p>");
            sb.Append($"<p><strong>Mã đơn:</strong> {WebUtility.HtmlEncode(export.Order?.Order_code ?? "-")}</p>");
            sb.Append($"<p><strong>Mã hóa đơn:</strong> {WebUtility.HtmlEncode(string.IsNullOrEmpty(invoiceCodes) ? "-" : invoiceCodes)}</p>");
            sb.Append($"<p><strong>Kho:</strong> {WebUtility.HtmlEncode(export.Warehouse?.Warehouse_name ?? export.Warehouse_id.ToString())}</p>");
            sb.Append($"<p><strong>Khách hàng:</strong> {WebUtility.HtmlEncode(export.Customer?.Full_name ?? export.Customer?.Customer_code ?? "-")}</p>");
            sb.Append($"<p><strong>Ngày xuất:</strong> {export.Export_date:dd/MM/yyyy HH:mm}</p>");
            sb.Append($"<p><strong>Trạng thái vận chuyển:</strong> {WebUtility.HtmlEncode(export.Delivery_status ?? "Pending")}</p>");
            sb.Append($"<p><strong>Địa chỉ giao:</strong> {WebUtility.HtmlEncode(export.Order?.Shipping_address ?? export.Customer?.Address ?? "")}</p>");

            sb.Append("<h3>Chi tiết xuất kho</h3>");
            sb.Append("<table border='1' cellpadding='8' cellspacing='0' style='border-collapse: collapse; width: 100%;'>");
            sb.Append("<tr style='background: #f0f0f0;'><th>STT</th><th>Sản phẩm</th><th>SL xuất</th><th>Đơn giá</th><th>Thành tiền</th></tr>");

            var stt = 1;
            foreach (var detail in export.Warehouse_Export_Details ?? [])
            {
                var productName = detail.Product?.Product_name ?? $"Product #{detail.Product_id}";
                sb.Append($"<tr><td>{stt}</td><td>{WebUtility.HtmlEncode(productName)}</td><td>{detail.Quantity_shipped}</td><td>{detail.Unit_price:N0}</td><td>{detail.Total_price:N0}</td></tr>");
                stt++;
            }

            sb.Append("</table>");
            sb.Append("<p>Vui lòng xử lý vận chuyển theo phiếu xuất kho trên.</p>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        private static string BuildDeliveryStatusBodyCustomer(Warehouse_export export)
        {
            var sb = new StringBuilder();
            var statusText = export.Delivery_status == "Delivered" ? "ĐÃ GIAO THÀNH CÔNG" : "ĐANG ĐƯỢC VẬN CHUYỂN";
            
            sb.Append("<html><body style='font-family: Arial, sans-serif;'>");
            sb.Append($"<h2>Thông báo trạng thái giao hàng: {statusText}</h2>");
            sb.Append($"<p>Kính gửi Quý khách,</p>");
            sb.Append($"<p>Đơn hàng <strong>#{export.Order?.Order_code}</strong> của quý khách hiện có trạng thái vận chuyển: <strong>{export.Delivery_status}</strong>.</p>");
            sb.Append($"<p><strong>Mã phiếu xuất kho:</strong> {WebUtility.HtmlEncode(export.Warehouse_export_code)}</p>");
            
            sb.Append($"<p><strong>Địa chỉ nhận hàng:</strong> {WebUtility.HtmlEncode(export.Order?.Shipping_address ?? export.Customer?.Address ?? "")}</p>");
            sb.Append("<p>Cảm ơn quý khách đã tin tưởng và sử dụng dịch vụ của chúng tôi!</p>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        private static string BuildCustomerInvoiceRequestNotificationBody(Warehouse_export export, int invoiceCount)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif;'>");
            sb.Append("<h2>Yêu cầu xuất hóa đơn mới từ Khách hàng</h2>");
            sb.Append($"<p>Khách hàng <strong>{WebUtility.HtmlEncode(export.Customer?.Full_name ?? export.Customer?.Customer_code ?? "-")}</strong> vừa gửi yêu cầu xuất hóa đơn thông qua Cổng thông tin cho Phiếu xuất kho <strong>{WebUtility.HtmlEncode(export.Warehouse_export_code)}</strong>.</p>");
            sb.Append($"<p>Hệ thống đã tự động ghi nhận và tạo <strong>{invoiceCount}</strong> hóa đơn Nháp (Draft) tương ứng. Vui lòng đăng nhập vào phần Quản lý Hóa đơn để kiểm tra, bổ sung thuế (nếu cần) và duyệt xuất hóa đơn.</p>");
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
