using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.OrderDTOs;
using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;
using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.Application.DTOs.CustomerDTOs;
using ErpOnlineOrder.Application.DTOs.OrganizationDTOs;
using ErpOnlineOrder.Application.DTOs.AuthDTOs;
using ErpOnlineOrder.Domain.Constants;
using ErpOnlineOrder.WebMVC.Extensions;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Services;
using ErpOnlineOrder.WebMVC.Services.Interfaces;
using ErpOnlineOrder.WebMVC.Models;

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
        private readonly IOrganizationApiClient _organizationApiClient;
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
            IOrganizationApiClient organizationApiClient,
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
            _organizationApiClient = organizationApiClient;
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

        public async Task<IActionResult> Products(string? search, List<string>? categories, List<string>? publishers, List<string>? authors, decimal? priceMin, decimal? priceMax, string? sort, int page = 1, int pageSize = 12)
        {
            try
            {
                var customerId = await GetCurrentCustomerIdAsync();
                var request = new ProductForShopFilterRequest
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchTerm = search,
                    Categories = categories,
                    Publishers = publishers,
                    Authors = authors,
                    Sort = sort,
                    PriceMin = priceMin,
                    PriceMax = priceMax
                };

                var pagedTask = _productApiClient.GetProductsForShopPagedAsync(customerId, request);
                var categoriesTask = _productApiClient.GetCategoriesForShopAsync(customerId);
                var publishersTask = _productApiClient.GetPublishersForShopAsync(customerId);
                var authorsTask = _productApiClient.GetAuthorsForShopAsync(customerId);
                await Task.WhenAll(pagedTask, categoriesTask, publishersTask, authorsTask);

                var paged = await pagedTask;

                ViewBag.Search = search;
                ViewBag.SelectedCategories = categories ?? new List<string>();
                ViewBag.SelectedPublishers = publishers ?? new List<string>();
                ViewBag.SelectedAuthors = authors ?? new List<string>();
                ViewBag.Sort = sort;
                ViewBag.PageSize = pageSize;
                ViewBag.PriceMin = priceMin;
                ViewBag.PriceMax = priceMax;
                ViewData["Categories"] = (await categoriesTask).ToList();
                ViewData["Publishers"] = (await publishersTask).ToList();
                ViewData["Authors"] = (await authorsTask).ToList();
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
                var product = await _productApiClient.GetProductForShopAsync(id, customerId);
                if (product == null)
                    return NotFound();

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
                    organizationAddress = org?.Organization_address;

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
            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault(m => !string.IsNullOrEmpty(m)) ?? "Dữ liệu đặt hàng không hợp lệ.";
                return Json(new { success = false, message = firstError });
            }

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

                var org = await _customerApiClient.GetOrganizationByCustomerIdAsync(customer.Id);
                // Ưu tiên địa chỉ người dùng nhập trực tiếp, fallback về địa chỉ org
                var shippingAddress = !string.IsNullOrWhiteSpace(request.ShippingAddress)
                    ? request.ShippingAddress
                    : !string.IsNullOrWhiteSpace(org?.Recipient_address)
                        ? org.Recipient_address
                        : org?.Organization_address;

                var recipientInfo = string.Empty;
                if (!string.IsNullOrWhiteSpace(request.RecipientName) || !string.IsNullOrWhiteSpace(request.RecipientPhone))
                {
                    recipientInfo = $"Người nhận: {request.RecipientName} - {request.RecipientPhone}".Trim(' ', '-');
                    if (!string.IsNullOrWhiteSpace(request.RecipientEmail))
                        recipientInfo += $" - {request.RecipientEmail}";
                }

                var fullNote = string.Join("\n", new[] { recipientInfo, request.Note }.Where(s => !string.IsNullOrWhiteSpace(s)));

                var createOrderDto = new CreateOrderDto
                {
                    Order_date = DateTime.Now,
                    Customer_id = customer.Id,
                    Shipping_address = shippingAddress,
                    note = fullNote,
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

            WarehouseExportDto? export = null;
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
                return RedirectToAction(nameof(RequestInvoiceSplit), new { exportId });
            }

            var requestDto = new CustomerInvoiceRequestDto
            {
                WarehouseExportId = export.Id,
                Note = (form["note"].ToString() is { Length: > 0 } n ? n[..Math.Min(n.Length, 500)] : "Khách hàng yêu cầu xuất hóa đơn"),
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

            return RedirectToAction(nameof(RequestInvoiceSplit), new { exportId });
        }

        /// <summary>Tổng hợp số lượng đã giao và yêu cầu hóa đơn (toàn bộ hoặc theo đơn hàng).</summary>
        [HttpGet]
        public async Task<IActionResult> RequestInvoiceByOrder(int? orderId = null)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var customer = await _customerApiClient.GetByUserIdAsync(userId);
            if (customer == null)
                return NotFound();

            try
            {
                var allExports = await _warehouseExportApiClient.GetByCustomerIdAsync(customer.Id);

                var completedExports = allExports
                    .Where(e => (orderId == null || e.Order_id == orderId)
                             && e.Status == ExportStatuses.Completed
                             && string.IsNullOrEmpty(e.Invoice_code)
                             && e.Details != null && e.Details.Any())
                    .ToList();

                if (!completedExports.Any())
                {
                    TempData["Error"] = "Không tìm thấy phiếu xuất kho nào đã hoàn thành, hoặc tất cả đã được xuất hóa đơn.";
                    return RedirectToAction(nameof(Exports));
                }

                var aggregated = completedExports
                    .SelectMany(e => e.Details)
                    .GroupBy(d => d.Product_id)
                    .Select(g => new AggregatedExportItemViewModel
                    {
                        ProductId = g.Key,
                        ProductName = g.First().Product_name,
                        ProductCode = g.First().Product_code,
                        TotalQuantity = g.Sum(d => d.Quantity_shipped),
                        UnitPrice = g.First().Unit_price,
                        TotalPrice = g.Sum(d => d.Total_price)
                    })
                    .ToList();

                var viewModel = new InvoiceByOrderViewModel
                {
                    OrderId = orderId ?? 0,
                    OrderCode = orderId.HasValue ? completedExports.First().Order_code : null,
                    ExportCount = completedExports.Count,
                    ExportIds = completedExports.Select(e => e.Id).ToList(),
                    AggregatedItems = aggregated,
                    TotalAmount = aggregated.Sum(a => a.TotalPrice)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading exports for invoice request");
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestInvoiceByOrder(int? orderId, IFormCollection form)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var customer = await _customerApiClient.GetByUserIdAsync(userId);
            if (customer == null)
                return NotFound();

            try
            {
                var allExports = await _warehouseExportApiClient.GetByCustomerIdAsync(customer.Id);

                var completedExports = allExports
                    .Where(e => (orderId == null || e.Order_id == orderId)
                             && e.Status == ExportStatuses.Completed
                             && string.IsNullOrEmpty(e.Invoice_code)
                             && e.Details != null && e.Details.Any())
                    .ToList();

                if (!completedExports.Any())
                {
                    TempData["Error"] = "Không còn phiếu xuất kho nào cần xuất hóa đơn.";
                    return RedirectToAction(nameof(Exports));
                }

                // Tổng SL đã giao theo product_id
                var maxQty = completedExports
                    .SelectMany(e => e.Details)
                    .GroupBy(d => d.Product_id)
                    .ToDictionary(g => g.Key, g => g.Sum(d => d.Quantity_shipped));

                // Đọc SL khách chọn từ form
                var requestedQty = new Dictionary<int, int>();
                foreach (var productId in maxQty.Keys)
                {
                    var key = $"qty_{productId}";
                    if (form.TryGetValue(key, out var val) && int.TryParse(val, out var qty) && qty > 0)
                    {
                        if (qty > maxQty[productId])
                        {
                            TempData["Error"] = $"Số lượng yêu cầu vượt quá số lượng đã giao.";
                            return RedirectToAction(nameof(RequestInvoiceByOrder), orderId.HasValue ? new { orderId } : null);
                        }
                        requestedQty[productId] = qty;
                    }
                }

                if (!requestedQty.Any())
                {
                    TempData["Error"] = "Vui lòng nhập số lượng ít nhất một sản phẩm cần xuất hóa đơn.";
                    return RedirectToAction(nameof(RequestInvoiceByOrder), orderId.HasValue ? new { orderId } : null);
                }

                // Phân bổ SL theo FIFO qua các phiếu xuất kho
                var remaining = new Dictionary<int, int>(requestedQty);
                var noteRaw = form["note"].ToString();
                var note = noteRaw.Length > 500 ? noteRaw[..500] : noteRaw;
                bool anySuccess = false;
                string? lastError = null;

                foreach (var export in completedExports)
                {
                    var items = new List<CustomerInvoiceSplitItemDto>();
                    foreach (var detail in export.Details)
                    {
                        if (remaining.TryGetValue(detail.Product_id, out var rem) && rem > 0)
                        {
                            var take = Math.Min(rem, detail.Quantity_shipped);
                            items.Add(new CustomerInvoiceSplitItemDto { ProductId = detail.Product_id, Quantity = take });
                            remaining[detail.Product_id] -= take;
                        }
                    }

                    if (!items.Any()) continue;

                    var requestDto = new CustomerInvoiceRequestDto
                    {
                        WarehouseExportId = export.Id,
                        Note = string.IsNullOrWhiteSpace(note) ? "Khách hàng yêu cầu xuất hóa đơn" : note,
                        SplitParts = new List<CustomerInvoiceSplitPartDto>
                        {
                            new CustomerInvoiceSplitPartDto { Items = items }
                        }
                    };

                    var (data, error) = await _invoiceApiClient.CustomerRequestInvoiceAsync(requestDto);
                    if (data != null && data.Any())
                        anySuccess = true;
                    else
                        lastError = error;
                }

                if (anySuccess)
                    TempData["Success"] = "Đã gửi yêu cầu xuất hóa đơn thành công!";
                else
                    TempData["Error"] = lastError ?? "Không thể gửi yêu cầu xuất hóa đơn. Vui lòng kiểm tra lại.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting invoice for order {OrderId}", orderId);
                TempData["Error"] = "Không thể xử lý yêu cầu lúc này. Vui lòng thử lại sau.";
            }

            return RedirectToAction(nameof(Exports));
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
            {
                TempData["Error"] = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault(m => !string.IsNullOrEmpty(m)) ?? "Thông tin không hợp lệ. Vui lòng kiểm tra lại.";
                return RedirectToAction(nameof(ChangePassword));
            }

            try
            {
                model.Identifier = HttpContext.Session.GetUsername();
                var (success, errorMessage) = await _authApiClient.ChangePasswordAsync(model);

                if (success)
                {
                    SetSuccessMessage("Đổi mật khẩu thành công!");
                    return RedirectToAction(nameof(ChangePassword));
                }

                TempData["Error"] = errorMessage ?? "Không thể đổi mật khẩu. Vui lòng kiểm tra mật khẩu hiện tại và mật khẩu mới.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user");
                TempData["Error"] = "Không thể đổi mật khẩu lúc này. Vui lòng thử lại sau.";
            }

            return RedirectToAction(nameof(ChangePassword));
        }

        #endregion

        /// <summary>Khách hàng chỉnh sửa thông tin đơn vị (tổ chức).</summary>
        [HttpGet]
        public async Task<IActionResult> UpdateOrganization(bool returnToCheckout = false)
        {
            var customerId = await GetCurrentCustomerIdAsync();
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Auth");

            var organizations = await _organizationApiClient.GetAllAsync();
            ViewBag.Organizations = organizations.OrderBy(o => o.Organization_name).ToList();
            ViewBag.ReturnToCheckout = returnToCheckout;

            var org = await _customerApiClient.GetOrganizationByCustomerIdAsync(customerId.Value);
            var model = new UpdateOrganizationByCustomerDto
            {
                Customer_id = customerId.Value,
                Organization_information_id = org?.Organization_information_id ?? 0,
                Recipient_name = org?.Recipient_name,
                Recipient_phone = org?.Recipient_phone ?? "",
                Recipient_address = org?.Recipient_address
            };

            if ((org?.Organization_information_id ?? 0) > 0)
            {
                var currentOrg = await _organizationApiClient.GetByIdAsync(org!.Organization_information_id);
                ViewBag.CurrentOrg = currentOrg;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrganization(UpdateOrganizationByCustomerDto model, bool returnToCheckout = false)
        {
            var customerId = await GetCurrentCustomerIdAsync();
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Auth");

            model.Customer_id = customerId.Value;

            if (!ModelState.IsValid)
            {
                TempData["Error"] = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault(m => !string.IsNullOrEmpty(m)) ?? "Thông tin không hợp lệ. Vui lòng kiểm tra lại.";
                return RedirectToAction(nameof(UpdateOrganization), new { returnToCheckout });
            }

            try
            {
                var result = await _customerApiClient.UpdateOrganizationAsync(model);
                if (result)
                {
                    SetSuccessMessage("Cập nhật thông tin đơn vị thành công.");
                    if (returnToCheckout)
                        return RedirectToAction(nameof(Checkout));
                    return RedirectToAction(nameof(UpdateOrganization));
                }
                TempData["Error"] = "Không thể cập nhật thông tin đơn vị. Vui lòng kiểm tra dữ liệu và thử lại.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organization for customer {CustomerId}", customerId);
                TempData["Error"] = "Không thể cập nhật thông tin đơn vị. Vui lòng kiểm tra dữ liệu và thử lại.";
            }

            return RedirectToAction(nameof(UpdateOrganization), new { returnToCheckout });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrgInfo(UpdateOrganizationDto model, bool returnToCheckout = false)
        {
            var customerId = await GetCurrentCustomerIdAsync();
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault(m => !string.IsNullOrEmpty(m)) ?? "Thông tin không hợp lệ.";
                return RedirectToAction(nameof(UpdateOrganization), new { returnToCheckout });
            }

            try
            {
                var (success, errorMsg) = await _organizationApiClient.UpdateAsync(model.Id, model);
                if (success)
                {
                    SetSuccessMessage("Cập nhật thông tin đơn vị thành công.");
                    if (returnToCheckout)
                        return RedirectToAction(nameof(Checkout));
                    return RedirectToAction(nameof(UpdateOrganization));
                }
                TempData["Error"] = errorMsg ?? "Không thể cập nhật thông tin đơn vị. Vui lòng thử lại.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organization info {OrgId}", model.Id);
                TempData["Error"] = "Không thể cập nhật thông tin đơn vị. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(UpdateOrganization), new { returnToCheckout });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestOrgUpdate(CustomerOrgUpdateRequestDto model, bool returnToCheckout = false)
        {
            var customerId = await GetCurrentCustomerIdAsync();
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault(m => !string.IsNullOrEmpty(m)) ?? "Thông tin không hợp lệ.";
                return RedirectToAction(nameof(UpdateOrganization), new { returnToCheckout });
            }

            model.Customer_id = customerId.Value;

            try
            {
                var (success, errorMsg) = await _customerApiClient.RequestOrgUpdateAsync(customerId.Value, model);
                if (success)
                {
                    SetSuccessMessage("Yêu cầu chỉnh sửa đã được gửi thành công. Chúng tôi sẽ liên hệ với bạn sớm nhất.");
                    return RedirectToAction(nameof(UpdateOrganization), new { returnToCheckout });
                }
                TempData["Error"] = errorMsg ?? "Không thể gửi yêu cầu. Vui lòng thử lại.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending org update request for customer {CustomerId}", customerId);
                TempData["Error"] = "Không thể gửi yêu cầu. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(UpdateOrganization), new { returnToCheckout });
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
        [System.ComponentModel.DataAnnotations.MinLength(1, ErrorMessage = "Giỏ hàng trống.")]
        public List<CartItemRequest> Items { get; set; } = new();

        [System.ComponentModel.DataAnnotations.StringLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự.")]
        public string? Note { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(500, ErrorMessage = "Địa chỉ giao hàng không được vượt quá 500 ký tự.")]
        public string? ShippingAddress { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(100, ErrorMessage = "Tên người nhận không được vượt quá 100 ký tự.")]
        public string? RecipientName { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
        public string? RecipientPhone { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(200, ErrorMessage = "Email không được vượt quá 200 ký tự.")]
        [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string? RecipientEmail { get; set; }
    }

    public class CartItemRequest
    {
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "Mã sản phẩm không hợp lệ.")]
        public int ProductId { get; set; }

        [System.ComponentModel.DataAnnotations.Range(1, 10000, ErrorMessage = "Số lượng phải từ 1 đến 10000.")]
        public int Quantity { get; set; }

        [System.ComponentModel.DataAnnotations.Range(0, double.MaxValue, ErrorMessage = "Đơn giá không được âm.")]
        public decimal Price { get; set; }
    }
}
