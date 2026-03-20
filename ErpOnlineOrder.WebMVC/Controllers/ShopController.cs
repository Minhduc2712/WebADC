using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.OrderDTOs;
using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;
using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.Application.DTOs.CustomerDTOs;
using ErpOnlineOrder.Application.DTOs.AuthDTOs;
using ErpOnlineOrder.Domain.Constants;
using ErpOnlineOrder.WebMVC.Extensions;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Services;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequireAuth]
    public class ShopController : BaseController
    {
        private readonly IProductApiClient _productApiClient;
        private readonly IOrderApiClient _orderApiClient;
        private readonly IWarehouseExportApiClient _warehouseExportApiClient;
        private readonly IInvoiceApiClient _invoiceApiClient;
        private readonly ICustomerApiClient _customerApiClient;
        private readonly IAuthApiClient _authApiClient;
        private readonly ILogger<ShopController> _logger;

        public ShopController(
            IProductApiClient productApiClient,
            IOrderApiClient orderApiClient,
            IWarehouseExportApiClient warehouseExportApiClient,
            IInvoiceApiClient invoiceApiClient,
            ICustomerApiClient customerApiClient,
            IAuthApiClient authApiClient,
            ILogger<ShopController> logger)
        {
            _productApiClient = productApiClient;
            _orderApiClient = orderApiClient;
            _warehouseExportApiClient = warehouseExportApiClient;
            _invoiceApiClient = invoiceApiClient;
            _customerApiClient = customerApiClient;
            _authApiClient = authApiClient;
            _logger = logger;
        }

        private async Task<int?> GetCurrentCustomerIdAsync()
        {
            if (!HttpContext.Session.IsCustomer()) return null;
            var userId = GetCurrentUserId();
            if (userId == 0) return null;
            // (Cần bổ sung endpoint bên WebAPI)
            var customer = await _customerApiClient.GetByUserIdAsync(userId);
            return customer?.Id;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(Products));
        }

        public async Task<IActionResult> Products(string? search, string? category, string? sort, int page = 1, int pageSize = 12)
        {
            try
            {
                var customerId = await GetCurrentCustomerIdAsync();
                var request = new ProductForShopFilterRequest
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchTerm = search,
                    Category = category,
                    Sort = sort
                };

                var paged = await _productApiClient.GetProductsForShopPagedAsync(customerId, request);
                var categories = await _productApiClient.GetCategoriesForShopAsync(customerId);

                ViewBag.Search = search;
                ViewBag.Category = category;
                ViewBag.Sort = sort;
                ViewBag.PageSize = pageSize;
                ViewData["Categories"] = categories.ToList();
                ViewBag.TotalCount = paged.TotalCount;
                ViewBag.PaginationAction = "Products";

                return View(paged);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products");
                return View(new PagedResult<ProductDTO> { Items = new List<ProductDTO>() });
            }
        }

        public async Task<IActionResult> ProductDetail(int id)
        {
            try
            {
                var customerId = await GetCurrentCustomerIdAsync();
                var product = await _productApiClient.GetByIdAsync(id);
                if (product == null)
                    return NotFound();

                // Khách hàng chỉ xem được sản phẩm đã được gán
                if (customerId.HasValue)
                {
                    var hasAccess = await _productApiClient.IsProductAssignedToCustomerAsync(id, customerId.Value);
                    if (!hasAccess)
                        return NotFound();
                }

                var relatedProducts = await _productApiClient.GetRelatedProductsForShopAsync(id, customerId, 4);
                ViewData["RelatedProducts"] = relatedProducts.ToList();

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product detail");
                return RedirectToAction(nameof(Products));
            }
        }

        public IActionResult Cart()
        {
            return View();
        }

        public async Task<IActionResult> Checkout()
        {
            var organizationAddress = string.Empty;
            var customerId = await GetCurrentCustomerIdAsync();
            if (customerId.HasValue)
            {
                var org = await _customerApiClient.GetOrganizationByCustomerIdAsync(customerId.Value);
                organizationAddress = org?.Recipient_address;
                if (string.IsNullOrWhiteSpace(organizationAddress))
                    organizationAddress = org?.Address;
            }

            ViewBag.OrganizationShippingAddress = organizationAddress ?? string.Empty;
            ViewBag.HasOrganizationShippingAddress = !string.IsNullOrWhiteSpace(organizationAddress);
            return View();
        }

        public async Task<IActionResult> Orders(string? status, int page = 1, int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var customer = await _customerApiClient.GetByUserIdAsync(userId);
                if (customer == null)
                {
                    ViewData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                    return View(new PagedResult<OrderDTO> { Items = new List<OrderDTO>() });
                }

                var request = new OrderFilterRequest { Page = page, PageSize = pageSize, Status = status };
                var paged = await _orderApiClient.GetOrdersByCustomerPagedAsync(customer.Id, request);
                ViewBag.Status = status;
                ViewBag.PageSize = pageSize;
                ViewBag.PaginationAction = "Orders";
                ViewBag.PaginationItemLabel = "đơn hàng";
                return View(paged);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders for user {UserId}", userId);
                return View(new PagedResult<OrderDTO> { Items = new List<OrderDTO>() });
            }
        }

        /// <summary>Chi tiết và theo dõi trạng thái đơn hàng.</summary>
        [HttpGet]
        public async Task<IActionResult> OrderDetail(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var customer = await _customerApiClient.GetByUserIdAsync(userId);
            if (customer == null)
                return NotFound();

            var order = await _orderApiClient.GetByIdAsync(id);
            if (order == null)
                return NotFound();

            // Đảm bảo đơn hàng thuộc về khách hàng này
            var myOrders = await _orderApiClient.GetOrdersByCustomerAsync(customer.Id);
            if (!myOrders.Any(o => o.Id == id))
                return Forbid();

            return View(order);
        }

        /// <summary>In đơn đặt hàng theo mẫu. Chỉ khách hàng sở hữu đơn mới in được.</summary>
        [HttpGet]
        public async Task<IActionResult> PrintOrder(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var customer = await _customerApiClient.GetByUserIdAsync(userId);
            if (customer == null)
                return NotFound();

            var order = await _orderApiClient.GetByIdAsync(id);
            if (order == null)
                return NotFound();

            var myOrders = await _orderApiClient.GetOrdersByCustomerAsync(customer.Id);
            if (!myOrders.Any(o => o.Id == id))
                return Forbid();

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để đặt hàng." });
                }

                var customer = await _customerApiClient.GetByUserIdAsync(userId);
                if (customer == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng." });
                }

                if (request.Items == null || !request.Items.Any())
                {
                    return Json(new { success = false, message = "Giỏ hàng trống." });
                }

                var createOrderDto = new CreateOrderDto
                {
                    Order_date = DateTime.Now,
                    Customer_id = customer.Id,
                    Shipping_address = request.ShippingAddress,
                    note = request.Note,
                    Created_by = userId,
                    Order_details = request.Items.Select(item => new OrderDetailDto
                    {
                        Product_id = item.ProductId,
                        Quantity = item.Quantity,
                        Unit_price = item.Price
                    }).ToList()
                };

                var result = await _orderApiClient.CreateOrderCustomerAsync(createOrderDto);

                if (result.Success)
                {
                    return Json(new { success = true, message = "Đặt hàng thành công!", orderCode = result.Order_code, orderId = result.Order_id });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing order");
                return Json(new { success = false, message = "Không thể đặt hàng lúc này. Vui lòng kiểm tra lại giỏ hàng và thử lại sau." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập." });
                }

                var customer = await _customerApiClient.GetByUserIdAsync(userId);
                if (customer == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng." });
                }

                // Verify order belongs to this customer
                var order = await _orderApiClient.GetByIdAsync(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
                }

                var result = await _orderApiClient.CancelOrderAsync(id);
                if (result)
                {
                    return Json(new { success = true, message = "Hủy đơn hàng thành công." });
                }
                return Json(new { success = false, message = "Không thể hủy đơn hàng này." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", id);
                return Json(new { success = false, message = "Không thể hủy đơn hàng lúc này. Vui lòng thử lại sau." });
            }
        }

        /// <summary>Danh sách hóa đơn của khách hàng.</summary>
        public async Task<IActionResult> Invoices(string? status, int page = 1, int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var customer = await _customerApiClient.GetByUserIdAsync(userId);
            if (customer == null)
            {
                ViewData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return View(new PagedResult<InvoiceDto> { Items = new List<InvoiceDto>() });
            }

            try
            {
                var request = new InvoiceFilterRequest { Page = page, PageSize = pageSize, Status = status };
                var paged = await _invoiceApiClient.GetByCustomerIdPagedAsync(customer.Id, request);
                ViewBag.Status = status;
                ViewBag.PageSize = pageSize;
                ViewBag.PaginationAction = "Invoices";
                ViewBag.PaginationItemLabel = "hóa đơn";
                return View(paged);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading invoices for customer {CustomerId}", customer.Id);
                return View(new PagedResult<InvoiceDto> { Items = new List<InvoiceDto>() });
            }
        }

        /// <summary>Danh sách phiếu xuất kho của khách hàng.</summary>
        public async Task<IActionResult> Exports(string? status, int page = 1, int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var customer = await _customerApiClient.GetByUserIdAsync(userId);
            if (customer == null)
            {
                ViewData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return View(new PagedResult<WarehouseExportDto> { Items = new List<WarehouseExportDto>() });
            }

            try
            {
                var request = new WarehouseExportFilterRequest { Page = page, PageSize = pageSize, Status = status };
                var paged = await _warehouseExportApiClient.GetByCustomerIdPagedAsync(customer.Id, request);
                ViewBag.Status = status;
                ViewBag.PageSize = pageSize;
                ViewBag.PaginationAction = "Exports";
                ViewBag.PaginationItemLabel = "phiếu xuất";
                return View(paged);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading exports for customer {CustomerId}", customer.Id);
                return View(new PagedResult<WarehouseExportDto> { Items = new List<WarehouseExportDto>() });
            }
        }

        /// <summary>Yêu cầu tách hóa đơn từ phiếu xuất kho - khách chỉ định số lượng cụ thể cho từng sản phẩm.</summary>
        [HttpGet]
        public async Task<IActionResult> RequestInvoiceSplit(int exportId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var customer = await _customerApiClient.GetByUserIdAsync(userId);
            if (customer == null)
                return NotFound();

            var export = await _warehouseExportApiClient.GetByIdAsync(exportId);
            if (export == null)
                return NotFound();

            if (export.Customer_id != customer.Id)
                return Forbid();

            if (export.Status == ExportStatuses.Cancelled)
            {
                TempData["Error"] = "Phiếu xuất kho đã hủy.";
                return RedirectToAction(nameof(Exports));
            }

            if (!export.Invoice_id.HasValue)
            {
                TempData["Error"] = "Phiếu xuất kho này chưa có hóa đơn.";
                return RedirectToAction(nameof(Exports));
            }

            var invoice = await _invoiceApiClient.GetByIdAsync(export.Invoice_id.Value);
            if (invoice == null)
            {
                TempData["Error"] = "Không tìm thấy hóa đơn liên quan.";
                return RedirectToAction(nameof(Exports));
            }

            if (invoice.Status == InvoiceStatuses.Split || invoice.Status == InvoiceStatuses.Merged)
            {
                TempData["Error"] = "Hóa đơn này đã được tách hoặc gộp.";
                return RedirectToAction(nameof(Exports));
            }

            if (invoice.Details == null || invoice.Details.Count < 2)
            {
                TempData["Error"] = "Hóa đơn chỉ có 1 sản phẩm, không thể tách.";
                return RedirectToAction(nameof(Exports));
            }

            ViewBag.Export = export;
            ViewBag.Invoice = invoice;
            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestInvoiceSplit(int exportId, IFormCollection form)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var customer = await _customerApiClient.GetByUserIdAsync(userId);
            if (customer == null)
                return NotFound();

            var export = await _warehouseExportApiClient.GetByIdAsync(exportId);
            if (export == null)
                return NotFound();

            if (export.Customer_id != customer.Id)
                return Forbid();

            if (!export.Invoice_id.HasValue)
            {
                TempData["Error"] = "Phiếu xuất kho này chưa có hóa đơn.";
                return RedirectToAction(nameof(Exports));
            }

            var invoice = await _invoiceApiClient.GetByIdAsync(export.Invoice_id.Value);
            if (invoice == null)
            {
                TempData["Error"] = "Không tìm thấy hóa đơn.";
                return RedirectToAction(nameof(Exports));
            }

            var items = new List<SplitInvoiceItem>();
            foreach (var detail in invoice.Details ?? new List<InvoiceDetailDto>())
            {
                var key = $"qty_{detail.Id}";
                if (form.TryGetValue(key, out var val) && int.TryParse(val, out var qty) && qty > 0)
                {
                    if (qty > detail.Quantity)
                    {
                        TempData["Error"] = $"Số lượng sản phẩm '{detail.Product_name}' vượt quá {detail.Quantity}.";
                        ViewBag.Export = export;
                        ViewBag.Invoice = invoice;
                        return View(invoice);
                    }
                    items.Add(new SplitInvoiceItem { Invoice_detail_id = detail.Id, Quantity = qty });
                }
            }

            if (items.Count == 0)
            {
                TempData["Error"] = "Vui lòng nhập ít nhất một sản phẩm với số lượng > 0.";
                ViewBag.Export = export;
                ViewBag.Invoice = invoice;
                return View(invoice);
            }

            var dto = new SplitInvoiceDto
            {
                Source_invoice_id = invoice.Id,
                Split_parts = new List<SplitInvoicePart> { new SplitInvoicePart { Items = items } },
                Note = $"Khách hàng yêu cầu tách từ phiếu xuất {export.Warehouse_export_code}"
            };

            try
            {
                var result = await _invoiceApiClient.SplitAsync(dto);
                if (result?.Success == true)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Exports));
                }
                TempData["Error"] = result?.Message ?? "Không thể tách hóa đơn. Vui lòng kiểm tra số lượng tách và trạng thái hóa đơn.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error splitting invoice for customer");
                TempData["Error"] = "Không thể xử lý yêu cầu tách hóa đơn lúc này. Vui lòng thử lại sau.";
            }

            ViewBag.Export = export;
            ViewBag.Invoice = invoice;
            return View(invoice);
        }

        public IActionResult Account()
        {
            return View();
        }

        #region Change Password (Customer)

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var model = new ChangePasswordDto
            {
                Identifier = HttpContext.Session.GetUsername()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                model.Identifier = HttpContext.Session.GetUsername();
                var (success, errorMessage) = await _authApiClient.ChangePasswordAsync(model);

                if (success)
                {
                    SetSuccessMessage("Đổi mật khẩu thành công!");
                    return RedirectToAction(nameof(ChangePassword));
                }

                ModelState.AddModelError("", errorMessage ?? "Không thể đổi mật khẩu. Vui lòng kiểm tra mật khẩu hiện tại và mật khẩu mới.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user");
                ModelState.AddModelError("", "Không thể đổi mật khẩu lúc này. Vui lòng thử lại sau.");
            }

            return View(model);
        }

        #endregion

        /// <summary>Khách hàng chỉnh sửa thông tin đơn vị (tổ chức).</summary>
        [HttpGet]
        public async Task<IActionResult> UpdateOrganization()
        {
            var customerId = await GetCurrentCustomerIdAsync();
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Auth");

            var org = await _customerApiClient.GetOrganizationByCustomerIdAsync(customerId.Value);
            var model = new UpdateOrganizationByCustomerDto
            {
                Customer_id = customerId.Value,
                Organization_name = org?.Organization_name ?? "",
                Address = org?.Address ?? "",
                Tax_number = org?.Tax_number ?? 0,
                Recipient_name = org?.Recipient_name ?? "",
                Recipient_phone = org?.Recipient_phone ?? 0,
                Recipient_address = org?.Recipient_address ?? ""
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrganization(UpdateOrganizationByCustomerDto model)
        {
            var customerId = await GetCurrentCustomerIdAsync();
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Auth");

            model.Customer_id = customerId.Value;
            if (string.IsNullOrWhiteSpace(model.Organization_name))
                ModelState.AddModelError("Organization_name", "Tên đơn vị không được để trống.");
            if (string.IsNullOrWhiteSpace(model.Address))
                ModelState.AddModelError("Address", "Địa chỉ không được để trống.");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = await _customerApiClient.UpdateOrganizationAsync(model);
                if (result)
                {
                    SetSuccessMessage("Cập nhật thông tin đơn vị thành công.");
                    return RedirectToAction(nameof(Account));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organization for customer {CustomerId}", customerId);
                ModelState.AddModelError("", "Không thể cập nhật thông tin đơn vị. Vui lòng kiểm tra dữ liệu và thử lại.");
            }
            return View(model);
        }

        public IActionResult Info(string slug)
        {
            ViewBag.Slug = slug ?? "gioi-thieu";
            return View();
        }
    }

    public class PlaceOrderRequest
    {
        public List<CartItemRequest> Items { get; set; } = new();
        public string? Note { get; set; }
        public string? ShippingAddress { get; set; }
    }

    public class CartItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
