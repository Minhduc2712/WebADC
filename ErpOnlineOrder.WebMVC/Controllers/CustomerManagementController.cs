using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Attributes;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequirePermission(PermissionCodes.CustomerView)]
    public class CustomerManagementController : BaseController
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerManagementService _customerManagementService;
        private readonly IStaffRepository _staffRepository;
        private readonly IProvinceRepository _provinceRepository;
        private readonly ILogger<CustomerManagementController> _logger;

        public CustomerManagementController(
            ICustomerRepository customerRepository,
            ICustomerManagementService customerManagementService,
            IStaffRepository staffRepository,
            IProvinceRepository provinceRepository,
            ILogger<CustomerManagementController> logger)
        {
            _customerRepository = customerRepository;
            _customerManagementService = customerManagementService;
            _staffRepository = staffRepository;
            _provinceRepository = provinceRepository;
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
                var customers = await _customerRepository.GetAllAsync();
                
                // Lấy tất cả assignments để hiển thị cán bộ phụ trách
                var allAssignments = await _customerManagementService.GetAllAsync();
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
                return View(Enumerable.Empty<Customer>());
            }
        }
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(id);
                if (customer == null)
                    return NotFound();

                // Lấy danh sách cán bộ phụ trách khách hàng này
                var assignments = await _customerManagementService.GetByCustomerAsync(id);
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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionCodes.CustomerCreate)]
        public async Task<IActionResult> Create(Customer model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var userId = GetCurrentUserId();
                model.Created_by = userId;
                model.Updated_by = userId;
                model.Created_at = DateTime.Now;
                model.Updated_at = DateTime.Now;
                model.Customer_code = $"KH{DateTime.Now:yyyyMMddHHmmss}";

                await _customerRepository.AddAsync(model);
                SetSuccessMessage("Thêm khách hàng thành công!");
                return RedirectToAction(nameof(Index));
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
                var customer = await _customerRepository.GetByIdAsync(id);
                if (customer == null)
                    return NotFound();

                return View(customer);
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
        public async Task<IActionResult> Edit(int id, Customer model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var existing = await _customerRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFound();
                }

                var userId = GetCurrentUserId();
                existing.Customer_code = model.Customer_code;
                existing.Full_name = model.Full_name;
                existing.Phone_number = model.Phone_number;
                existing.Address = model.Address;
                existing.Updated_by = userId;
                existing.Updated_at = DateTime.Now;

                await _customerRepository.UpdateAsync(existing);
                SetSuccessMessage("Cập nhật khách hàng thành công!");
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
                await _customerRepository.DeleteAsync(id);
                SetSuccessMessage("Xóa khách hàng thành công!");
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
                IEnumerable<Customer_management> assignments;
                
                if (staffId.HasValue && staffId.Value > 0)
                {
                    assignments = await _customerManagementService.GetByStaffAsync(staffId.Value);
                }
                else
                {
                    assignments = await _customerManagementService.GetAllAsync();
                }

                if (!string.IsNullOrEmpty(search))
                {
                    assignments = assignments.Where(a =>
                        (a.Customer?.Full_name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (a.Customer?.Customer_code?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (a.Staff?.Full_name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (a.Staff?.Staff_code?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                    );
                }

                // Lấy danh sách nhân viên cho bộ lọc
                var staffList = await _staffRepository.GetAllAsync();
                ViewBag.StaffList = staffList.Where(s => !s.Is_deleted).ToList();
                ViewBag.Search = search;
                ViewBag.SelectedStaffId = staffId;

                LoadCurrentUserPermissions();
                return View(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách gán cán bộ phụ trách");
                SetErrorMessage(GetDetailedErrorMessage(ex));
                return View(Enumerable.Empty<Customer_management>());
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

                var userId = GetCurrentUserId();
                await _customerManagementService.AssignStaffToCustomerAsync(Staff_id, Customer_id, Province_id, userId);

                SetSuccessMessage("Gán cán bộ phụ trách thành công!");
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
                var assignment = await _customerManagementService.GetByIdAsync(id);
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
                var assignment = await _customerManagementService.GetByIdAsync(id);
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

                // Kiểm tra nếu thay đổi cán bộ, đã có gán trùng chưa
                if (assignment.Staff_id != Staff_id)
                {
                    var isExisting = await _customerManagementService.IsAlreadyAssignedAsync(Staff_id, assignment.Customer_id);
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

                await _customerManagementService.UpdateCustomerManagementAsync(assignment);
                SetSuccessMessage("Cập nhật gán cán bộ phụ trách thành công!");
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
                var customer = await _customerRepository.GetByIdAsync(id);
                if (customer == null)
                    return NotFound();

                var staffList = await _staffRepository.GetAllAsync();
                var provinces = await _provinceRepository.GetAllAsync();
                var existingAssignments = await _customerManagementService.GetByCustomerAsync(id);

                ViewBag.Customer = customer;
                ViewBag.StaffList = staffList.Where(s => !s.Is_deleted).ToList();
                ViewBag.Provinces = provinces.Where(p => !p.Is_deleted).OrderBy(p => p.Province_name).ToList();
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

                var userId = GetCurrentUserId();
                await _customerManagementService.AssignStaffToCustomerAsync(Staff_id, id, Province_id, userId);
                
                SetSuccessMessage("Gán cán bộ phụ trách thành công!");
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
                await _customerManagementService.RemoveAssignmentAsync(id);
                SetSuccessMessage("Xóa gán cán bộ phụ trách thành công!");
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
            var customers = await _customerRepository.GetAllAsync();
            var staffList = await _staffRepository.GetAllAsync();
            var provinces = await _provinceRepository.GetAllAsync();

            ViewBag.Customers = customers.Where(c => !c.Is_deleted).OrderBy(c => c.Full_name).ToList();
            ViewBag.StaffList = staffList.Where(s => !s.Is_deleted).OrderBy(s => s.Full_name).ToList();
            ViewBag.Provinces = provinces.Where(p => !p.Is_deleted).OrderBy(p => p.Province_name).ToList();
        }

        #endregion
    }
}
