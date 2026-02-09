using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.OrderDTOs;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    public class ShopController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<ShopController> _logger;

        public ShopController(
            IProductService productService,
            IOrderService orderService,
            ICustomerRepository customerRepository,
            ILogger<ShopController> logger)
        {
            _productService = productService;
            _orderService = orderService;
            _customerRepository = customerRepository;
            _logger = logger;
        }

        private int GetCurrentUserId() => HttpContext.Session.GetInt32("UserId") ?? 0;

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productService.GetAllAsync();
                var productList = products.ToList();
                ViewData["FeaturedProducts"] = productList.Take(8).ToList();
                ViewData["NewArrivals"] = productList.OrderByDescending(p => p.Id).Take(8).ToList();

                var categories = productList
                    .SelectMany(p => p.Categories)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
                ViewData["ShopCategories"] = categories;

                return View(productList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shop homepage");
                return View(new List<ProductDTO>());
            }
        }

        public async Task<IActionResult> Products(string? search, string? category, string? sort)
        {
            try
            {
                IEnumerable<ProductDTO> products;

                if (!string.IsNullOrEmpty(search))
                {
                    products = await _productService.SearchAsync(search, null, null);
                }
                else
                {
                    products = await _productService.GetAllAsync();
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

                var allProducts = await _productService.GetAllAsync();
                var categories = allProducts
                    .SelectMany(p => p.Categories)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
                ViewBag.Categories = categories;

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
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                    return NotFound();

                var allProducts = await _productService.GetAllAsync();
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

                var result = await _orderService.CancelOrderAsync(id);
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

        public IActionResult Account()
        {
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
