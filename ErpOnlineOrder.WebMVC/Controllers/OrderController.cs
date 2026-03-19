using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.OrderDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Services;
using ErpOnlineOrder.WebMVC.Services.Interfaces;
using System.Text;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.OrderView)]
    public class OrderController : BaseController
    {
        private readonly IOrderApiClient _orderApiClient;
        private readonly IOrderService _orderService;
        private readonly ICustomerApiClient _customerApiClient;
        private readonly IProductApiClient _productApiClient;
        private readonly IPermissionApiClient _permissionApiClient;
        private readonly IInvoiceApiClient _invoiceApiClient;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderApiClient orderApiClient,
            IOrderService orderService,
            ICustomerApiClient customerApiClient,
            IProductApiClient productApiClient,
            IPermissionApiClient permissionApiClient,
            IInvoiceApiClient invoiceApiClient,
            ILogger<OrderController> logger)
        {
            _orderApiClient = orderApiClient;
            _orderService = orderService;
            _customerApiClient = customerApiClient;
            _productApiClient = productApiClient;
            _permissionApiClient = permissionApiClient;
            _invoiceApiClient = invoiceApiClient;
            _logger = logger;
        }

        private async Task<SelectList> GetCustomerSelectListAsync(int? selectedId = null)
        {
            var customers = await _customerApiClient.GetForSelectAsync();
            return new SelectList(customers, "Id", "Display_text", selectedId);
        }

        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string? status = null, string? search = null)
        {
            try
            {
                var result = await _orderApiClient.GetPagedAsync(page, pageSize, status, search);
                ViewBag.Status = status;
                ViewBag.PageSize = pageSize;
                ViewBag.Search = search;
                await LoadCurrentUserPermissions();
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return View(new PagedResult<OrderDTO> { Items = new List<OrderDTO>() });
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var order = await _orderService.GetByIdAsync(id, userId);
                if (order == null)
                {
                    SetErrorMessage("Không tìm thấy đơn hàng.");
                    return RedirectToAction(nameof(Index));
                }

                await LoadCurrentUserPermissions();
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order details");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>In đơn đặt hàng. Cán bộ phụ trách in được đơn của khách mình quản lý.</summary>
        [HttpGet]
        public async Task<IActionResult> PrintOrder(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var order = await _orderService.GetByIdAsync(id, userId);
            if (order == null)
            {
                SetErrorMessage("Không tìm thấy đơn hàng.");
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Shop/PrintOrder.cshtml", order);
        }

        [RequirePermission(PermissionCodes.OrderCreate)]
        public IActionResult Create() => RedirectToAction(nameof(Index));

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.OrderCreate)]
        public IActionResult Create(CreateOrderDto _) => RedirectToAction(nameof(Index));

        [RequirePermission(PermissionCodes.OrderUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var order = await _orderApiClient.GetByIdAsync(id);
                if (order == null)
                {
                    SetErrorMessage("Không tìm thấy đơn hàng.");
                    return RedirectToAction(nameof(Index));
                }
                if (order.Order_status != "Pending")
                {
                    SetErrorMessage("Chỉ có thể sửa đơn hàng đang chờ xử lý.");
                    return RedirectToAction(nameof(Details), new { id });
                }

                var model = new UpdateOrderDto
                {
                    Id = order.Id,
                    Order_code = order.Order_code,
                    Order_date = order.Order_date,
                    Shipping_address = order.Shipping_address,
                    note = order.note,
                    Order_details = order.Order_details.Select(od => new OrderDetailDto
                    {
                        Product_id = od.Product_id,
                        Quantity = od.Quantity,
                        Unit_price = od.Unit_price
                    }).ToList()
                };
                ViewBag.Order = order;
                ViewBag.Customers = await GetCustomerSelectListAsync();
                ViewBag.Products = await _productApiClient.GetForOrderAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.OrderUpdate)]
        public async Task<IActionResult> Edit(int id, UpdateOrderDto model)
        {
            try
            {
                if (model.Id != id)
                {
                    SetErrorMessage("Dữ liệu không hợp lệ.");
                    return RedirectToAction(nameof(Index));
                }

                var order = await _orderApiClient.GetByIdAsync(id);
                if (order == null)
                {
                    SetErrorMessage("Không tìm thấy đơn hàng.");
                    return RedirectToAction(nameof(Index));
                }
                if (order.Order_status != "Pending")
                {
                    SetErrorMessage("Chỉ có thể sửa đơn hàng đang chờ xử lý.");
                    return RedirectToAction(nameof(Details), new { id });
                }
                if (model.Order_details == null || !model.Order_details.Any())
                {
                    SetErrorMessage("Đơn hàng phải có ít nhất một sản phẩm.");
                    ViewBag.Order = order;
                    ViewBag.Customers = await GetCustomerSelectListAsync();
                    ViewBag.Products = await _productApiClient.GetForOrderAsync();
                    return View(model);
                }

                model.Updated_by = GetCurrentUserId();
                var (success, errorMessage) = await _orderApiClient.UpdateOrderAsync(model);

                if (success)
                {
                    SetSuccessMessage("Cập nhật đơn hàng thành công!");
                    return RedirectToAction(nameof(Details), new { id });
                }
                SetErrorMessage(errorMessage ?? "Không thể cập nhật đơn hàng. Vui lòng kiểm tra trạng thái đơn và dữ liệu chi tiết sản phẩm.");
                ViewBag.Order = order;
                ViewBag.Customers = await GetCustomerSelectListAsync();
                ViewBag.Products = await _productApiClient.GetForOrderAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.OrderApprove)]
        public async Task<IActionResult> Confirm(int id, ConfirmOrderDto model)
        {
            try
            {
                model.OrderId = id;
                var result = await _orderApiClient.ConfirmOrderAsync(id, model);
                if (!result.Success)
                {
                    SetErrorMessage(string.IsNullOrWhiteSpace(result.Message)
                        ? "Không thể xác nhận đơn hàng. Đơn hàng có thể không ở trạng thái chờ xử lý."
                        : result.Message);
                    return RedirectToAction(nameof(Details), new { id });
                }

                if (string.Equals(model.Notify_method, "download", StringComparison.OrdinalIgnoreCase))
                {
                    var order = await _orderApiClient.GetByIdAsync(id);
                    if (order == null)
                    {
                        SetSuccessMessage(result.Message);
                        return RedirectToAction(nameof(Index));
                    }

                    var csv = BuildApprovedOrderCsv(order);
                    var fileName = $"DonDuyet_{order.Order_code}_{DateTime.Now:yyyyMMddHHmmss}.csv";
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv; charset=utf-8", fileName);
                }

                if (result.Is_split && !string.IsNullOrWhiteSpace(result.Child_order_code))
                    SetSuccessMessage($"{result.Message} Đơn con: {result.Child_order_code}.");
                else
                    SetSuccessMessage(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming order");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.OrderApprove)]
        public async Task<IActionResult> GetConfirmItems(int id)
        {
            var order = await _orderApiClient.GetByIdAsync(id);
            if (order == null || order.Order_status != "Pending")
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng hoặc đơn hàng không thể duyệt." });
            }

            var items = order.Order_details.Select(x => new
            {
                product_id = x.Product_id,
                product_name = x.Product_name,
                quantity = x.Quantity,
                unit_price = x.Unit_price,
                total_price = x.Total_price
            });

            return Json(new
            {
                order_id = order.Id,
                order_code = order.Order_code,
                items
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.OrderReject)]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var result = await _orderApiClient.CancelOrderAsync(id);
                if (result)
                    SetSuccessMessage("Đã hủy đơn hàng thành công!");
                else
                    SetErrorMessage("Không thể hủy đơn hàng. Đơn hàng có thể đã được xác nhận hoặc đã xử lý ở bước tiếp theo.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling order");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.OrderDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _orderApiClient.DeletePendingOrderAsync(id);
                if (result)
                    SetSuccessMessage("Đã xóa đơn hàng thành công!");
                else
                    SetErrorMessage("Không thể xóa đơn hàng! Chỉ có thể xóa đơn hàng đang chờ xử lý.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Index));
        }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // [RequirePermission(PermissionCodes.InvoiceCreate)]
        // public async Task<IActionResult> CreateInvoice(int id)
        // {
        //     try
        //     {
        //         var result = await _invoiceApiClient.CreateFromOrderAsync(id);
        //         if (result?.Success == true)
        //             SetSuccessMessage(result.Message);
        //         else
        //             SetErrorMessage(result?.Message ?? "Không thể tạo hóa đơn từ đơn hàng. Vui lòng kiểm tra trạng thái đơn hàng và dữ liệu chi tiết.");
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error creating invoice from order {OrderId}", id);
        //         SetErrorMessage(GetDetailedErrorMessage(ex));
        //     }

        //     return RedirectToAction(nameof(Details), new { id });
        // }

        [RequirePermission(PermissionCodes.OrderExport)]
        public async Task<IActionResult> ExportExcel()
        {
            try
            {
                var bytes = await _orderService.ExportOrdersToExcelAsync();
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"DonHang_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting orders");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task LoadCurrentUserPermissions()
        {
            try
            {
                var roles = HttpContext.Session.GetString("Roles") ?? "";
                if (roles.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains("ROLE_ADMIN"))
                {
                    ViewBag.CurrentUserPermissions = new List<string> { "ALL" };
                    ViewBag.CanCreate = true;
                    ViewBag.CanUpdate = true;
                    ViewBag.CanDelete = true;
                    ViewBag.CanApprove = true;
                    ViewBag.CanReject = true;
                    ViewBag.CanExport = true;
                    ViewBag.CanCreateInvoice = true;
                    return;
                }

                var userId = GetCurrentUserId();
                var permissions = await _permissionApiClient.GetUserPermissionCodesAsync(userId);
                ViewBag.CurrentUserPermissions = permissions?.ToList() ?? new List<string>();

                ViewBag.CanCreate = permissions?.Contains(PermissionCodes.OrderCreate) ?? false;
                ViewBag.CanUpdate = permissions?.Contains(PermissionCodes.OrderUpdate) ?? false;
                ViewBag.CanDelete = permissions?.Contains(PermissionCodes.OrderDelete) ?? false;
                ViewBag.CanApprove = permissions?.Contains(PermissionCodes.OrderApprove) ?? false;
                ViewBag.CanReject = permissions?.Contains(PermissionCodes.OrderReject) ?? false;
                ViewBag.CanExport = permissions?.Contains(PermissionCodes.OrderExport) ?? false;
                ViewBag.CanCreateInvoice = permissions?.Contains(PermissionCodes.InvoiceCreate) ?? false;
            }
            catch
            {
                ViewBag.CurrentUserPermissions = new List<string>();
                ViewBag.CanCreate = false;
                ViewBag.CanUpdate = false;
                ViewBag.CanDelete = false;
                ViewBag.CanApprove = false;
                ViewBag.CanReject = false;
                ViewBag.CanExport = false;
                ViewBag.CanCreateInvoice = false;
            }
        }

        private static string BuildApprovedOrderCsv(OrderDTO order)
        {
            var sb = new StringBuilder();
            sb.AppendLine("MaDon,MaSanPham,TenSanPham,SoLuong,DonGia,ThanhTien");
            foreach (var item in order.Order_details)
            {
                var productName = (item.Product_name ?? string.Empty).Replace('"', '\'');
                sb.AppendLine($"{order.Order_code},{item.Product_id},\"{productName}\",{item.Quantity},{item.Unit_price:0.##},{item.Total_price:0.##}");
            }

            return sb.ToString();
        }
    }
}
