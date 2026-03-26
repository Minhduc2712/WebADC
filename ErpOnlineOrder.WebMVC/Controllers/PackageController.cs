using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.PackageDTOs;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.PackageView)]
    public class PackageController : BaseController
    {
        private readonly IPackageApiClient _packageApiClient;
        private readonly IProductApiClient _productApiClient;
        private readonly IRegionApiClient _regionApiClient;
        private readonly IProvinceApiClient _provinceApiClient;
        private readonly IWardApiClient _wardApiClient;
        private readonly IOrganizationApiClient _organizationApiClient;
        private readonly ILogger<PackageController> _logger;

        public PackageController(
            IPackageApiClient packageApiClient,
            IProductApiClient productApiClient,
            IRegionApiClient regionApiClient,
            IProvinceApiClient provinceApiClient,
            IWardApiClient wardApiClient,
            IOrganizationApiClient organizationApiClient,
            ILogger<PackageController> logger)
        {
            _packageApiClient = packageApiClient;
            _productApiClient = productApiClient;
            _regionApiClient = regionApiClient;
            _provinceApiClient = provinceApiClient;
            _wardApiClient = wardApiClient;
            _organizationApiClient = organizationApiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var packages = await _packageApiClient.GetAllAsync();
                return View(packages.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách gói sản phẩm");
                SetErrorFromException(ex);
                return View(new List<PackageDto>());
            }
        }

        [RequirePermission(PermissionCodes.PackageView)]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var package = await _packageApiClient.GetByIdAsync(id);
                if (package == null)
                {
                    SetErrorMessage("Không tìm thấy gói sản phẩm.");
                    return RedirectToAction(nameof(Index));
                }

                var allProducts = await _productApiClient.GetAllAsync();
                var assignedProductIds = package.Products.Select(p => p.Product_id).ToHashSet();
                ViewBag.AvailableProducts = allProducts
                    .Where(p => !assignedProductIds.Contains(p.Id))
                    .ToList();

                return View(package);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải chi tiết gói {Id}", id);
                SetErrorFromException(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission(PermissionCodes.PackageCreate)]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            return View(new CreatePackageDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.PackageCreate)]
        public async Task<IActionResult> Create(CreatePackageDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadDropdownsAsync(model.Organization_information_id, model.Region_id, model.Province_id, model.Ward_id);
                    return View(model);
                }

                var (data, error) = await _packageApiClient.CreateAsync(model);
                if (data != null)
                {
                    SetSuccessMessage("Thêm gói sản phẩm thành công!");
                    return RedirectToAction(nameof(Details), new { id = data.Id });
                }

                ModelState.AddModelError("", error ?? "Không thể thêm gói sản phẩm. Vui lòng kiểm tra mã gói.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo gói sản phẩm");
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }

            await LoadDropdownsAsync(model.Organization_information_id, model.Region_id, model.Province_id, model.Ward_id);
            return View(model);
        }

        [RequirePermission(PermissionCodes.PackageUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var package = await _packageApiClient.GetByIdAsync(id);
                if (package == null)
                {
                    SetErrorMessage("Không tìm thấy gói sản phẩm.");
                    return RedirectToAction(nameof(Index));
                }

                var model = new UpdatePackageDto
                {
                    Package_name = package.Package_name,
                    Description = package.Description,
                    Organization_information_id = package.Organization_information_id,
                    Region_id = package.Region_id,
                    Province_id = package.Province_id,
                    Ward_id = package.Ward_id,
                    Is_active = package.Is_active
                };

                ViewBag.PackageCode = package.Package_code;
                ViewBag.PackageId = id;
                await LoadDropdownsAsync(package.Organization_information_id, package.Region_id, package.Province_id, package.Ward_id);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải form sửa gói {Id}", id);
                SetErrorFromException(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.PackageUpdate)]
        public async Task<IActionResult> Edit(int id, UpdatePackageDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.PackageId = id;
                    await LoadDropdownsAsync(model.Organization_information_id, model.Region_id, model.Province_id, model.Ward_id);
                    return View(model);
                }

                var (success, error) = await _packageApiClient.UpdateAsync(id, model);
                if (success)
                {
                    SetSuccessMessage("Cập nhật gói sản phẩm thành công!");
                    return RedirectToAction(nameof(Details), new { id });
                }

                ModelState.AddModelError("", error ?? "Không thể cập nhật gói sản phẩm.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật gói {Id}", id);
                ModelState.AddModelError("", GetDetailedErrorMessage(ex));
            }

            ViewBag.PackageId = id;
            await LoadDropdownsAsync(model.Organization_information_id, model.Region_id, model.Province_id, model.Ward_id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.PackageDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (success, error) = await _packageApiClient.DeleteAsync(id);
                if (success)
                    SetSuccessMessage("Đã xóa gói sản phẩm.");
                else
                    SetErrorMessage(error ?? "Không thể xóa gói sản phẩm.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa gói {Id}", id);
                SetErrorFromException(ex);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.PackageUpdate)]
        public async Task<IActionResult> AddProduct(int packageId, List<int> productIds)
        {
            if (productIds == null || !productIds.Any())
            {
                SetErrorMessage("Vui lòng chọn ít nhất một sản phẩm.");
                return RedirectToAction(nameof(Details), new { id = packageId });
            }

            int added = 0, skipped = 0;
            foreach (var productId in productIds)
            {
                try
                {
                    var dto = new CreatePackageProductDto { Product_id = productId, Is_active = true };
                    var (success, _) = await _packageApiClient.AddProductAsync(packageId, dto);
                    if (success) added++; else skipped++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi thêm sản phẩm {ProductId} vào gói {PackageId}", productId, packageId);
                    skipped++;
                }
            }

            if (added > 0)
                SetSuccessMessage(skipped > 0
                    ? $"Đã thêm {added} sản phẩm. {skipped} sản phẩm đã tồn tại trong gói."
                    : $"Đã thêm {added} sản phẩm vào gói.");
            else
                SetErrorMessage("Không thể thêm sản phẩm. Các sản phẩm đã tồn tại trong gói.");

            return RedirectToAction(nameof(Details), new { id = packageId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.PackageUpdate)]
        public async Task<IActionResult> RemoveProduct(int packageId, int productId)
        {
            try
            {
                var (success, error) = await _packageApiClient.RemoveProductAsync(packageId, productId);
                if (success)
                    SetSuccessMessage("Đã xóa sản phẩm khỏi gói.");
                else
                    SetErrorMessage(error ?? "Không thể xóa sản phẩm khỏi gói.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm {ProductId} khỏi gói {PackageId}", productId, packageId);
                SetErrorFromException(ex);
            }

            return RedirectToAction(nameof(Details), new { id = packageId });
        }

        private async Task LoadDropdownsAsync(int? orgId = null, int? regionId = null, int? provinceId = null, int? wardId = null)
        {
            var orgs = await _organizationApiClient.GetAllAsync();
            var regions = await _regionApiClient.GetAllAsync();
            ViewBag.Organizations = new SelectList(orgs, "Id", "Organization_name", orgId);
            ViewBag.Regions = new SelectList(regions, "Id", "Region_name", regionId);

            if (regionId.HasValue)
            {
                var provinces = await _provinceApiClient.GetByRegionIdAsync(regionId.Value);
                ViewBag.Provinces = new SelectList(provinces, "Id", "Province_name", provinceId);
            }
            else
            {
                ViewBag.Provinces = new SelectList(Enumerable.Empty<object>(), "Id", "Province_name");
            }

            if (provinceId.HasValue)
            {
                var wards = await _wardApiClient.GetByProvinceIdAsync(provinceId.Value);
                ViewBag.Wards = new SelectList(wards, "Id", "Ward_name", wardId);
            }
            else
            {
                ViewBag.Wards = new SelectList(Enumerable.Empty<object>(), "Id", "Ward_name");
            }
        }
    }
}
