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
        private readonly ICustomerManagementApiClient _customerManagementApiClient;
        private readonly IProvinceApiClient _provinceApiClient;
        private readonly ILogger<ShopController> _logger;

        public ShopController(
            IProductApiClient productApiClient,
            IOrderApiClient orderApiClient,
            IWarehouseExportApiClient warehouseExportApiClient,
            IInvoiceApiClient invoiceApiClient,
            ICustomerApiClient customerApiClient,
            IAuthApiClient authApiClient,
            ICustomerManagementApiClient customerManagementApiClient,
            IProvinceApiClient provinceApiClient,
            ILogger<ShopController> logger)
        {
            _productApiClient = productApiClient;
            _orderApiClient = orderApiClient;
            _warehouseExportApiClient = warehouseExportApiClient;
            _invoiceApiClient = invoiceApiClient;
            _customerApiClient = customerApiClient;
            _authApiClient = authApiClient;
            _customerManagementApiClient = customerManagementApiClient;
            _provinceApiClient = provinceApiClient;
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
            var userId = GetCurrentUserId();

            if (customerId.HasValue)
            {
                var org = await _customerApiClient.GetOrganizationByCustomerIdAsync(customerId.Value);
                organizationAddress = org?.Recipient_address;
                if (string.IsNullOrWhiteSpace(organizationAddress))
                    organizationAddress = org?.Address;

                ViewBag.RecipientName = !string.IsNullOrWhiteSpace(org?.Recipient_name)
                    ? org.Recipient_name
                    : null;
                ViewBag.RecipientPhone = !string.IsNullOrWhiteSpace(org?.Recipient_phone)
                    ? org.Recipient_phone
                    : null;
                ViewBag.OrganizationName = org?.Organization_name;
                ViewBag.OrganizationTaxNumber = org?.Tax_number;
            }

            if (userId != 0)
            {
                var customer = await _customerApiClient.GetByUserIdAsync(userId);
                if (customer != null)
                {
                    ViewBag.CustomerFullName = customer.Full_name;
                    ViewBag.CustomerEmail = customer.Email;
                    ViewBag.CustomerPhone ??= customer.Phone_number;
                    ViewBag.RecipientName ??= customer.Full_name;
                }
            }

            ViewBag.OrganizationShippingAddress = organizationAddress ?? string.Empty;
            ViewBag.HasOrganizationShippingAddress = !string.IsNullOrWhiteSpace(organizationAddress);

            var provinces = await _provinceApiClient.GetAllAsync();
            ViewBag.Provinces = provinces.OrderBy(p => p.Province_name).ToList();

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

            try
            {
                var order = await _orderApiClient.GetByIdAsync(id);
                if (order == null)
                    return NotFound();

                // Đảm bảo đơn hàng thuộc về khách hàng này
                var myOrders = await _orderApiClient.GetOrdersByCustomerAsync(customer.Id);
                if (!myOrders.Any(o => o.Id == id))
                    return Forbid();

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order detail {OrderId}", id);
                return NotFound();
            }
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

            try
            {
                var order = await _orderApiClient.GetByIdAsync(id);
                if (order == null)
                    return NotFound();

                var myOrders = await _orderApiClient.GetOrdersByCustomerAsync(customer.Id);
                if (!myOrders.Any(o => o.Id == id))
                    return Forbid();

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading print order {OrderId}", id);
                return NotFound();
            }
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

        [HttpPost]
        public async Task<IActionResult> CustomerRejectOrder(int id)
        {
            try
            {
                var (success, error) = await _orderApiClient.CustomerRejectOrderAsync(id);
                if (error != null && error.Contains("JSON"))
                {
                    return Json(new { success = false, message = "Máy chủ WebAPI chưa nhận code mới. Vui lòng tắt và khởi động lại dự án WebAPI." });
                }
                return Json(new { success = success, message = success ? "Đã không duyệt. Đơn hàng gốc đã được khôi phục về trạng thái Chờ xử lý." : (error ?? "Không thể thao tác lúc này.") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error customer rejecting order {OrderId}", id);
                return Json(new { success = false, message = "Lỗi kết nối máy chủ." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CustomerApproveOrder(int id)
        {
            try
            {
                var (success, error) = await _orderApiClient.CustomerApproveOrderAsync(id);
                if (error != null && error.Contains("JSON"))
                {
                    return Json(new { success = false, message = "Máy chủ WebAPI chưa nhận code mới. Vui lòng tắt và khởi động lại dự án WebAPI." });
                }
                return Json(new { success = success, message = success ? "Đã duyệt đơn hàng thành công!" : (error ?? "Không thể duyệt đơn hàng lúc này.") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error customer approving order {OrderId}", id);
                return Json(new { success = false, message = "Lỗi kết nối máy chủ." });
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

        /// <summary>Yêu cầu xuất/tách hóa đơn từ phiếu xuất kho</summary>
        [HttpGet]
        public async Task<IActionResult> RequestInvoiceSplit(int exportId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var customer = await _customerApiClient.GetByUserIdAsync(userId);
            if (customer == null)
                return NotFound();

            try
            {
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

                if (!string.IsNullOrEmpty(export.Invoice_code))
                {
                    TempData["Error"] = "Phiếu xuất kho này đã được xử lý hóa đơn.";
                    return RedirectToAction(nameof(Exports));
                }

                if (export.Details == null || !export.Details.Any())
                {
                    TempData["Error"] = "Phiếu xuất kho không có sản phẩm.";
                    return RedirectToAction(nameof(Exports));
                }

                return View(export);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading export {ExportId} for split request", exportId);
                return NotFound();
            }
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

            WarehouseExportDto export;
            try
            {
                export = await _warehouseExportApiClient.GetByIdAsync(exportId);
                if (export == null)
                    return NotFound();

                if (export.Customer_id != customer.Id)
                    return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading export {ExportId}", exportId);
                return NotFound();
            }

            if (!string.IsNullOrEmpty(export.Invoice_code))
            {
                TempData["Error"] = "Phiếu xuất kho này đã được xử lý hóa đơn.";
                return RedirectToAction(nameof(Exports));
            }

            var items = new List<CustomerInvoiceSplitItemDto>();
            bool hasInvalidQuantity = false;

            foreach (var detail in export.Details ?? new List<WarehouseExportDetailDto>())
            {
                var key = $"qty_{detail.Product_id}";
                if (form.TryGetValue(key, out var val) && int.TryParse(val, out var qty) && qty > 0)
                {
                    if (qty > detail.Quantity_shipped)
                    {
                        TempData["Error"] = $"Số lượng sản phẩm '{detail.Product_name}' vượt quá số lượng xuất ({detail.Quantity_shipped}).";
                        hasInvalidQuantity = true;
                        break;
                    }
                    items.Add(new CustomerInvoiceSplitItemDto { ProductId = detail.Product_id, Quantity = qty });
                }
            }

            if (hasInvalidQuantity)
            {
                return View(export);
            }

            var requestDto = new CustomerInvoiceRequestDto
            {
                WarehouseExportId = export.Id,
                Note = form["note"].ToString() ?? "Khách hàng yêu cầu xuất hóa đơn",
                // Nếu khách có nhập SL thì tách part, nếu không hệ thống sẽ tạo 1 hóa đơn tổng
                SplitParts = items.Count > 0 ? new List<CustomerInvoiceSplitPartDto> { new CustomerInvoiceSplitPartDto { Items = items } } : null
            };

            try
            {
                var (data, error) = await _invoiceApiClient.CustomerRequestInvoiceAsync(requestDto);
                if (data != null && data.Any())
                {
                    TempData["Success"] = "Đã gửi yêu cầu xuất hóa đơn thành công!";
                    return RedirectToAction(nameof(Exports));
                }
                TempData["Error"] = error ?? "Không thể gửi yêu cầu xuất hóa đơn. Vui lòng kiểm tra lại.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error splitting invoice for customer");
                TempData["Error"] = "Không thể xử lý yêu cầu tách hóa đơn lúc này. Vui lòng thử lại sau.";
            }

            return View(export);
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
                Tax_number = org?.Tax_number ?? "",
                Recipient_name = org?.Recipient_name ?? "",
                Recipient_phone = org?.Recipient_phone ?? "",
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
                    return RedirectToAction(nameof(UpdateOrganization));
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

        public async Task<IActionResult> MyStaff()
        {
            var customerId = await GetCurrentCustomerIdAsync();
            if (customerId == null)
                return RedirectToAction("Products");

            try
            {
                var assignments = await _customerManagementApiClient.GetByCustomerAsync(customerId.Value);
                return View(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading staff assignments for customer {CustomerId}", customerId);
                return View(Enumerable.Empty<ErpOnlineOrder.Domain.Models.Customer_management>());
            }
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
