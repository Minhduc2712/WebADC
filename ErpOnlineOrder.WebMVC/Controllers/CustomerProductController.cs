using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.CustomerProductDTOs;
using ErpOnlineOrder.Application.DTOs.CustomerDTOs;
using ErpOnlineOrder.Application.DTOs.ProductDTOs;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Services;
using ErpOnlineOrder.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.CustomerProductView)]
    public class CustomerProductController : BaseController
    {
        private readonly ICustomerProductApiClient _customerProductApiClient;
        private readonly ICustomerApiClient _customerApiClient;
        private readonly IProductApiClient _productApiClient;
        private readonly ICategoryApiClient _categoryApiClient;
        private readonly IPermissionApiClient _permissionApiClient;
        private readonly ILogger<CustomerProductController> _logger;

        public CustomerProductController(
            ICustomerProductApiClient customerProductApiClient,
            ICustomerApiClient customerApiClient,
            IProductApiClient productApiClient,
            ICategoryApiClient categoryApiClient,
            IPermissionApiClient permissionApiClient,
            ILogger<CustomerProductController> logger)
        {
            _customerProductApiClient = customerProductApiClient;
            _customerApiClient = customerApiClient;
            _productApiClient = productApiClient;
            _categoryApiClient = categoryApiClient;
            _permissionApiClient = permissionApiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int customerId)
        {
            try
            {
                var customer = await _customerApiClient.GetByIdAsync(customerId);
                if (customer == null)
                {
                    SetErrorMessage("Không tìm thấy khách hàng.");
                    return RedirectToAction("Index", "CustomerManagement");
                }

                var customerProducts = await _customerProductApiClient.GetByCustomerIdAsync(customerId);
                ViewBag.Customer = customer;
                ViewBag.CanAssign = await HasPermissionAsync(PermissionCodes.CustomerProductAssign);
                return View(customerProducts.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách sản phẩm của khách hàng {CustomerId}", customerId);
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction("Index", "CustomerManagement");
            }
        }

        [RequirePermission(PermissionCodes.CustomerProductAssign)]
        public async Task<IActionResult> AssignProducts(int customerId, int page = 1, int pageSize = 20, string? search = null, int? categoryId = null)
        {
            try
            {
                var customer = await _customerApiClient.GetByIdAsync(customerId);
                if (customer == null)
                {
                    SetErrorMessage("Không tìm thấy khách hàng.");
                    return RedirectToAction("Index", "CustomerManagement");
                }

                var pagedProducts = await _productApiClient.GetPagedAsync(page, pageSize, search, categoryId);
                var assignedProducts = await _customerProductApiClient.GetByCustomerIdAsync(customerId);
                var assignedProductIds = assignedProducts.Select(cp => cp.Product_id).ToHashSet();

                ViewBag.Customer = customer;
                ViewBag.AssignedProductIds = assignedProductIds;
                ViewBag.PagedProducts = pagedProducts;
                ViewBag.Search = search;
                ViewBag.CategoryId = categoryId;
                ViewBag.PageSize = pageSize;
                ViewBag.CustomerId = customerId;
                ViewBag.Categories = new SelectList(await _categoryApiClient.GetAllAsync(), "Id", "Category_name", categoryId);

                return View("AssignProducts", new AssignProductsToCustomerDto { Customer_id = customerId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải form gán sản phẩm cho khách hàng {CustomerId}", customerId);
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction("Index", new { customerId });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CustomerProductAssign)]
        public async Task<IActionResult> AssignProducts(AssignProductsToCustomerDto model)
        {
            try
            {
                if (model.Product_ids == null || !model.Product_ids.Any())
                {
                    SetErrorMessage("Vui lòng chọn ít nhất một sản phẩm.");
                    return RedirectToAction("AssignProducts", new { customerId = model.Customer_id });
                }
                var (success, error) = await _customerProductApiClient.AssignProductsAsync(model);
                if (success)
                {
                    SetSuccessMessage("Gán sản phẩm thành công!");
                    return RedirectToAction("Index", new { customerId = model.Customer_id });
                }

                SetErrorMessage(error ?? "Không thể gán sản phẩm cho khách hàng. Vui lòng kiểm tra sản phẩm đã chọn và trạng thái phân quyền.");
                return RedirectToAction("AssignProducts", new { customerId = model.Customer_id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gán sản phẩm cho khách hàng {CustomerId}", model.Customer_id);
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction("AssignProducts", new { customerId = model.Customer_id });
            }
        }

        [RequirePermission(PermissionCodes.CustomerProductAssign)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var cp = await _customerProductApiClient.GetByIdAsync(id);
                if (cp == null)
                {
                    SetErrorMessage("Không tìm thấy bản ghi.");
                    return RedirectToAction("Index", "CustomerManagement");
                }

                var customer = await _customerApiClient.GetByIdAsync(cp.Customer_id);
                ViewBag.Customer = customer;
                return View(new UpdateCustomerProductDto
                {
                    Id = cp.Id,
                    Max_quantity = cp.Max_quantity,
                    Is_active = cp.Is_active
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải form sửa CustomerProduct {Id}", id);
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction("Index", "CustomerManagement");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CustomerProductAssign)]
        public async Task<IActionResult> Edit(int id, UpdateCustomerProductDto model)
        {
            try
            {
                if (id != model.Id)
                {
                    SetErrorMessage("ID không hợp lệ.");
                    return RedirectToAction("Index", "CustomerManagement");
                }

                var (success, error) = await _customerProductApiClient.UpdateAsync(id, model);
                if (success)
                {
                    SetSuccessMessage("Đã cập nhật.");
                    return RedirectToAction("Index", new { customerId = (await _customerProductApiClient.GetByIdAsync(id))?.Customer_id ?? 0 });
                }

                SetErrorMessage(error ?? "Không thể cập nhật cấu hình sản phẩm của khách hàng. Vui lòng kiểm tra dữ liệu số lượng và trạng thái bản ghi.");
                return RedirectToAction("Edit", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật CustomerProduct {Id}", id);
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction("Edit", new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CustomerProductAssign)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var cp = await _customerProductApiClient.GetByIdAsync(id);
                var customerId = cp?.Customer_id ?? 0;

                var (success, error) = await _customerProductApiClient.DeleteAsync(id);
                if (success)
                {
                    SetSuccessMessage("Đã xóa gán sản phẩm.");
                }
                else
                {
                    SetErrorMessage(error ?? "Không thể xóa gán sản phẩm. Bản ghi có thể không tồn tại hoặc đã bị thay đổi.");
                }

                return RedirectToAction("Index", new { customerId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa CustomerProduct {Id}", id);
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction("Index", "CustomerManagement");
            }
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
