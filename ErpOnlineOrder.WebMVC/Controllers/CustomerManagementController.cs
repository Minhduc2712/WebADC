using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.CustomerDTOs;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Services;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.CustomerView)]
    public class CustomerManagementController : BaseController
    {
        private readonly ICustomerApiClient _customerApiClient;
        private readonly ICustomerManagementApiClient _customerManagementApiClient;
        private readonly IAdminApiClient _adminApiClient;
        private readonly IProvinceApiClient _provinceApiClient;
        private readonly ILogger<CustomerManagementController> _logger;

        public CustomerManagementController(
            ICustomerApiClient customerApiClient,
            ICustomerManagementApiClient customerManagementApiClient,
            IAdminApiClient adminApiClient,
            IProvinceApiClient provinceApiClient,
            ILogger<CustomerManagementController> logger)
        {
            _customerApiClient = customerApiClient;
            _customerManagementApiClient = customerManagementApiClient;
            _adminApiClient = adminApiClient;
            _provinceApiClient = provinceApiClient;
            _logger = logger;
        }
        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }
        private void LoadCurrentUserPermissions()
        {
            var roles = HttpContext.Session.GetString("Roles") ?? "";
            if (roles.Contains("ROLE_ADMIN"))
            {
                ViewBag.CanCreate = true;
                ViewBag.CanUpdate = true;
                ViewBag.CanDelete = true;
                ViewBag.CanAssign = true;
                return;
            }

            var permissions = HttpContext.Session.GetString("Permissions") ?? "";
            ViewBag.CanCreate = permissions.Contains(PermissionCodes.CustomerCreate);
            ViewBag.CanUpdate = permissions.Contains(PermissionCodes.CustomerUpdate);
            ViewBag.CanDelete = permissions.Contains(PermissionCodes.CustomerDelete);
            ViewBag.CanAssign = permissions.Contains(PermissionCodes.CustomerAssign);
        }

        #region CRUD Khách hàng
        public async Task<IActionResult> Index(string? search)
        {
            try
            {
                var customers = await _customerApiClient.GetAllAsync();
                var allAssignments = await _customerManagementApiClient.GetAllAsync(null, null);
                ViewBag.Assignments = allAssignments.ToList();

                if (!string.IsNullOrEmpty(search))
                {
                    customers = customers.Where(c =>
                        (c.Full_name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (c.Customer_code?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (c.Phone_number?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                    );
                }

                ViewBag.Search = search;
                LoadCurrentUserPermissions();
                return View(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách khách hàng");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return View(Enumerable.Empty<CustomerDTO>());
            }
        }
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var customer = await _customerApiClient.GetByIdAsync(id);
                if (customer == null)
                    return NotFound();

                var assignments = await _customerManagementApiClient.GetByCustomerAsync(id);
                ViewBag.Assignments = assignments.ToList();

                LoadCurrentUserPermissions();
                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải chi tiết khách hàng {Id}", id);
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }
        [RequirePermission(PermissionCodes.CustomerCreate)]
        public IActionResult Create()
        {
            return View(new CreateCustomerDto { User_id = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CustomerCreate)]
        public async Task<IActionResult> Create(CreateCustomerDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                model.Customer_code = $"KH{DateTime.Now:yyyyMMddHHmmss}";
                var created = await _customerApiClient.CreateAsync(model);
                if (created != null)
                {
                    SetSuccessMessage("Thêm khách hàng thành công!");
                    return RedirectToAction(nameof(Index));
                }
                SetErrorMessage("Thêm khách hàng thất bại.");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm khách hàng");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return View(model);
            }
        }
        [RequirePermission(PermissionCodes.CustomerUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var customer = await _customerApiClient.GetByIdAsync(id);
                if (customer == null)
                    return NotFound();

                var updateDto = new UpdateCustomerByAdminDto
                {
                    Id = customer.Id,
                    Customer_code = customer.Customer_code,
                    Full_name = customer.Full_name,
                    Phone_number = customer.Phone_number,
                    Address = customer.Address
                };
                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải khách hàng {Id} để sửa", id);
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CustomerUpdate)]
        public async Task<IActionResult> Edit(int id, UpdateCustomerByAdminDto model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                    return View(model);

                var (success, _) = await _customerApiClient.UpdateAsync(id, model);
                if (success)
                    SetSuccessMessage("Cập nhật khách hàng thành công!");
                else
                    SetErrorMessage("Cập nhật khách hàng thất bại.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật khách hàng {Id}", id);
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return View(model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CustomerDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (success, _) = await _customerApiClient.DeleteAsync(id);
                if (success)
                    SetSuccessMessage("Xóa khách hàng thành công!");
                else
                    SetErrorMessage("Xóa khách hàng thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa khách hàng {Id}", id);
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Gán cán bộ phụ trách
        [HttpGet]
        [RequirePermission(PermissionCodes.CustomerAssign)]
        public async Task<IActionResult> StaffAssignment(string? search, int? staffId)
        {
            try
            {
                var assignments = await _customerManagementApiClient.GetAllAsync(staffId, null);

                if (!string.IsNullOrEmpty(search))
                {
                    assignments = assignments.Where(a =>
                        (a.Customer?.Full_name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (a.Customer?.Customer_code?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (a.Staff?.Full_name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (a.Staff?.Staff_code?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                    );
                }

                var staffList = await _adminApiClient.GetAllStaffAsync();
                ViewBag.StaffList = staffList.Where(s => s.Is_active).ToList();
                ViewBag.Search = search;
                ViewBag.SelectedStaffId = staffId;

                LoadCurrentUserPermissions();
                return View(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách gán cán bộ phụ trách");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return View(Enumerable.Empty<Customer_management>().ToList());
            }
        }
        [HttpGet]
        [RequirePermission(PermissionCodes.CustomerAssign)]
        public async Task<IActionResult> CreateAssignment()
        {
            try
            {
                await LoadAssignmentFormData();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang thêm gán cán bộ");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(StaffAssignment));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CustomerAssign)]
        public async Task<IActionResult> CreateAssignment(int Customer_id, int Staff_id, int Province_id)
        {
            try
            {
                if (Customer_id <= 0)
                {
                    SetErrorMessage("Vui lòng chọn khách hàng.");
                    await LoadAssignmentFormData();
                    return View();
                }
                if (Staff_id <= 0)
                {
                    SetErrorMessage("Vui lòng chọn cán bộ phụ trách.");
                    await LoadAssignmentFormData();
                    return View();
                }
                if (Province_id <= 0)
                {
                    SetErrorMessage("Vui lòng chọn tỉnh/thành phố.");
                    await LoadAssignmentFormData();
                    return View();
                }

                var created = await _customerManagementApiClient.AssignStaffAsync(Staff_id, Customer_id, Province_id);
                if (created != null)
                    SetSuccessMessage("Gán cán bộ phụ trách thành công!");
                else
                    SetErrorMessage("Gán cán bộ phụ trách thất bại.");
                return RedirectToAction(nameof(StaffAssignment));
            }
            catch (InvalidOperationException ex)
            {
                SetErrorMessage(ex.Message);
                await LoadAssignmentFormData();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gán cán bộ {StaffId} cho khách hàng {CustomerId}", Staff_id, Customer_id);
                SetErrorMessage(GetDetailedErrorMessage(ex));
                await LoadAssignmentFormData();
                return View();
            }
        }
        [HttpGet]
        [RequirePermission(PermissionCodes.CustomerAssign)]
        public async Task<IActionResult> EditAssignment(int id)
        {
            try
            {
                var assignment = await _customerManagementApiClient.GetByIdAsync(id);
                if (assignment == null)
                    return NotFound();

                await LoadAssignmentFormData();
                return View(assignment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang sửa gán cán bộ {Id}", id);
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(StaffAssignment));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CustomerAssign)]
        public async Task<IActionResult> EditAssignment(int id, int Staff_id, int Province_id)
        {
            try
            {
                var assignment = await _customerManagementApiClient.GetByIdAsync(id);
                if (assignment == null)
                    return NotFound();

                if (Staff_id <= 0)
                {
                    SetErrorMessage("Vui lòng chọn cán bộ phụ trách.");
                    await LoadAssignmentFormData();
                    return View(assignment);
                }
                if (Province_id <= 0)
                {
                    SetErrorMessage("Vui lòng chọn tỉnh/thành phố.");
                    await LoadAssignmentFormData();
                    return View(assignment);
                }

                if (assignment.Staff_id != Staff_id)
                {
                    var isExisting = await _customerManagementApiClient.IsAlreadyAssignedAsync(Staff_id, assignment.Customer_id);
                    if (isExisting)
                    {
                        SetErrorMessage("Cán bộ này đã được gán phụ trách khách hàng này rồi.");
                        await LoadAssignmentFormData();
                        return View(assignment);
                    }
                }

                assignment.Staff_id = Staff_id;
                assignment.Province_id = Province_id;
                assignment.Updated_by = GetCurrentUserId();

                var (success, _) = await _customerManagementApiClient.UpdateAsync(id, assignment);
                if (success)
                    SetSuccessMessage("Cập nhật gán cán bộ phụ trách thành công!");
                else
                    SetErrorMessage("Cập nhật thất bại.");
                return RedirectToAction(nameof(StaffAssignment));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật gán cán bộ {Id}", id);
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(StaffAssignment));
            }
        }
        [HttpGet]
        [RequirePermission(PermissionCodes.CustomerAssign)]
        public async Task<IActionResult> AssignStaff(int id)
        {
            try
            {
                var customer = await _customerApiClient.GetByIdAsync(id);
                if (customer == null)
                    return NotFound();

                var staffList = await _adminApiClient.GetAllStaffAsync();
                var provinces = await _provinceApiClient.GetAllAsync();
                var existingAssignments = await _customerManagementApiClient.GetByCustomerAsync(id);

                ViewBag.Customer = customer;
                ViewBag.StaffList = staffList.Where(s => s.Is_active).ToList();
                ViewBag.Provinces = provinces.OrderBy(p => p.Province_name).ToList();
                ViewBag.ExistingAssignments = existingAssignments.ToList();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang gán cán bộ cho khách hàng {Id}", id);
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CustomerAssign)]
        public async Task<IActionResult> AssignStaff(int id, int Staff_id, int Province_id)
        {
            try
            {
                if (Staff_id <= 0)
                {
                    SetErrorMessage("Vui lòng chọn cán bộ phụ trách.");
                    return RedirectToAction(nameof(AssignStaff), new { id });
                }

                if (Province_id <= 0)
                {
                    SetErrorMessage("Vui lòng chọn tỉnh/thành phố.");
                    return RedirectToAction(nameof(AssignStaff), new { id });
                }

                var created = await _customerManagementApiClient.AssignStaffAsync(Staff_id, id, Province_id);
                if (created != null)
                    SetSuccessMessage("Gán cán bộ phụ trách thành công!");
                else
                    SetErrorMessage("Gán cán bộ phụ trách thất bại.");
                return RedirectToAction(nameof(AssignStaff), new { id });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cán bộ {StaffId} đã được gán cho khách hàng {CustomerId}", Staff_id, id);
                SetErrorMessage(ex.Message);
                return RedirectToAction(nameof(AssignStaff), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gán cán bộ {StaffId} cho khách hàng {CustomerId}", Staff_id, id);
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return RedirectToAction(nameof(AssignStaff), new { id });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CustomerAssign)]
        public async Task<IActionResult> RemoveAssignment(int id, int customerId)
        {
            try
            {
                var (success, _) = await _customerManagementApiClient.DeleteAsync(id);
                if (success)
                    SetSuccessMessage("Xóa gán cán bộ phụ trách thành công!");
                else
                    SetErrorMessage("Xóa thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa gán cán bộ {Id}", id);
                SetErrorMessage(GetDetailedErrorMessage(ex));
            }

            if (customerId > 0)
                return RedirectToAction(nameof(AssignStaff), new { id = customerId });

            return RedirectToAction(nameof(StaffAssignment));
        }
        private async Task LoadAssignmentFormData()
        {
            var customers = await _customerApiClient.GetAllAsync();
            var staffList = await _adminApiClient.GetAllStaffAsync();
            var provinces = await _provinceApiClient.GetAllAsync();

            ViewBag.Customers = customers.Where(c => !c.Is_deleted).OrderBy(c => c.Full_name).ToList();
            ViewBag.StaffList = staffList.Where(s => s.Is_active).OrderBy(s => s.Full_name).ToList();
            ViewBag.Provinces = provinces.OrderBy(p => p.Province_name).ToList();
        }

        #endregion
    }
}
