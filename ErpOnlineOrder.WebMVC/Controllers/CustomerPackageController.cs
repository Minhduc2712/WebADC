using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.CustomerPackageDTOs;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.CustomerPackageView)]
    public class CustomerPackageController : BaseController
    {
        private readonly ICustomerPackageApiClient _customerPackageApiClient;
        private readonly ICustomerApiClient _customerApiClient;
        private readonly IPackageApiClient _packageApiClient;
        private readonly ICustomerProductApiClient _customerProductApiClient;
        private readonly IPermissionApiClient _permissionApiClient;
        private readonly ICustomerManagementApiClient _customerManagementApiClient;
        private readonly ILogger<CustomerPackageController> _logger;

        public CustomerPackageController(
            ICustomerPackageApiClient customerPackageApiClient,
            ICustomerApiClient customerApiClient,
            IPackageApiClient packageApiClient,
            ICustomerProductApiClient customerProductApiClient,
            IPermissionApiClient permissionApiClient,
            ICustomerManagementApiClient customerManagementApiClient,
            ILogger<CustomerPackageController> logger)
        {
            _customerPackageApiClient = customerPackageApiClient;
            _customerApiClient = customerApiClient;
            _packageApiClient = packageApiClient;
            _customerProductApiClient = customerProductApiClient;
            _permissionApiClient = permissionApiClient;
            _customerManagementApiClient = customerManagementApiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string? search = null)
        {
            try
            {
                var result = await _customerPackageApiClient.GetPagedAsync(page, pageSize, search);

                ViewBag.Search = search;
                ViewBag.PageSize = pageSize;
                ViewBag.CanAssign = await HasPermissionAsync(PermissionCodes.CustomerPackageAssign);
                if (ViewBag.CanAssign == true)
                    ViewBag.Customers = (await _customerApiClient.GetForSelectAsync()).ToList();
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách khách hàng - gói sản phẩm");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction("Index", "Home");
            }
        }

        [RequirePermission(PermissionCodes.CustomerPackageAssign)]
        public async Task<IActionResult> AssignPackages(int customerId, string? search = null)
        {
            try
            {
                var customer = await _customerApiClient.GetByIdAsync(customerId);
                if (customer == null)
                {
                    SetErrorMessage("Không tìm thấy khách hàng.");
                    return RedirectToAction("Index", "CustomerManagement");
                }

                // Guard: giống Index
                var roles = HttpContext.Session.GetString("Roles") ?? "";
                if (!roles.Contains("ROLE_ADMIN"))
                {
                    var assignments = await _customerManagementApiClient.GetByCustomerAsync(customerId);
                    int currentUserId = GetCurrentUserId();
                    if (!assignments.Any(a => a.Staff?.User_id == currentUserId))
                    {
                        SetErrorMessage("Bạn không có quyền gán gói sản phẩm cho khách hàng này.");
                        return RedirectToAction("Index", "CustomerManagement");
                    }
                }

                var allPackages = await _packageApiClient.GetAllAsync();
                var assignedPackages = await _customerPackageApiClient.GetByCustomerIdAsync(customerId);
                var assignedPackageIds = assignedPackages.Select(cp => cp.Package_id).ToHashSet();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    allPackages = allPackages.Where(p =>
                        p.Package_name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        p.Package_code.Contains(search, StringComparison.OrdinalIgnoreCase));
                }

                ViewBag.Customer = customer;
                ViewBag.AssignedPackageIds = assignedPackageIds;
                ViewBag.Search = search;
                ViewBag.CustomerId = customerId;

                return View(allPackages.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải form gán gói cho khách hàng {CustomerId}", customerId);
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction("Index", new { customerId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CustomerPackageAssign)]
        public async Task<IActionResult> Assign(int customerId, int packageId)
        {
            try
            {
                var dto = new CreateCustomerPackageDto
                {
                    Customer_id = customerId,
                    Package_id = packageId,
                    Is_active = true
                };

                var (data, error) = await _customerPackageApiClient.AssignPackageAsync(dto);
                if (data != null)
                {
                    SetSuccessMessage("Đã gán gói sản phẩm thành công!");
                }
                else
                {
                    SetErrorMessage(error ?? "Không thể gán gói sản phẩm. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gán gói cho khách hàng {CustomerId}", customerId);
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction("AssignPackages", new { customerId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CustomerPackageAssign)]
        public async Task<IActionResult> Unassign(int id, int customerId, bool returnToAll = false)
        {
            try
            {
                var (success, error) = await _customerPackageApiClient.UnassignPackageAsync(id);
                if (success)
                {
                    SetSuccessMessage("Đã hủy gán gói sản phẩm.");
                }
                else
                {
                    SetErrorMessage(error ?? "Không thể hủy gán gói sản phẩm. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hủy gán CustomerPackage {Id}", id);
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            if (returnToAll)
                return RedirectToAction("Index");
            return RedirectToAction("Index", new { customerId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CustomerPackageAssign)]
        public async Task<IActionResult> ToggleActive(int id, int customerId, bool returnToAll = false)
        {
            try
            {
                var cp = await _customerPackageApiClient.GetByIdAsync(id);
                if (cp == null)
                {
                    SetErrorMessage("Không tìm thấy bản ghi.");
                    if (returnToAll)
                        return RedirectToAction("Index");
                    return RedirectToAction("Index", new { customerId });
                }

                var dto = new UpdateCustomerPackageDto { Id = id, Is_active = !cp.Is_active };
                var (success, error) = await _customerPackageApiClient.UpdateAsync(id, dto);
                if (success)
                {
                    SetSuccessMessage(cp.Is_active ? "Đã tạm khóa gói." : "Đã kích hoạt gói.");
                }
                else
                {
                    SetErrorMessage(error ?? "Không thể cập nhật trạng thái gói.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái CustomerPackage {Id}", id);
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            if (returnToAll)
                return RedirectToAction("Index");
            return RedirectToAction("Index", new { customerId });
        }

        private async Task<bool> HasPermissionAsync(string permissionCode)
        {
            var roles = HttpContext.Session.GetString("Roles") ?? "";
            if (roles.Contains("ROLE_ADMIN")) return true;

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId == 0) return false;

            var permissions = await _permissionApiClient.GetUserPermissionCodesAsync(userId);
            return permissions?.Contains(permissionCode) ?? false;
        }

        public async Task<IActionResult> CustomerPackageProducts(int id)
        {
            try
            {
                var cp = await _customerPackageApiClient.GetByIdAsync(id);
                if (cp == null)
                {
                    SetErrorMessage("Không tìm thấy bản ghi khách hàng - gói.");
                    return RedirectToAction("Index");
                }

                // Guard: kiểm tra nhân viên có quyền quản lý khách hàng này không
                var roles = HttpContext.Session.GetString("Roles") ?? "";
                if (!roles.Contains("ROLE_ADMIN"))
                {
                    var assignments = await _customerManagementApiClient.GetByCustomerAsync(cp.Customer_id);
                    int currentUserId = GetCurrentUserId();
                    if (!assignments.Any(a => a.Staff?.User_id == currentUserId))
                    {
                        SetErrorMessage("Bạn không có quyền quản lý sản phẩm gói của khách hàng này.");
                        return RedirectToAction("Index");
                    }
                }

                var customer = await _customerApiClient.GetByIdAsync(cp.Customer_id);
                var package = await _packageApiClient.GetByIdAsync(cp.Package_id);
                if (package == null)
                {
                    SetErrorMessage("Không tìm thấy gói sản phẩm.");
                    return RedirectToAction("Index");
                }

                var excludedProductIds = await _customerProductApiClient.GetExcludedProductIdsAsync(cp.Customer_id);

                ViewBag.Customer = customer;
                ViewBag.CustomerPackage = cp;
                ViewBag.ExcludedProductIds = excludedProductIds.ToHashSet();
                ViewBag.CanAssign = await HasPermissionAsync(PermissionCodes.CustomerProductAssign);

                return View(package);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải sản phẩm gói cho CustomerPackage {Id}", id);
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CustomerProductAssign)]
        public async Task<IActionResult> ExcludeProduct(int customerPackageId, int customerId, int productId)
        {
            try
            {
                if (customerPackageId <= 0 || customerId <= 0 || productId <= 0)
                {
                    SetErrorMessage("Dữ liệu không hợp lệ.");
                    return RedirectToAction("Index");
                }

                // Kiểm tra customerPackageId tồn tại và customerId khớp
                var cp = await _customerPackageApiClient.GetByIdAsync(customerPackageId);
                if (cp == null || cp.Customer_id != customerId)
                {
                    SetErrorMessage("Bản ghi khách hàng - gói không hợp lệ.");
                    return RedirectToAction("Index");
                }

                var (success, error) = await _customerProductApiClient.ExcludeProductAsync(customerId, productId);
                if (success)
                    SetSuccessMessage("Đã loại bỏ sản phẩm khỏi gói của khách hàng.");
                else
                    SetErrorMessage(error ?? "Không thể loại bỏ sản phẩm.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi loại bỏ sản phẩm {ProductId} cho khách hàng {CustomerId}", productId, customerId);
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }
            return RedirectToAction("CustomerPackageProducts", new { id = customerPackageId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CustomerProductAssign)]
        public async Task<IActionResult> UnexcludeProduct(int customerPackageId, int customerId, int productId)
        {
            try
            {
                if (customerPackageId <= 0 || customerId <= 0 || productId <= 0)
                {
                    SetErrorMessage("Dữ liệu không hợp lệ.");
                    return RedirectToAction("Index");
                }

                var cp = await _customerPackageApiClient.GetByIdAsync(customerPackageId);
                if (cp == null || cp.Customer_id != customerId)
                {
                    SetErrorMessage("Bản ghi khách hàng - gói không hợp lệ.");
                    return RedirectToAction("Index");
                }

                var (success, error) = await _customerProductApiClient.UnexcludeProductAsync(customerId, productId);
                if (success)
                    SetSuccessMessage("Đã khôi phục sản phẩm trong gói của khách hàng.");
                else
                    SetErrorMessage(error ?? "Không thể khôi phục sản phẩm.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi khôi phục sản phẩm {ProductId} cho khách hàng {CustomerId}", productId, customerId);
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }
            return RedirectToAction("CustomerPackageProducts", new { id = customerPackageId });
        }
    }
}
