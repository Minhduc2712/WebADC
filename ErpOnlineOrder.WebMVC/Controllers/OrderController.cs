using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.OrderDTOs;
using ErpOnlineOrder.WebMVC.Attributes;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.OrderView)]
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductService _productService;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            ICustomerRepository customerRepository,
            IProductService productService,
            IPermissionService permissionService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _customerRepository = customerRepository;
            _productService = productService;
            _permissionService = permissionService;
            _logger = logger;
        }

        private async Task<SelectList> GetCustomerSelectListAsync(int? selectedId = null)
        {
            var customers = await _customerRepository.GetAllAsync();
            var items = customers
                .Where(c => !c.Is_deleted)
                .Select(c => new { c.Id, DisplayText = string.IsNullOrEmpty(c.Full_name) ? (c.Customer_code ?? $"Khách hàng #{c.Id}") : $"{c.Customer_code} - {c.Full_name}" })
                .ToList();
            return new SelectList(items, "Id", "DisplayText", selectedId);
        }

        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        public async Task<IActionResult> Index(string? status)
        {
            try
            {
                IEnumerable<OrderDTO> orders;

                if (!string.IsNullOrEmpty(status))
                {
                    orders = await _orderService.GetOrdersByStatusAsync(status);
                }
                else
                {
                    orders = await _orderService.GetAllAsync();
                }

                ViewBag.Status = status;
                await LoadCurrentUserPermissions();
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return View(Enumerable.Empty<OrderDTO>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(id);
                if (order == null)
                    return NotFound();

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

        [RequirePermission(PermissionCodes.OrderCreate)]
        public async Task<IActionResult> Create()
        {
            try
            {
                ViewBag.Customers = await GetCustomerSelectListAsync();
                ViewBag.Products = await _productService.GetAllAsync();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create form");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.OrderCreate)]
        public async Task<IActionResult> Create(CreateOrderDto model)
        {
            try
            {
                if (model.Customer_id == null)
                    ModelState.AddModelError(nameof(CreateOrderDto.Customer_id), "Vui lòng chọn khách hàng.");

                if (!ModelState.IsValid)
                {
                    ViewBag.Customers = await GetCustomerSelectListAsync(model.Customer_id);
                    ViewBag.Products = await _productService.GetAllAsync();
                    return View(model);
                }

                var dto = new CreateOrderDto
                {
                    Order_date = model.Order_date,
                    Customer_id = model.Customer_id!.Value,
                    Shipping_address = model.Shipping_address,
                    note = model.note,
                    Order_details = model.Order_details ?? new List<OrderDetailDto>()
                };
                var result = await _orderService.CreateOrderWithoutValidationAsync(dto);

                if (result.Success)
                {
                    SetSuccessMessage("Tạo đơn hàng thành công!");
                    return RedirectToAction(nameof(Details), new { id = result.Order_id });
                }
                else
                {
                    SetErrorMessage(result.Message ?? "Tạo đơn hàng thất bại!");
                    ViewBag.Customers = await GetCustomerSelectListAsync(model.Customer_id);
                    ViewBag.Products = await _productService.GetAllAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.OrderApprove)]
        public async Task<IActionResult> Confirm(int id)
        {
            try
            {
                var result = await _orderService.ConfirmOrderAsync(id);
                if (result)
                    SetSuccessMessage("Đã xác nhận đơn hàng thành công!");
                else
                    SetErrorMessage("Không thể xác nhận đơn hàng!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming order");
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.OrderReject)]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var result = await _orderService.CancelOrderAsync(id);
                if (result)
                    SetSuccessMessage("Đã hủy đơn hàng thành công!");
                else
                    SetErrorMessage("Không thể hủy đơn hàng!");
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
                var result = await _orderService.DeletePendingOrderAsync(id);
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
                    return;
                }

                var userId = GetCurrentUserId();
                var permissions = await _permissionService.GetUserPermissionsAsync(userId);
                ViewBag.CurrentUserPermissions = permissions?.ToList() ?? new List<string>();

                ViewBag.CanCreate = permissions?.Contains(PermissionCodes.OrderCreate) ?? false;
                ViewBag.CanUpdate = permissions?.Contains(PermissionCodes.OrderUpdate) ?? false;
                ViewBag.CanDelete = permissions?.Contains(PermissionCodes.OrderDelete) ?? false;
                ViewBag.CanApprove = permissions?.Contains(PermissionCodes.OrderApprove) ?? false;
                ViewBag.CanReject = permissions?.Contains(PermissionCodes.OrderReject) ?? false;
                ViewBag.CanExport = permissions?.Contains(PermissionCodes.OrderExport) ?? false;
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
            }
        }
    }
}
