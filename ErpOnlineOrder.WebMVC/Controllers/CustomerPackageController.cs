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
        private readonly IPermissionApiClient _permissionApiClient;
        private readonly ICustomerManagementApiClient _customerManagementApiClient;
        private readonly ILogger<CustomerPackageController> _logger;

        public CustomerPackageController(
            ICustomerPackageApiClient customerPackageApiClient,
            ICustomerApiClient customerApiClient,
            IPackageApiClient packageApiClient,
            IPermissionApiClient permissionApiClient,
            ICustomerManagementApiClient customerManagementApiClient,
            ILogger<CustomerPackageController> logger)
        {
            _customerPackageApiClient = customerPackageApiClient;
            _customerApiClient = customerApiClient;
            _packageApiClient = packageApiClient;
            _permissionApiClient = permissionApiClient;
            _customerManagementApiClient = customerManagementApiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int customerId)
        {
            try
            {
                // var customer = await _customerApiClient.GetByIdAsync(customerId);
                // if (customer == null)
                // {
                //     SetErrorMessage("Không tìm thấy khách hàng.");
                //     return RedirectToAction("Index", "CustomerManagement");
                // }

                var roles = HttpContext.Session.GetString("Roles") ?? "";
                bool isAdmin = roles.Contains("ROLE_ADMIN");
                int? filterStaffId = isAdmin ? null : GetCurrentUserId();
                
                // if (!roles.Contains("ROLE_ADMIN"))
                // {
                //     var assignments = await _customerManagementApiClient.GetByCustomerAsync(customerId);
                //     int currentUserId = GetCurrentUserId();
                //     if (!assignments.Any(a => a.Staff?.User_id == currentUserId))
                //     {
                //         SetErrorMessage("Bạn không có quyền xem gói sản phẩm của khách hàng này.");
                //         return RedirectToAction("Index", "CustomerManagement");
                //     }
                // }

                var customerPackages = await _customerPackageApiClient.GetByCustomerIdAsync(customerId);
                ViewBag.Customer = customer;
                ViewBag.CanAssign = await HasPermissionAsync(PermissionCodes.CustomerPackageAssign);
                return View(customerPackages.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách gói của khách hàng {CustomerId}", customerId);
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction("Index", "CustomerManagement");
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
                _logger.LogError(ex, "Lỗi khi gán gói {PackageId} cho khách hàng {CustomerId}", packageId, customerId);
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction("AssignPackages", new { customerId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CustomerPackageAssign)]
        public async Task<IActionResult> Unassign(int id, int customerId)
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

            return RedirectToAction("Index", new { customerId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CustomerPackageAssign)]
        public async Task<IActionResult> ToggleActive(int id, int customerId)
        {
            try
            {
                var cp = await _customerPackageApiClient.GetByIdAsync(id);
                if (cp == null)
                {
                    SetErrorMessage("Không tìm thấy bản ghi.");
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
    }
}
