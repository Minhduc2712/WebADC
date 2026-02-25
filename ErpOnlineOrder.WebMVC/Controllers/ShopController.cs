using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.OrderDTOs;
using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;
using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.WebMVC.Extensions;
using ErpOnlineOrder.WebMVC.Attributes;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequireAuth]
    public class ShopController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IWarehouseExportService _warehouseExportService;
        private readonly IInvoiceService _invoiceService;
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<ShopController> _logger;

        public ShopController(
            IProductService productService,
            IOrderService orderService,
            IWarehouseExportService warehouseExportService,
            IInvoiceService invoiceService,
            ICustomerRepository customerRepository,
            ILogger<ShopController> logger)
        {
            _productService = productService;
            _orderService = orderService;
            _warehouseExportService = warehouseExportService;
            _invoiceService = invoiceService;
            _customerRepository = customerRepository;
            _logger = logger;
        }

        private int GetCurrentUserId() => HttpContext.Session.GetInt32("UserId") ?? 0;

        private async Task<int?> GetCurrentCustomerIdAsync()
        {
            if (!HttpContext.Session.IsCustomer()) return null;
            var userId = GetCurrentUserId();
            if (userId == 0) return null;
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            return customer?.Id;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(Products));
        }

        public async Task<IActionResult> Products(string? search, string? category, string? sort)
        {
            try
            {
                var customerId = await GetCurrentCustomerIdAsync();
                IEnumerable<ProductDTO> products;

                if (!string.IsNullOrEmpty(search))
                {
                    products = await _productService.SearchByAllForShopAsync(search, customerId);
                }
                else
                {
                    products = await _productService.GetProductsForShopAsync(customerId);
                }

                if (!string.IsNullOrEmpty(category))
                {
                    products = products.Where(p => p.Categories.Contains(category));
                }

                switch (sort)
                {
                    case "price_asc":
                        products = products.OrderBy(p => p.Product_price);
                        break;
                    case "price_desc":
                        products = products.OrderByDescending(p => p.Product_price);
                        break;
                    case "name_asc":
                        products = products.OrderBy(p => p.Product_name);
                        break;
                    case "newest":
                        products = products.OrderByDescending(p => p.Id);
                        break;
                    default:
                        break;
                }

                ViewBag.Search = search;
                ViewBag.Category = category;
                ViewBag.Sort = sort;

                var allProducts = await _productService.GetProductsForShopAsync(customerId);
                var categories = allProducts
                    .SelectMany(p => p.Categories)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
                ViewBag.Categories = categories;
                ViewBag.TotalCount = allProducts.Count();

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products");
                return View(Enumerable.Empty<ProductDTO>());
            }
        }

        public async Task<IActionResult> ProductDetail(int id)
        {
            try
            {
                var customerId = await GetCurrentCustomerIdAsync();
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                    return NotFound();

                // Khách hàng chỉ xem được sản phẩm đã được gán
                if (customerId.HasValue)
                {
                    var hasAccess = await _productService.IsProductAssignedToCustomerAsync(id, customerId.Value);
                    if (!hasAccess)
                        return NotFound();
                }

                var allProducts = await _productService.GetProductsForShopAsync(customerId);
                ViewData["RelatedProducts"] = allProducts
                    .Where(p => p.Categories.Intersect(product.Categories).Any() && p.Id != id)
                    .Take(4)
                    .ToList();

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

        public IActionResult Checkout()
        {
            return View();
        }

        public async Task<IActionResult> Orders()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var customer = await _customerRepository.GetByUserIdAsync(userId);
                if (customer == null)
                {
                    ViewData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                    return View(new List<OrderDTO>());
                }

                var orders = await _orderService.GetOrdersByCustomerAsync(customer.Id);
                return View(orders.OrderByDescending(o => o.Order_date).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders for user {UserId}", userId);
                return View(new List<OrderDTO>());
            }
        }

        /// <summary>In đơn đặt hàng theo mẫu. Chỉ khách hàng sở hữu đơn mới in được.</summary>
        [HttpGet]
        public async Task<IActionResult> PrintOrder(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return NotFound();

            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return NotFound();

            var myOrders = await _orderService.GetOrdersByCustomerAsync(customer.Id);
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

                var customer = await _customerRepository.GetByUserIdAsync(userId);
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
                    Created_by = userId,
                    Order_details = request.Items.Select(item => new OrderDetailDto
                    {
                        Product_id = item.ProductId,
                        Quantity = item.Quantity,
                        Unit_price = item.Price
                    }).ToList()
                };

                var result = await _orderService.CreateOrderWithoutValidationAsync(createOrderDto);

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
                return Json(new { success = false, message = "Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại." });
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

                var customer = await _customerRepository.GetByUserIdAsync(userId);
                if (customer == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng." });
                }

                // Verify order belongs to this customer
                var order = await _orderService.GetByIdAsync(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
                }

                var result = await _orderService.CancelOrderAsync(new CancelOrderDto { OrderId = id, Updated_by = userId });
                if (result)
                {
                    return Json(new { success = true, message = "Hủy đơn hàng thành công." });
                }
                return Json(new { success = false, message = "Không thể hủy đơn hàng này." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra." });
            }
        }

        /// <summary>Danh sách hóa đơn của khách hàng.</summary>
        public async Task<IActionResult> Invoices()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
            {
                ViewData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return View(new List<InvoiceDto>());
            }

            try
            {
                var invoices = await _invoiceService.GetByCustomerIdAsync(customer.Id);
                var list = invoices.OrderByDescending(i => i.Invoice_date).ToList();
                return View(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading invoices for customer {CustomerId}", customer.Id);
                return View(new List<InvoiceDto>());
            }
        }

        /// <summary>Danh sách phiếu xuất kho của khách hàng.</summary>
        public async Task<IActionResult> Exports()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
            {
                ViewData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return View(new List<WarehouseExportDto>());
            }

            try
            {
                var exports = await _warehouseExportService.GetByCustomerIdAsync(customer.Id);
                var list = exports
                    .Where(e => e.Status != "Cancelled")
                    .OrderByDescending(e => e.Export_date)
                    .ToList();
                return View(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading exports for customer {CustomerId}", customer.Id);
                return View(new List<WarehouseExportDto>());
            }
        }

        /// <summary>Yêu cầu tách hóa đơn từ phiếu xuất kho - khách chỉ định số lượng cụ thể cho từng sản phẩm.</summary>
        [HttpGet]
        public async Task<IActionResult> RequestInvoiceSplit(int exportId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return NotFound();

            var export = await _warehouseExportService.GetByIdAsync(exportId);
            if (export == null)
                return NotFound();

            if (export.Customer_id != customer.Id)
                return Forbid();

            if (export.Status == "Cancelled")
            {
                TempData["Error"] = "Phiếu xuất kho đã hủy.";
                return RedirectToAction(nameof(Exports));
            }

            var invoice = await _invoiceService.GetByIdAsync(export.Invoice_id);
            if (invoice == null)
            {
                TempData["Error"] = "Không tìm thấy hóa đơn liên quan.";
                return RedirectToAction(nameof(Exports));
            }

            if (invoice.Status == "Split" || invoice.Status == "Merged")
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

            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return NotFound();

            var export = await _warehouseExportService.GetByIdAsync(exportId);
            if (export == null)
                return NotFound();

            if (export.Customer_id != customer.Id)
                return Forbid();

            var invoice = await _invoiceService.GetByIdAsync(export.Invoice_id);
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
                var result = await _invoiceService.SplitInvoiceAsync(dto, userId);
                if (result?.Success == true)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Exports));
                }
                TempData["Error"] = result?.Message ?? "Tách hóa đơn thất bại.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error splitting invoice for customer");
                TempData["Error"] = "Có lỗi xảy ra khi tách hóa đơn.";
            }

            ViewBag.Export = export;
            ViewBag.Invoice = invoice;
            return View(invoice);
        }

        public IActionResult Account()
        {
            return View();
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
