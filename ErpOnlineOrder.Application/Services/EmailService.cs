using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ICustomerRepository _customerRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IProductRepository _productRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            ISettingService settingService,
            IOrderRepository orderRepository,
            IWarehouseExportRepository warehouseExportRepository,
            IRoleRepository roleRepository,
            IUserRepository userRepository,
            ICustomerManagementRepository customerManagementRepository,
            IInvoiceRepository invoiceRepository,
            ICustomerRepository customerRepository,
            IStaffRepository staffRepository,
            IProductRepository productRepository,
            IPackageRepository packageRepository,
            ILogger<EmailService> logger)
        {
            _settingService = settingService;
            _orderRepository = orderRepository;
            _warehouseExportRepository = warehouseExportRepository;
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _customerManagementRepository = customerManagementRepository;
            _invoiceRepository = invoiceRepository;
            _customerRepository = customerRepository;
            _staffRepository = staffRepository;
            _productRepository = productRepository;
            _packageRepository = packageRepository;
            _logger = logger;
        }

        public async Task SendWarehouseExportNotificationForStaffAndAdminAsync(int warehouseExportId, CancellationToken cancellationToken = default)
        {
            try
            {
                var export = await _warehouseExportRepository.GetByIdAsync(warehouseExportId);
                if (export == null) return;

                var customer = export.Customer ?? await _customerRepository.GetByIdAsync(export.Customer_id);

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
                var body = BuildWarehouseExportNotificationBody(export, invoiceCodes, customer);

                foreach (var email in staffAdminEmails)
                {
                    if (string.IsNullOrWhiteSpace(email)) continue;
                    await SendEmailAsync(smtp!, email.Trim(), subject, body, cancellationToken);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email thông báo phiếu xuất kho {ExportId}", warehouseExportId);
            }
        }

        public async Task SendOrderNotificationForStaffAndAdminAsync(int OrderId, CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(OrderId);
                if (order == null) return;

                var customer = order.Customer ?? await _customerRepository.GetByIdAsync(order.Customer_id);

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                var staffAdminEmails = await GetStaffAndAdminEmailsAsync(order.Customer_id);
                if (staffAdminEmails.Count == 0) return;

                var subject = $"[Thông báo] Đơn hàng mới đã được tạo - Đơn hàng #{order.Order_code}";
                var body = BuildOrderNotificationBodyForStaffAndAdmin(order, customer);

                foreach (var email in staffAdminEmails)
                {
                    if (string.IsNullOrWhiteSpace(email)) continue;
                    await SendEmailAsync(smtp!, email.Trim(), subject, body, cancellationToken);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email thông báo nhân viên/admin cho đơn hàng {OrderId}", OrderId);
            }
        }

        public async Task SendOrderNotificationForCustomerAsync(int OrderId, CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(OrderId);
                if (order == null) return;

                var customer = order.Customer ?? await _customerRepository.GetByIdAsync(order.Customer_id);

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                var customerEmail = await GetCustomerEmailAsync(order.Customer_id, customer);
                if (string.IsNullOrWhiteSpace(customerEmail)) return;

                var subject = $"[Xác nhận] Đơn hàng của bạn đã được tạo - Đơn hàng #{order.Order_code}";
                var body = BuildOrderNotificationBodyCustomer(order, customer);

                await SendEmailAsync(smtp!, customerEmail, subject, body, cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email thông báo tạo đơn hàng cho khách hàng (Đơn {OrderId})", OrderId);
            }
        }

        public async Task SendOrderConfirmedNotificationForCustomerAsync(int OrderId, CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(OrderId);
                if (order == null) return;

                var customer = order.Customer ?? await _customerRepository.GetByIdAsync(order.Customer_id);

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                var customerEmail = await GetCustomerEmailAsync(order.Customer_id, customer);
                if (string.IsNullOrWhiteSpace(customerEmail)) return;

                var subject = $"[Thông báo] Đơn hàng của bạn đã được duyệt - Đơn hàng #{order.Order_code}";
                var body = BuildOrderConfirmedBodyCustomer(order, customer);

                await SendEmailAsync(smtp!, customerEmail, subject, body, cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email xác nhận duyệt đơn hàng cho khách hàng (Đơn {OrderId})", OrderId);
            }
        }

        public async Task SendExportDeliveryStatusToCustomerAsync(int warehouseExportId, CancellationToken cancellationToken = default)
        {
            try
            {
                var export = await _warehouseExportRepository.GetByIdAsync(warehouseExportId);
                if (export == null) return;

                var customer = export.Customer ?? await _customerRepository.GetByIdAsync(export.Customer_id);

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                var customerEmail = await GetCustomerEmailAsync(export.Customer_id, customer);
                if (string.IsNullOrWhiteSpace(customerEmail)) return;

                var statusText = export.Delivery_status == "Delivered" ? "đã giao thành công" : "đang được giao đến bạn";
                var orderCodeText = export.Order != null ? $"Đơn hàng #{export.Order.Order_code}" : $"Phiếu xuất kho #{export.Warehouse_export_code}";
                var subject = $"[Vận chuyển] {orderCodeText} {statusText}";
                var body = BuildDeliveryStatusBodyCustomer(export, customer);

                await SendEmailAsync(smtp!, customerEmail, subject, body, cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email thông báo trạng thái vận chuyển cho phiếu xuất {ExportId}", warehouseExportId);
            }
        }

        public async Task SendOrderWaitingCustomerNotificationAsync(int OrderId, CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(OrderId);
                if (order == null) return;

                var customer = order.Customer ?? await _customerRepository.GetByIdAsync(order.Customer_id);

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                var customerEmail = await GetCustomerEmailAsync(order.Customer_id, customer);
                if (string.IsNullOrWhiteSpace(customerEmail)) return;

                var subject = $"[Cần xác nhận] Đơn hàng #{order.Order_code} giao một phần";
                var body = BuildOrderWaitingCustomerBodyCustomer(order, customer);

                await SendEmailAsync(smtp!, customerEmail, subject, body, cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email yêu cầu xác nhận đơn hàng chờ {OrderId}", OrderId);
            }
        }

        public async Task SendOrderUpdatedNotificationForCustomerAsync(int orderId, CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null) return;

                var customer = order.Customer ?? await _customerRepository.GetByIdAsync(order.Customer_id);

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                var customerEmail = await GetCustomerEmailAsync(order.Customer_id, customer);
                if (string.IsNullOrWhiteSpace(customerEmail)) return;

                var subject = $"[Thông báo] Đơn hàng #{order.Order_code} của bạn đã được cập nhật";
                var body = BuildOrderUpdatedBodyCustomer(order, customer);

                await SendEmailAsync(smtp!, customerEmail, subject, body, cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email thông báo cập nhật đơn hàng {OrderId}", orderId);
            }
        }

        public async Task SendOrderRejectedByCustomerNotificationAsync(int orderId, CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null) return;

                var customer = order.Customer ?? await _customerRepository.GetByIdAsync(order.Customer_id);

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                var staffAdminEmails = await GetStaffAndAdminEmailsAsync(order.Customer_id);
                if (staffAdminEmails.Count == 0) return;

                var subject = $"[Thông báo] Khách hàng từ chối nhận một phần - Đơn hàng #{order.Order_code}";
                var body = BuildOrderRejectedNotificationBody(order, customer);

                foreach (var email in staffAdminEmails)
                {
                    if (string.IsNullOrWhiteSpace(email)) continue;
                    await SendEmailAsync(smtp!, email.Trim(), subject, body, cancellationToken);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email thông báo khách hàng từ chối đơn {OrderId}", orderId);
            }
        }

        public async Task SendCustomerInvoiceRequestNotificationAsync(int exportId, int invoiceCount, CancellationToken cancellationToken = default)
        {
            try
            {
                var export = await _warehouseExportRepository.GetByIdAsync(exportId);
                if (export == null) return;

                var customer = export.Customer ?? await _customerRepository.GetByIdAsync(export.Customer_id);

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                var staffAdminEmails = await GetStaffAndAdminEmailsAsync(export.Customer_id);
                if (staffAdminEmails.Count == 0) return;

                var customerName = customer?.Full_name ?? customer?.Customer_code ?? "-";
                var subject = $"[Yêu cầu hóa đơn] Khách hàng {customerName} yêu cầu HĐ cho phiếu {export.Warehouse_export_code}";
                var body = BuildCustomerInvoiceRequestNotificationBody(export, invoiceCount, customer);

                foreach (var email in staffAdminEmails)
                {
                    if (string.IsNullOrWhiteSpace(email)) continue;
                    await SendEmailAsync(smtp!, email.Trim(), subject, body, cancellationToken);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email yêu cầu xuất hóa đơn cho phiếu {ExportId}", exportId);
            }
        }

        public async Task SendInvoiceToCustomerAsync(int invoiceId, CancellationToken cancellationToken = default)
        {
            try
            {
                var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
                if (invoice == null) return;

                var customer = await _customerRepository.GetByIdAsync(invoice.Customer_id);
                if (customer == null) return;

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                var customerEmail = await GetCustomerEmailAsync(invoice.Customer_id, customer);
                if (string.IsNullOrWhiteSpace(customerEmail)) return;

                var customerName = customer.Full_name ?? customer.Customer_code ?? "-";
                var subject = $"[Hóa đơn] Hóa đơn {invoice.Invoice_code} từ hệ thống";

                var sb = new StringBuilder();
                sb.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>");
                sb.AppendLine($"<h2 style='color: #1a7f37;'>Hóa đơn #{invoice.Invoice_code}</h2>");
                sb.AppendLine($"<p>Xin chào <strong>{customerName}</strong>,</p>");
                sb.AppendLine($"<p>Hệ thống gửi đến bạn hóa đơn <strong>{invoice.Invoice_code}</strong> ngày <strong>{invoice.Invoice_date:dd/MM/yyyy}</strong>.</p>");
                sb.AppendLine("<table style='width: 100%; border-collapse: collapse; margin: 16px 0;'>");
                sb.AppendLine("<thead><tr style='background: #f8f8f8;'>");
                sb.AppendLine("<th style='border: 1px solid #ddd; padding: 8px; text-align: left;'>Sản phẩm</th>");
                sb.AppendLine("<th style='border: 1px solid #ddd; padding: 8px; text-align: center;'>SL</th>");
                sb.AppendLine("<th style='border: 1px solid #ddd; padding: 8px; text-align: right;'>Đơn giá</th>");
                sb.AppendLine("<th style='border: 1px solid #ddd; padding: 8px; text-align: right;'>Thành tiền</th>");
                sb.AppendLine("</tr></thead><tbody>");

                if (invoice.Invoice_Details != null)
                {
                    foreach (var d in invoice.Invoice_Details)
                    {
                        var productName = d.Product?.Product_name ?? $"SP #{d.Product_id}";
                        sb.AppendLine($"<tr>");
                        sb.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{productName}</td>");
                        sb.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{d.Quantity}</td>");
                        sb.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px; text-align: right;'>{d.Unit_price:N0}</td>");
                        sb.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px; text-align: right;'>{d.Total_price:N0}</td>");
                        sb.AppendLine("</tr>");
                    }
                }

                sb.AppendLine("</tbody></table>");
                sb.AppendLine($"<p><strong>Tổng tiền hàng:</strong> {invoice.Total_amount:N0} VNĐ</p>");
                sb.AppendLine($"<p><strong>Thuế:</strong> {invoice.Tax_amount:N0} VNĐ</p>");
                sb.AppendLine($"<p style='font-size: 18px; color: #1a7f37;'><strong>Tổng thanh toán: {(invoice.Total_amount + invoice.Tax_amount):N0} VNĐ</strong></p>");
                sb.AppendLine("<hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />");
                sb.AppendLine("<p style='color: #888; font-size: 13px;'>Đây là email tự động từ hệ thống. Nếu có thắc mắc, vui lòng liên hệ bộ phận hỗ trợ.</p>");
                sb.AppendLine("</div>");

                await SendEmailAsync(smtp!, customerEmail, subject, sb.ToString(), cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi hóa đơn {InvoiceId} cho khách hàng", invoiceId);
            }
        }

        public async Task SendProductAssignedToCustomerAsync(int customerId, List<int> productIds, CancellationToken cancellationToken = default)
        {
            try
            {
                if (productIds == null || productIds.Count == 0) return;

                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null) return;

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                var customerEmail = await GetCustomerEmailAsync(customerId, customer);
                if (string.IsNullOrWhiteSpace(customerEmail)) return;

                var products = new List<Product>();
                foreach (var pid in productIds)
                {
                    var product = await _productRepository.GetByIdBasicAsync(pid);
                    if (product != null) products.Add(product);
                }
                if (products.Count == 0) return;

                var subject = products.Count == 1
                    ? $"[Thông báo] Bạn vừa được cấp quyền đặt sản phẩm: {products[0].Product_name}"
                    : $"[Thông báo] Bạn vừa được cấp quyền đặt {products.Count} sản phẩm mới";

                var body = BuildProductAssignedBody(customer, products);
                await SendEmailAsync(smtp!, customerEmail, subject, body, cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email thông báo gán sản phẩm cho khách hàng {CustomerId}", customerId);
            }
        }

        private static string BuildProductAssignedBody(Customer customer, List<Product> products)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>");
            sb.Append("<div style='background: #1a7f37; padding: 30px; text-align: center;'>");
            sb.Append("<h1 style='color: white; margin: 0;'>Sản phẩm mới được cấp phép</h1>");
            sb.Append("</div>");
            sb.Append("<div style='padding: 30px; background: #f9f9f9;'>");
            sb.Append($"<p>Xin chào <strong>{WebUtility.HtmlEncode(customer.Full_name ?? customer.Customer_code)}</strong>,</p>");
            if (products.Count == 1)
                sb.Append("<p>Bạn vừa được cấp quyền đặt hàng sản phẩm mới sau đây:</p>");
            else
                sb.Append($"<p>Bạn vừa được cấp quyền đặt hàng <strong>{products.Count}</strong> sản phẩm mới:</p>");
            sb.Append("<table style='border-collapse: collapse; width: 100%; margin-top: 15px;'>");
            sb.Append("<tr style='background: #1a7f37; color: white;'>");
            sb.Append("<th style='padding: 10px; text-align: left; border: 1px solid #dee2e6;'>Mã sản phẩm</th>");
            sb.Append("<th style='padding: 10px; text-align: left; border: 1px solid #dee2e6;'>Tên sản phẩm</th>");
            sb.Append("<th style='padding: 10px; text-align: right; border: 1px solid #dee2e6;'>Đơn giá</th>");
            sb.Append("</tr>");
            for (int i = 0; i < products.Count; i++)
            {
                var p = products[i];
                var rowBg = i % 2 == 0 ? "#ffffff" : "#f8f9fa";
                sb.Append($"<tr style='background: {rowBg};'>");
                sb.Append($"<td style='padding: 10px; border: 1px solid #dee2e6;'>{WebUtility.HtmlEncode(p.Product_code)}</td>");
                sb.Append($"<td style='padding: 10px; border: 1px solid #dee2e6;'>{WebUtility.HtmlEncode(p.Product_name)}</td>");
                sb.Append($"<td style='padding: 10px; border: 1px solid #dee2e6; text-align: right;'>{p.Product_price:N0} ₫</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table>");
            sb.Append("<br/><p>Bạn có thể đăng nhập hệ thống để xem và đặt hàng các sản phẩm này ngay bây giờ.</p>");
            sb.Append("<br/><p style='color: #888; font-size: 13px;'>Email này được gửi tự động, vui lòng không trả lời.</p>");
            sb.Append("</div>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        private static bool IsSmtpValid(SmtpSettingsDto? smtp)
        {
            return smtp != null && !string.IsNullOrWhiteSpace(smtp.FromEmail) && !string.IsNullOrWhiteSpace(smtp.Host);
        }

        private async Task<string?> GetCustomerEmailAsync(int customerId, Customer? loadedCustomer)
        {
            var customer = loadedCustomer ?? await _customerRepository.GetByIdAsync(customerId);
            if (customer == null) return null;

            var user = customer.User ?? await _userRepository.GetByIdBasicAsync(customer.User_id);
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
                    int? staffUserId = mgmt.Staff?.User_id;

                    if (staffUserId == null && mgmt.Staff_id > 0)
                    {
                        var staff = await _staffRepository.GetByIdAsync(mgmt.Staff_id);
                        if (staff != null && !staff.Is_deleted)
                        {
                            staffUserId = staff.User_id;
                        }
                    }

                    if (staffUserId.HasValue)
                    {
                        var staffUser = await _userRepository.GetByIdBasicAsync(staffUserId.Value);
                        if (staffUser != null && staffUser.Is_active && !staffUser.Is_deleted && !string.IsNullOrWhiteSpace(staffUser.Email))
                            emails.Add(staffUser.Email.Trim());
                    }
                }
            }

            return emails;
        }

        private static string TranslateOrderStatus(string? status)
        {
            return status switch
            {
                "Pending" => "Chờ xử lý",
                "Exporting" => "Đang xuất kho",
                "WaitingCustomer" => "Chờ khách xác nhận",
                "Completed" => "Đã hoàn thành",
                "Cancelled" => "Đã hủy",
                "Split" => "Đã tách đơn",
                _ => status ?? "Không xác định"
            };
        }

        private static string TranslateDeliveryStatus(string? status)
        {
            return status switch
            {
                "Pending" => "Chờ lấy hàng",
                "Shipped" => "Đang vận chuyển",
                "Delivered" => "Đã giao thành công",
                _ => status ?? "Chờ xử lý"
            };
        }

        private static string BuildOrderNotificationBodyForStaffAndAdmin(Order order, Customer? customer)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif;'>");
            sb.Append("<h2>Đơn hàng mới - Thông báo</h2>");
            sb.Append($"<p><strong>Mã đơn:</strong> {WebUtility.HtmlEncode(order.Order_code)}</p>");
            sb.Append($"<p><strong>Ngày đặt:</strong> {order.Order_date:dd/MM/yyyy HH:mm}</p>");
            sb.Append($"<p><strong>Trạng thái:</strong> {WebUtility.HtmlEncode(TranslateOrderStatus(order.Order_status))}</p>");

            if (customer != null)
            {
                sb.Append($"<p><strong>Khách hàng:</strong> {WebUtility.HtmlEncode(customer.Full_name ?? customer.Customer_code)}</p>");
                sb.Append($"<p><strong>Điện thoại:</strong> {WebUtility.HtmlEncode(customer.Phone_number ?? "")}</p>");
                sb.Append($"<p><strong>Địa chỉ giao:</strong> {WebUtility.HtmlEncode(order.Shipping_address ?? customer.Address ?? "")}</p>");
            }

            if (!string.IsNullOrWhiteSpace(order.note))
                sb.Append($"<p><strong>Ghi chú:</strong> {WebUtility.HtmlEncode(order.note)}</p>");

            sb.Append("<h3>Chi tiết đơn hàng</h3>");
            sb.Append("<table border='1' cellpadding='8' cellspacing='0' style='border-collapse: collapse; width: 100%;'>");
            sb.Append("<tr style='background: #f0f0f0;'><th>STT</th><th>Sản phẩm</th><th>SL</th><th>Đơn giá</th><th>Thành tiền</th></tr>");

            int stt = 1;
            foreach (var od in order.Order_Details?.Where(d => !d.Is_deleted) ?? Enumerable.Empty<Order_detail>())
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

        private static string BuildOrderNotificationBodyCustomer(Order order, Customer? customer)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif;'>");
            sb.Append("<h2>Xác nhận đơn hàng</h2>");
            sb.Append($"<p><strong>Mã đơn:</strong> {WebUtility.HtmlEncode(order.Order_code)}</p>");
            sb.Append($"<p><strong>Ngày đặt:</strong> {order.Order_date:dd/MM/yyyy HH:mm}</p>");
            sb.Append($"<p><strong>Trạng thái:</strong> {WebUtility.HtmlEncode(TranslateOrderStatus(order.Order_status))}</p>");

            if (customer != null)
            {
                sb.Append($"<p><strong>Khách hàng:</strong> {WebUtility.HtmlEncode(customer.Full_name ?? customer.Customer_code)}</p>");
                sb.Append($"<p><strong>Điện thoại:</strong> {WebUtility.HtmlEncode(customer.Phone_number ?? "")}</p>");
                sb.Append($"<p><strong>Địa chỉ giao:</strong> {WebUtility.HtmlEncode(order.Shipping_address ?? customer.Address ?? "")}</p>");
            }

            if (!string.IsNullOrWhiteSpace(order.note))
                sb.Append($"<p><strong>Ghi chú:</strong> {WebUtility.HtmlEncode(order.note)}</p>");

            sb.Append("<h3>Chi tiết đơn hàng</h3>");
            sb.Append("<table border='1' cellpadding='8' cellspacing='0' style='border-collapse: collapse; width: 100%;'>");
            sb.Append("<tr style='background: #f0f0f0;'><th>STT</th><th>Sản phẩm</th><th>SL</th><th>Đơn giá</th><th>Thành tiền</th></tr>");

            int stt = 1;
            foreach (var od in order.Order_Details?.Where(d => !d.Is_deleted) ?? Enumerable.Empty<Order_detail>())
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

        private static string BuildOrderConfirmedBodyCustomer(Order order, Customer? customer)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif;'>");
            sb.Append("<h2>Đơn hàng đã được duyệt</h2>");
            sb.Append("<p>Đơn hàng của bạn đã được duyệt và đang được xử lý.</p>");
            sb.Append($"<p><strong>Mã đơn:</strong> {WebUtility.HtmlEncode(order.Order_code)}</p>");
            sb.Append($"<p><strong>Ngày đặt:</strong> {order.Order_date:dd/MM/yyyy HH:mm}</p>");
            sb.Append($"<p><strong>Trạng thái:</strong> {WebUtility.HtmlEncode(TranslateOrderStatus(order.Order_status))}</p>");

            if (customer != null)
            {
                sb.Append($"<p><strong>Khách hàng:</strong> {WebUtility.HtmlEncode(customer.Full_name ?? customer.Customer_code)}</p>");
                sb.Append($"<p><strong>Địa chỉ giao:</strong> {WebUtility.HtmlEncode(order.Shipping_address ?? customer.Address ?? "")}</p>");
            }

            sb.Append("<h3>Chi tiết đơn hàng</h3>");
            sb.Append("<table border='1' cellpadding='8' cellspacing='0' style='border-collapse: collapse; width: 100%;'>");
            sb.Append("<tr style='background: #f0f0f0;'><th>STT</th><th>Sản phẩm</th><th>SL</th><th>Đơn giá</th><th>Thành tiền</th></tr>");

            int stt = 1;
            foreach (var od in order.Order_Details?.Where(d => !d.Is_deleted) ?? Enumerable.Empty<Order_detail>())
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

        private static string BuildOrderWaitingCustomerBodyCustomer(Order order, Customer? customer)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif;'>");
            sb.Append("<h2>Cần xác nhận giao hàng một phần</h2>");
            sb.Append("<p>Kính gửi Quý khách,</p>");
            sb.Append("<p>Đơn hàng của quý khách hiện không đủ toàn bộ số lượng sản phẩm. Chúng tôi đã tách các sản phẩm thiếu sang một đơn chờ riêng.</p>");
            sb.Append("<p>Vui lòng đăng nhập vào hệ thống để <strong>Xác nhận</strong> quý khách có muốn nhận phần hàng có sẵn dưới đây trước hay không.</p>");
            sb.Append($"<p><strong>Mã đơn (phần có sẵn):</strong> {WebUtility.HtmlEncode(order.Order_code)}</p>");
            sb.Append($"<p><strong>Ngày đặt:</strong> {order.Order_date:dd/MM/yyyy HH:mm}</p>");

            if (customer != null)
            {
                sb.Append($"<p><strong>Khách hàng:</strong> {WebUtility.HtmlEncode(customer.Full_name ?? customer.Customer_code)}</p>");
            }

            sb.Append("<h3>Chi tiết hàng có sẵn</h3>");
            sb.Append("<table border='1' cellpadding='8' cellspacing='0' style='border-collapse: collapse; width: 100%;'>");
            sb.Append("<tr style='background: #f0f0f0;'><th>STT</th><th>Sản phẩm</th><th>SL</th><th>Đơn giá</th><th>Thành tiền</th></tr>");

            int stt = 1;
            foreach (var od in order.Order_Details?.Where(d => !d.Is_deleted) ?? Enumerable.Empty<Order_detail>())
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

        private static string BuildOrderUpdatedBodyCustomer(Order order, Customer? customer)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif;'>");
            sb.Append("<h2>Thông báo cập nhật đơn hàng</h2>");
            sb.Append("<p>Kính gửi Quý khách,</p>");
            sb.Append($"<p>Đơn hàng <strong>#{WebUtility.HtmlEncode(order.Order_code)}</strong> của quý khách vừa được nhân viên cập nhật thông tin.</p>");
            sb.Append($"<p><strong>Ngày đặt:</strong> {order.Order_date:dd/MM/yyyy HH:mm}</p>");
            sb.Append($"<p><strong>Trạng thái:</strong> {WebUtility.HtmlEncode(TranslateOrderStatus(order.Order_status))}</p>");

            if (customer != null)
            {
                sb.Append($"<p><strong>Địa chỉ giao:</strong> {WebUtility.HtmlEncode(order.Shipping_address ?? customer.Address ?? "")}</p>");
            }

            if (!string.IsNullOrWhiteSpace(order.note))
                sb.Append($"<p><strong>Ghi chú:</strong> {WebUtility.HtmlEncode(order.note)}</p>");

            sb.Append("<h3>Chi tiết đơn hàng cập nhật</h3>");
            sb.Append("<table border='1' cellpadding='8' cellspacing='0' style='border-collapse: collapse; width: 100%;'>");
            sb.Append("<tr style='background: #f0f0f0;'><th>STT</th><th>Sản phẩm</th><th>SL</th><th>Đơn giá</th><th>Thành tiền</th></tr>");

            int stt = 1;
            foreach (var od in order.Order_Details?.Where(d => !d.Is_deleted) ?? Enumerable.Empty<Order_detail>())
            {
                var productName = od.Product?.Product_name ?? $"Product #{od.Product_id}";
                sb.Append($"<tr><td>{stt}</td><td>{WebUtility.HtmlEncode(productName)}</td><td>{od.Quantity}</td><td>{od.Unit_price:N0}</td><td>{od.Total_price:N0}</td></tr>");
                stt++;
            }

            sb.Append("</table>");
            sb.Append($"<p><strong>Tổng tiền:</strong> {order.Total_price:N0} VNĐ</p>");
            sb.Append("<p>Cảm ơn quý khách!</p>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        private static string BuildOrderRejectedNotificationBody(Order order, Customer? customer)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif;'>");
            sb.Append("<h2>Khách hàng từ chối nhận hàng một phần</h2>");
            sb.Append($"<p>Khách hàng <strong>{WebUtility.HtmlEncode(customer?.Full_name ?? customer?.Customer_code ?? "")}</strong> đã <strong>TỪ CHỐI</strong> nhận trước phần hàng có sẵn cho đơn hàng <strong>{WebUtility.HtmlEncode(order.Order_code)}</strong>.</p>");
            sb.Append("<p>Hệ thống đã tự động hoàn tác việc tách đơn và khôi phục đơn hàng gốc về trạng thái <strong>Chờ xử lý (Pending)</strong>.</p>");
            sb.Append("<p>Vui lòng kiểm tra lại đơn hàng và có phương án xử lý (nhập thêm tồn kho hoặc liên hệ lại với khách hàng).</p>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        private static string BuildWarehouseExportNotificationBody(Warehouse_export export, string invoiceCodes, Customer? customer)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif;'>");
            sb.Append("<h2>Phiếu xuất kho mới</h2>");
            sb.Append($"<p><strong>Mã phiếu xuất:</strong> {WebUtility.HtmlEncode(export.Warehouse_export_code)}</p>");
            sb.Append($"<p><strong>Mã đơn:</strong> {WebUtility.HtmlEncode(export.Order?.Order_code ?? "-")}</p>");
            sb.Append($"<p><strong>Mã hóa đơn:</strong> {WebUtility.HtmlEncode(string.IsNullOrEmpty(invoiceCodes) ? "-" : invoiceCodes)}</p>");
            sb.Append($"<p><strong>Kho:</strong> {WebUtility.HtmlEncode(export.Warehouse?.Warehouse_name ?? export.Warehouse_id.ToString())}</p>");
            sb.Append($"<p><strong>Khách hàng:</strong> {WebUtility.HtmlEncode(customer?.Full_name ?? customer?.Customer_code ?? "-")}</p>");
            sb.Append($"<p><strong>Ngày xuất:</strong> {export.Export_date:dd/MM/yyyy HH:mm}</p>");
            sb.Append($"<p><strong>Trạng thái vận chuyển:</strong> {WebUtility.HtmlEncode(TranslateDeliveryStatus(export.Delivery_status))}</p>");
            sb.Append($"<p><strong>Địa chỉ giao:</strong> {WebUtility.HtmlEncode(export.Order?.Shipping_address ?? customer?.Address ?? "")}</p>");

            sb.Append("<h3>Chi tiết xuất kho</h3>");
            sb.Append("<table border='1' cellpadding='8' cellspacing='0' style='border-collapse: collapse; width: 100%;'>");
            sb.Append("<tr style='background: #f0f0f0;'><th>STT</th><th>Sản phẩm</th><th>SL xuất</th><th>Đơn giá</th><th>Thành tiền</th></tr>");

            var stt = 1;
            foreach (var detail in export.Warehouse_Export_Details?.Where(d => !d.Is_deleted) ?? Enumerable.Empty<Warehouse_export_detail>())
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

        private static string BuildDeliveryStatusBodyCustomer(Warehouse_export export, Customer? customer)
        {
            var sb = new StringBuilder();
            var statusText = export.Delivery_status == "Delivered" ? "ĐÃ GIAO THÀNH CÔNG" : "ĐANG ĐƯỢC VẬN CHUYỂN";
            
            sb.Append("<html><body style='font-family: Arial, sans-serif;'>");
            sb.Append($"<h2>Thông báo trạng thái giao hàng: {statusText}</h2>");
            sb.Append($"<p>Kính gửi Quý khách,</p>");
            if (export.Order != null)
            {
                sb.Append($"<p>Đơn hàng <strong>#{export.Order.Order_code}</strong> của quý khách hiện có trạng thái vận chuyển: <strong>{TranslateDeliveryStatus(export.Delivery_status)}</strong>.</p>");
            }
            else
            {
                sb.Append($"<p>Các sản phẩm của quý khách hiện có trạng thái vận chuyển: <strong>{TranslateDeliveryStatus(export.Delivery_status)}</strong>.</p>");
            }
            sb.Append($"<p><strong>Mã phiếu xuất kho:</strong> {WebUtility.HtmlEncode(export.Warehouse_export_code)}</p>");
            
            sb.Append($"<p><strong>Địa chỉ nhận hàng:</strong> {WebUtility.HtmlEncode(export.Order?.Shipping_address ?? customer?.Address ?? "")}</p>");
            sb.Append("<p>Cảm ơn quý khách đã tin tưởng và sử dụng dịch vụ của chúng tôi!</p>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        private static string BuildCustomerInvoiceRequestNotificationBody(Warehouse_export export, int invoiceCount, Customer? customer)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif;'>");
            sb.Append("<h2>Yêu cầu xuất hóa đơn mới từ Khách hàng</h2>");
            sb.Append($"<p>Khách hàng <strong>{WebUtility.HtmlEncode(customer?.Full_name ?? customer?.Customer_code ?? "-")}</strong> vừa gửi yêu cầu xuất hóa đơn thông qua Cổng thông tin cho Phiếu xuất kho <strong>{WebUtility.HtmlEncode(export.Warehouse_export_code)}</strong>.</p>");
            sb.Append($"<p>Hệ thống đã tự động ghi nhận và tạo <strong>{invoiceCount}</strong> hóa đơn Nháp (Draft) tương ứng. Vui lòng đăng nhập vào phần Quản lý Hóa đơn để kiểm tra, bổ sung thuế (nếu cần) và duyệt xuất hóa đơn.</p>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        public async Task SendCustomerRegistrationNotificationAsync(int customerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null) return;

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                // Load cán bộ được phân công (nếu có)
                Staff? assignedStaff = null;
                var mgmtList = await _customerManagementRepository.GetByCustomerBasicAsync(customerId);
                var mgmt = mgmtList?.FirstOrDefault();
                if (mgmt != null)
                    assignedStaff = mgmt.Staff ?? await _staffRepository.GetByIdAsync(mgmt.Staff_id);

                // 1. Email chào mừng đến khách hàng (có thông tin CB phụ trách)
                var customerEmail = await GetCustomerEmailAsync(customerId, customer);
                if (!string.IsNullOrWhiteSpace(customerEmail))
                {
                    var welcomeSubject = "Chào mừng bạn đến với hệ thống - Đăng ký thành công!";
                    var welcomeBody = BuildRegistrationWelcomeBody(customer, assignedStaff);
                    await SendEmailAsync(smtp!, customerEmail, welcomeSubject, welcomeBody, cancellationToken);
                }

                // 2. Email riêng cho cán bộ được phân công
                if (assignedStaff != null)
                {
                    var staffUser = await _userRepository.GetByIdBasicAsync(assignedStaff.User_id);
                    if (staffUser != null && staffUser.Is_active && !staffUser.Is_deleted && !string.IsNullOrWhiteSpace(staffUser.Email))
                    {
                        var staffSubject = $"[Phân công] Khách hàng mới: {customer.Full_name ?? customer.Customer_code}";
                        var staffBody = BuildStaffAssignedNotificationBody(customer, assignedStaff);
                        await SendEmailAsync(smtp!, staffUser.Email.Trim(), staffSubject, staffBody, cancellationToken);
                    }
                }

                // 3. Thông báo cho admin
                var adminRole = await _roleRepository.GetByNameAsync("ROLE_ADMIN");
                if (adminRole != null)
                {
                    var adminUsers = await _userRepository.GetUsersByRoleAsync(adminRole.Id);
                    var adminSubject = $"[Khách hàng mới] {customer.Full_name ?? customer.Customer_code} vừa đăng ký";
                    var adminBody = BuildNewCustomerNotificationBody(customer, assignedStaff);
                    foreach (var user in adminUsers)
                    {
                        if (user.Is_active && !user.Is_deleted && !string.IsNullOrWhiteSpace(user.Email))
                            await SendEmailAsync(smtp!, user.Email.Trim(), adminSubject, adminBody, cancellationToken);
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email thông báo đăng ký cho khách hàng {CustomerId}", customerId);
            }
        }

        private static string BuildRegistrationWelcomeBody(Customer customer, Staff? assignedStaff)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>");
            sb.Append("<div style='background: #1a7f37; padding: 30px; text-align: center;'>");
            sb.Append("<h1 style='color: white; margin: 0;'>Đăng ký thành công!</h1>");
            sb.Append("</div>");
            sb.Append("<div style='padding: 30px; background: #f9f9f9;'>");
            sb.Append($"<p>Xin chào <strong>{WebUtility.HtmlEncode(customer.Full_name ?? customer.Customer_code)}</strong>,</p>");
            sb.Append("<p>Tài khoản của bạn đã được tạo thành công trên hệ thống. Bạn có thể đăng nhập và bắt đầu đặt hàng ngay bây giờ.</p>");
            sb.Append($"<p><strong>Mã khách hàng:</strong> {WebUtility.HtmlEncode(customer.Customer_code)}</p>");
            if (!string.IsNullOrWhiteSpace(customer.Phone_number))
                sb.Append($"<p><strong>Số điện thoại:</strong> {WebUtility.HtmlEncode(customer.Phone_number)}</p>");
            if (assignedStaff != null)
            {
                sb.Append("<hr style='border: 1px solid #ddd; margin: 20px 0;'/>");
                sb.Append("<h3 style='color: #1a7f37; margin-top: 0;'>✅ Cán bộ phụ trách của bạn</h3>");
                sb.Append("<table style='border-collapse: collapse; width: 100%;'>");
                sb.Append($"<tr><td style='padding: 8px; width: 40%;'><strong>Họ tên:</strong></td><td style='padding: 8px;'>{WebUtility.HtmlEncode(assignedStaff.Full_name ?? "-")}</td></tr>");
                sb.Append($"<tr><td style='padding: 8px;'><strong>Mã cán bộ:</strong></td><td style='padding: 8px;'>{WebUtility.HtmlEncode(assignedStaff.Staff_code ?? "-")}</td></tr>");
                if (!string.IsNullOrWhiteSpace(assignedStaff.Phone_number))
                    sb.Append($"<tr><td style='padding: 8px;'><strong>Số điện thoại:</strong></td><td style='padding: 8px;'>{WebUtility.HtmlEncode(assignedStaff.Phone_number)}</td></tr>");
                sb.Append("</table>");
                sb.Append("<p>Cán bộ phụ trách sẽ liên hệ với bạn trong thời gian sớm nhất để hỗ trợ.</p>");
            }
            else
            {
                sb.Append("<p>Nếu cần hỗ trợ, hãy liên hệ với chúng tôi qua hotline hoặc email.</p>");
            }
            sb.Append("<br/><p style='color: #888; font-size: 13px;'>Email này được gửi tự động, vui lòng không trả lời.</p>");
            sb.Append("</div>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        private static string BuildStaffAssignedNotificationBody(Customer customer, Staff staff)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>");
            sb.Append("<div style='background: #0d6efd; padding: 30px; text-align: center;'>");
            sb.Append("<h1 style='color: white; margin: 0;'>Bạn được phân công khách hàng mới</h1>");
            sb.Append("</div>");
            sb.Append("<div style='padding: 30px; background: #f9f9f9;'>");
            sb.Append($"<p>Xin chào <strong>{WebUtility.HtmlEncode(staff.Full_name ?? "-")}</strong>,</p>");
            sb.Append("<p>Bạn vừa được hệ thống tự động phân công phụ trách khách hàng mới đã đăng ký:</p>");
            sb.Append("<table style='border-collapse: collapse; width: 100%; margin-top: 15px;'>");
            sb.Append($"<tr style='background:#e9ecef;'><td style='padding:10px; border:1px solid #dee2e6; width:40%;'><strong>Họ tên</strong></td><td style='padding:10px; border:1px solid #dee2e6;'>{WebUtility.HtmlEncode(customer.Full_name ?? "-")}</td></tr>");
            sb.Append($"<tr><td style='padding:10px; border:1px solid #dee2e6;'><strong>Mã khách hàng</strong></td><td style='padding:10px; border:1px solid #dee2e6;'>{WebUtility.HtmlEncode(customer.Customer_code)}</td></tr>");
            if (!string.IsNullOrWhiteSpace(customer.Phone_number))
                sb.Append($"<tr style='background:#f8f9fa;'><td style='padding:10px; border:1px solid #dee2e6;'><strong>Điện thoại</strong></td><td style='padding:10px; border:1px solid #dee2e6;'>{WebUtility.HtmlEncode(customer.Phone_number)}</td></tr>");
            if (!string.IsNullOrWhiteSpace(customer.Address))
                sb.Append($"<tr><td style='padding:10px; border:1px solid #dee2e6;'><strong>Địa chỉ</strong></td><td style='padding:10px; border:1px solid #dee2e6;'>{WebUtility.HtmlEncode(customer.Address)}</td></tr>");
            sb.Append($"<tr style='background:#f8f9fa;'><td style='padding:10px; border:1px solid #dee2e6;'><strong>Thời gian đăng ký</strong></td><td style='padding:10px; border:1px solid #dee2e6;'>{customer.Created_at:dd/MM/yyyy HH:mm}</td></tr>");
            sb.Append("</table>");
            sb.Append("<br/><p>Vui lòng đăng nhập hệ thống để xem thông tin chi tiết và liên hệ với khách hàng.</p>");
            sb.Append("<br/><p style='color: #888; font-size: 13px;'>Email này được gửi tự động, vui lòng không trả lời.</p>");
            sb.Append("</div>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        private static string BuildNewCustomerNotificationBody(Customer customer, Staff? assignedStaff)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif;'>");
            sb.Append("<h2>Khách hàng mới vừa đăng ký</h2>");
            sb.Append($"<p><strong>Họ tên:</strong> {WebUtility.HtmlEncode(customer.Full_name ?? "-")}</p>");
            sb.Append($"<p><strong>Mã khách hàng:</strong> {WebUtility.HtmlEncode(customer.Customer_code)}</p>");
            if (!string.IsNullOrWhiteSpace(customer.Phone_number))
                sb.Append($"<p><strong>Số điện thoại:</strong> {WebUtility.HtmlEncode(customer.Phone_number)}</p>");
            if (!string.IsNullOrWhiteSpace(customer.Address))
                sb.Append($"<p><strong>Địa chỉ:</strong> {WebUtility.HtmlEncode(customer.Address)}</p>");
            sb.Append($"<p><strong>Thời gian đăng ký:</strong> {customer.Created_at:dd/MM/yyyy HH:mm}</p>");
            if (assignedStaff != null)
                sb.Append($"<p><strong>Cán bộ phụ trách:</strong> {WebUtility.HtmlEncode(assignedStaff.Full_name ?? "-")} ({WebUtility.HtmlEncode(assignedStaff.Staff_code ?? "-")})</p>");
            else
                sb.Append("<p><strong>Cán bộ phụ trách:</strong> <em style='color:#dc3545;'>Chưa được phân công (không có quy tắc vùng phù hợp)</em></p>");
            sb.Append("<p>Vui lòng đăng nhập hệ thống để xem thông tin chi tiết và liên hệ với khách hàng.</p>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        public async Task SendStaffAssignmentNotificationAsync(int customerId, int staffId, CancellationToken cancellationToken = default)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null) return;

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                var staff = await _staffRepository.GetByIdAsync(staffId);

                // Gửi thông báo đến khách hàng
                var customerEmail = await GetCustomerEmailAsync(customerId, customer);
                if (!string.IsNullOrWhiteSpace(customerEmail))
                {
                    var subject = $"[Thông báo] Bạn đã được phân công cán bộ phụ trách";
                    var body = BuildStaffAssignmentBodyForCustomer(customer, staff);
                    await SendEmailAsync(smtp!, customerEmail, subject, body, cancellationToken);
                }

                // Gửi thông báo đến cán bộ được gán
                if (staff?.User_id != null)
                {
                    var staffUser = await _userRepository.GetByIdBasicAsync(staff.User_id);
                    if (staffUser != null && staffUser.Is_active && !staffUser.Is_deleted && !string.IsNullOrWhiteSpace(staffUser.Email))
                    {
                        var subject = $"[Phân công] Bạn được gán phụ trách khách hàng {WebUtility.HtmlEncode(customer.Full_name ?? customer.Customer_code)}";
                        var body = BuildStaffAssignmentBodyForStaff(customer, staff);
                        await SendEmailAsync(smtp!, staffUser.Email.Trim(), subject, body, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email thông báo gán cán bộ cho khách hàng {CustomerId}", customerId);
            }
        }

        private static string BuildStaffAssignmentBodyForCustomer(Customer customer, Staff? staff)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>");
            sb.Append("<div style='background: #198754; padding: 30px; text-align: center;'>");
            sb.Append("<h1 style='color: white; margin: 0;'>Thông báo phân công cán bộ phụ trách</h1>");
            sb.Append("</div>");
            sb.Append("<div style='padding: 30px; background: #f9f9f9;'>");
            sb.Append($"<p>Xin chào <strong>{WebUtility.HtmlEncode(customer.Full_name ?? customer.Customer_code)}</strong>,</p>");
            sb.Append("<p>Bạn đã được phân công cán bộ phụ trách mới cho tài khoản của mình:</p>");
            sb.Append("<table style='border-collapse: collapse; width: 100%; margin: 16px 0;'>");
            sb.Append("<tr style='background: #e9ecef;'><th style='padding: 10px; text-align: left; border: 1px solid #dee2e6;'>Thông tin</th><th style='padding: 10px; text-align: left; border: 1px solid #dee2e6;'>Chi tiết</th></tr>");
            sb.Append($"<tr><td style='padding: 10px; border: 1px solid #dee2e6;'>Cán bộ phụ trách</td><td style='padding: 10px; border: 1px solid #dee2e6;'><strong style='color:#198754;'>{WebUtility.HtmlEncode(staff?.Full_name ?? "N/A")}</strong></td></tr>");
            if (!string.IsNullOrWhiteSpace(staff?.Staff_code))
                sb.Append($"<tr><td style='padding: 10px; border: 1px solid #dee2e6;'>Mã cán bộ</td><td style='padding: 10px; border: 1px solid #dee2e6;'>{WebUtility.HtmlEncode(staff.Staff_code)}</td></tr>");
            sb.Append("</table>");
            sb.Append("<p>Cán bộ phụ trách sẽ hỗ trợ bạn trong quá trình sử dụng dịch vụ. Vui lòng liên hệ nếu cần hỗ trợ.</p>");
            sb.Append("</div></body></html>");
            return sb.ToString();
        }

        private static string BuildStaffAssignmentBodyForStaff(Customer customer, Staff staff)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>");
            sb.Append("<div style='background: #0d6efd; padding: 30px; text-align: center;'>");
            sb.Append("<h1 style='color: white; margin: 0;'>Thông báo phân công khách hàng mới</h1>");
            sb.Append("</div>");
            sb.Append("<div style='padding: 30px; background: #f9f9f9;'>");
            sb.Append($"<p>Xin chào <strong>{WebUtility.HtmlEncode(staff.Full_name ?? staff.Staff_code)}</strong>,</p>");
            sb.Append("<p>Bạn vừa được gán phụ trách khách hàng sau:</p>");
            sb.Append("<table style='border-collapse: collapse; width: 100%; margin: 16px 0;'>");
            sb.Append("<tr style='background: #e9ecef;'><th style='padding: 10px; text-align: left; border: 1px solid #dee2e6;'>Thông tin</th><th style='padding: 10px; text-align: left; border: 1px solid #dee2e6;'>Chi tiết</th></tr>");
            sb.Append($"<tr><td style='padding: 10px; border: 1px solid #dee2e6;'>Tên khách hàng</td><td style='padding: 10px; border: 1px solid #dee2e6;'><strong>{WebUtility.HtmlEncode(customer.Full_name ?? customer.Customer_code)}</strong></td></tr>");
            sb.Append($"<tr><td style='padding: 10px; border: 1px solid #dee2e6;'>Mã khách hàng</td><td style='padding: 10px; border: 1px solid #dee2e6;'>{WebUtility.HtmlEncode(customer.Customer_code ?? "")}</td></tr>");
            if (!string.IsNullOrWhiteSpace(customer.Phone_number))
                sb.Append($"<tr><td style='padding: 10px; border: 1px solid #dee2e6;'>Số điện thoại</td><td style='padding: 10px; border: 1px solid #dee2e6;'>{WebUtility.HtmlEncode(customer.Phone_number)}</td></tr>");
            sb.Append("</table>");
            sb.Append("<p>Vui lòng đăng nhập hệ thống để xem thông tin chi tiết và liên hệ với khách hàng.</p>");
            sb.Append("</div></body></html>");
            return sb.ToString();
        }

        public async Task SendStaffReplacementNotificationAsync(int customerId, int oldStaffId, int newStaffId, CancellationToken cancellationToken = default)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null) return;

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                var oldStaff = await _staffRepository.GetByIdAsync(oldStaffId);
                var newStaff = await _staffRepository.GetByIdAsync(newStaffId);

                // Gửi thông báo đến khách hàng
                var customerEmail = await GetCustomerEmailAsync(customerId, customer);
                if (!string.IsNullOrWhiteSpace(customerEmail))
                {
                    var subject = $"[Thông báo] Cán bộ phụ trách của bạn đã thay đổi";
                    var body = BuildStaffReplacementBodyForCustomer(customer, oldStaff, newStaff);
                    await SendEmailAsync(smtp!, customerEmail, subject, body, cancellationToken);
                }

                // Gửi thông báo đến admin và cán bộ mới
                var adminEmails = new HashSet<string>();
                var adminRole = await _roleRepository.GetByNameAsync("ROLE_ADMIN");
                if (adminRole != null)
                {
                    var adminUsers = await _userRepository.GetUsersByRoleAsync(adminRole.Id);
                    foreach (var user in adminUsers)
                    {
                        if (user.Is_active && !user.Is_deleted && !string.IsNullOrWhiteSpace(user.Email))
                            adminEmails.Add(user.Email.Trim());
                    }
                }

                if (newStaff?.User_id != null)
                {
                    var newStaffUser = await _userRepository.GetByIdBasicAsync(newStaff.User_id);
                    if (newStaffUser != null && newStaffUser.Is_active && !newStaffUser.Is_deleted && !string.IsNullOrWhiteSpace(newStaffUser.Email))
                        adminEmails.Add(newStaffUser.Email.Trim());
                }

                if (adminEmails.Count > 0)
                {
                    var notifySubject = $"[Thay thế] Cán bộ phụ trách khách hàng {customer.Full_name ?? customer.Customer_code} đã được thay đổi";
                    var notifyBody = BuildStaffReplacementBodyForAdmin(customer, oldStaff, newStaff);
                    foreach (var email in adminEmails)
                    {
                        if (!string.IsNullOrWhiteSpace(email))
                            await SendEmailAsync(smtp!, email, notifySubject, notifyBody, cancellationToken);
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email thông báo thay thế cán bộ cho khách hàng {CustomerId}", customerId);
            }
        }

        private static string BuildStaffReplacementBodyForCustomer(Customer customer, Staff? oldStaff, Staff? newStaff)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>");
            sb.Append("<div style='background: #0d6efd; padding: 30px; text-align: center;'>");
            sb.Append("<h1 style='color: white; margin: 0;'>Thông báo thay đổi cán bộ phụ trách</h1>");
            sb.Append("</div>");
            sb.Append("<div style='padding: 30px; background: #f9f9f9;'>");
            sb.Append($"<p>Xin chào <strong>{WebUtility.HtmlEncode(customer.Full_name ?? customer.Customer_code)}</strong>,</p>");
            sb.Append("<p>Chúng tôi thông báo cán bộ phụ trách tài khoản của bạn đã được thay đổi:</p>");
            sb.Append("<table style='border-collapse: collapse; width: 100%; margin: 16px 0;'>");
            sb.Append("<tr style='background: #e9ecef;'><th style='padding: 10px; text-align: left; border: 1px solid #dee2e6;'>Trạng thái</th><th style='padding: 10px; text-align: left; border: 1px solid #dee2e6;'>Cán bộ</th></tr>");
            sb.Append($"<tr><td style='padding: 10px; border: 1px solid #dee2e6; color: #dc3545;'>Cán bộ cũ</td><td style='padding: 10px; border: 1px solid #dee2e6;'>{WebUtility.HtmlEncode(oldStaff?.Full_name ?? "N/A")}</td></tr>");
            sb.Append($"<tr><td style='padding: 10px; border: 1px solid #dee2e6; color: #198754;'>Cán bộ mới</td><td style='padding: 10px; border: 1px solid #dee2e6;'><strong>{WebUtility.HtmlEncode(newStaff?.Full_name ?? "N/A")}</strong></td></tr>");
            sb.Append("</table>");
            sb.Append("<p>Nếu có bất kỳ thắc mắc, vui lòng liên hệ với chúng tôi.</p>");
            sb.Append("<br/><p style='color: #888; font-size: 13px;'>Email này được gửi tự động, vui lòng không trả lời.</p>");
            sb.Append("</div>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        private static string BuildStaffReplacementBodyForAdmin(Customer customer, Staff? oldStaff, Staff? newStaff)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif;'>");
            sb.Append("<h2>Thay đổi cán bộ phụ trách khách hàng</h2>");
            sb.Append($"<p><strong>Khách hàng:</strong> {WebUtility.HtmlEncode(customer.Full_name ?? "-")}</p>");
            sb.Append($"<p><strong>Mã khách hàng:</strong> {WebUtility.HtmlEncode(customer.Customer_code)}</p>");
            sb.Append($"<p><strong>Cán bộ cũ:</strong> {WebUtility.HtmlEncode(oldStaff?.Full_name ?? "N/A")} ({WebUtility.HtmlEncode(oldStaff?.Staff_code ?? "-")})</p>");
            sb.Append($"<p><strong>Cán bộ mới:</strong> {WebUtility.HtmlEncode(newStaff?.Full_name ?? "N/A")} ({WebUtility.HtmlEncode(newStaff?.Staff_code ?? "-")})</p>");
            sb.Append($"<p><strong>Thời gian thay đổi:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>");
            sb.Append("<p>Vui lòng đăng nhập hệ thống để xem chi tiết phân công.</p>");
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
                var secureSocketOptions = settings.UseSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None;
                await smtp.ConnectAsync(settings.Host, settings.Port, secureSocketOptions, cancellationToken);
                if (!string.IsNullOrEmpty(settings.Password)
                    && smtp.Capabilities.HasFlag(MailKit.Net.Smtp.SmtpCapabilities.Authentication))
                {
                    await smtp.AuthenticateAsync(settings.FromEmail, settings.Password, cancellationToken);
                }
                await smtp.SendAsync(message, cancellationToken);
            }
            finally
            {
                await smtp.DisconnectAsync(true, cancellationToken);
            }
        }

        public async Task SendPackageAssignedToCustomerAsync(int customerId, List<int> packageIds, CancellationToken cancellationToken = default)
        {
            try
            {
                if (packageIds == null || packageIds.Count == 0) return;

                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null) return;

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                var customerEmail = await GetCustomerEmailAsync(customerId, customer);
                if (string.IsNullOrWhiteSpace(customerEmail)) return;

                var packages = new List<Package>();
                foreach (var pid in packageIds)
                {
                    var pkg = await _packageRepository.GetByIdAsync(pid);
                    if (pkg != null) packages.Add(pkg);
                }
                if (packages.Count == 0) return;

                var subject = packages.Count == 1
                    ? $"[Thông báo] Bạn vừa được gán gói sản phẩm: {packages[0].Package_name}"
                    : $"[Thông báo] Bạn vừa được gán {packages.Count} gói sản phẩm";

                var body = BuildPackageAssignedBody(customer, packages);
                await SendEmailAsync(smtp!, customerEmail, subject, body, cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email thông báo gán gói cho khách hàng {CustomerId}", customerId);
            }
        }

        private static string BuildPackageAssignedBody(Customer customer, List<Package> packages)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>");
            sb.Append("<div style='background: #0d6efd; padding: 30px; text-align: center;'>");
            sb.Append("<h1 style='color: white; margin: 0;'>Gói sản phẩm đã được kích hoạt</h1>");
            sb.Append("</div>");
            sb.Append("<div style='padding: 30px; background: #f9f9f9;'>");
            sb.Append($"<p>Xin chào <strong>{WebUtility.HtmlEncode(customer.Full_name ?? customer.Customer_code)}</strong>,</p>");
            if (packages.Count == 1)
                sb.Append("<p>Tài khoản của bạn vừa được gán gói sản phẩm sau đây:</p>");
            else
                sb.Append($"<p>Tài khoản của bạn vừa được gán <strong>{packages.Count}</strong> gói sản phẩm:</p>");
            sb.Append("<table style='border-collapse: collapse; width: 100%; margin-top: 15px;'>");
            sb.Append("<tr style='background: #0d6efd; color: white;'>");
            sb.Append("<th style='padding: 10px; text-align: left; border: 1px solid #dee2e6;'>Mã gói</th>");
            sb.Append("<th style='padding: 10px; text-align: left; border: 1px solid #dee2e6;'>Tên gói</th>");
            sb.Append("<th style='padding: 10px; text-align: left; border: 1px solid #dee2e6;'>Mô tả</th>");
            sb.Append("</tr>");
            for (int i = 0; i < packages.Count; i++)
            {
                var pkg = packages[i];
                var rowBg = i % 2 == 0 ? "#ffffff" : "#f8f9fa";
                sb.Append($"<tr style='background: {rowBg};'>");
                sb.Append($"<td style='padding: 10px; border: 1px solid #dee2e6;'>{WebUtility.HtmlEncode(pkg.Package_code)}</td>");
                sb.Append($"<td style='padding: 10px; border: 1px solid #dee2e6;'>{WebUtility.HtmlEncode(pkg.Package_name)}</td>");
                sb.Append($"<td style='padding: 10px; border: 1px solid #dee2e6;'>{WebUtility.HtmlEncode(pkg.Description ?? "—")}</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table>");
            sb.Append("<p style='margin-top: 20px;'>Bạn có thể đăng nhập vào hệ thống để xem danh sách sản phẩm có trong gói và bắt đầu đặt hàng.</p>");
            sb.Append("<hr style='border: 1px solid #ddd; margin: 20px 0;'/>");
            sb.Append("<p style='color: #888; font-size: 12px;'>Email này được gửi tự động, vui lòng không trả lời.</p>");
            sb.Append("</div>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        public async Task SendPasswordResetEmailAsync(int userId, string resetLink, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByIdBasicAsync(userId);
                if (user == null || user.Is_deleted || string.IsNullOrWhiteSpace(user.Email)) return;

                var smtp = await _settingService.GetSmtpSettingsAsync();
                if (!IsSmtpValid(smtp)) return;

                var subject = "[ERP] Yêu cầu đặt lại mật khẩu";
                var body = BuildPasswordResetEmailBody(user.Username, resetLink);
                await SendEmailAsync(smtp!, user.Email.Trim(), subject, body, cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email đặt lại mật khẩu cho user {UserId}", userId);
            }
        }

        private static string BuildPasswordResetEmailBody(string username, string resetLink)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>");
            sb.Append("<div style='background: #dc3545; padding: 30px; text-align: center;'>");
            sb.Append("<h1 style='color: white; margin: 0;'>Đặt lại mật khẩu</h1>");
            sb.Append("</div>");
            sb.Append("<div style='padding: 30px; background: #f9f9f9;'>");
            sb.Append($"<p>Xin chào <strong>{WebUtility.HtmlEncode(username)}</strong>,</p>");
            sb.Append("<p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>");
            sb.Append("<p>Nhấn vào nút bên dưới để đặt mật khẩu mới. <strong>Link có hiệu lực trong 1 giờ.</strong></p>");
            sb.Append("<div style='text-align: center; margin: 30px 0;'>");
            sb.Append($"<a href='{WebUtility.HtmlEncode(resetLink)}' style='background: #dc3545; color: white; padding: 14px 28px; text-decoration: none; border-radius: 6px; font-size: 16px; font-weight: bold;'>Đặt lại mật khẩu</a>");
            sb.Append("</div>");
            sb.Append($"<p style='color: #666; font-size: 13px;'>Hoặc copy link sau vào trình duyệt:<br/><a href='{WebUtility.HtmlEncode(resetLink)}'>{WebUtility.HtmlEncode(resetLink)}</a></p>");
            sb.Append("<hr style='border: 1px solid #ddd; margin: 20px 0;'/>");
            sb.Append("<p style='color: #888; font-size: 12px;'>Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này. Tài khoản của bạn vẫn an toàn.</p>");
            sb.Append("<p style='color: #888; font-size: 12px;'>Email này được gửi tự động, vui lòng không trả lời.</p>");
            sb.Append("</div>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

    }
}
