using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.CustomerDTOs;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Extensions;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequireAuth]
    public class CustomerController : BaseController
    {
        private readonly ICustomerApiClient _customerApiClient;
        private readonly IOrganizationApiClient _organizationApiClient;
        private readonly HttpClient _httpClient;

        public CustomerController(
            ICustomerApiClient customerApiClient,
            IOrganizationApiClient organizationApiClient,
            IHttpClientFactory httpClientFactory)
        {
            _customerApiClient = customerApiClient;
            _organizationApiClient = organizationApiClient;
            _httpClient = httpClientFactory.CreateClient("ErpApi");
        }

        [HttpGet]
        public IActionResult UpdateCustomer() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCustomer(UpdateCustomerDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _httpClient.PutAsJsonAsync("customer/update-customer", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Cập nhật thông tin khách hàng thành công.";
                return RedirectToAction("Index", "Home");
            }
            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", error);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateOrganization()
        {
            var organizations = await _organizationApiClient.GetAllAsync();
            ViewBag.Organizations = organizations.OrderBy(o => o.Organization_name).ToList();

            var userId = GetCurrentUserId();
            var customer = await _customerApiClient.GetByUserIdAsync(userId);
            if (customer != null)
            {
                var orgView = await _customerApiClient.GetOrganizationByCustomerIdAsync(customer.Id);
                var model = new UpdateOrganizationByCustomerDto
                {
                    Customer_id = customer.Id,
                    Organization_information_id = orgView?.Organization_information_id ?? 0,
                    Recipient_name = orgView?.Recipient_name,
                    Recipient_phone = orgView?.Recipient_phone ?? string.Empty,
                    Recipient_address = orgView?.Recipient_address
                };
                return View(model);
            }

            return View(new UpdateOrganizationByCustomerDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrganization(UpdateOrganizationByCustomerDto model)
        {
            if (!ModelState.IsValid)
            {
                var organizations = await _organizationApiClient.GetAllAsync();
                ViewBag.Organizations = organizations.OrderBy(o => o.Organization_name).ToList();
                return View(model);
            }

            var success = await _customerApiClient.UpdateOrganizationAsync(model);
            if (success)
            {
                TempData["SuccessMessage"] = "Cập nhật thông tin tổ chức thành công.";
                return RedirectToAction("Index", "Home");
            }

            var orgs = await _organizationApiClient.GetAllAsync();
            ViewBag.Organizations = orgs.OrderBy(o => o.Organization_name).ToList();
            ModelState.AddModelError("", "Cập nhật thất bại. Vui lòng thử lại.");
            return View(model);
        }
    }
}