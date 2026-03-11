using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using ErpOnlineOrder.Application.DTOs.CustomerDTOs;
using ErpOnlineOrder.WebMVC.Attributes;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    [RequireAuth]
    public class CustomerController : Controller
    {
        private readonly HttpClient _httpClient;

        public CustomerController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ErpApi");
        }

        [HttpGet]
        public IActionResult UpdateCustomer() => View();

        [HttpPost]
        public async Task<IActionResult> UpdateCustomer(UpdateCustomerDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _httpClient.PutAsJsonAsync("customer/update-customer", model);
            if (response.IsSuccessStatusCode)
            {
                ViewBag.Message = "Cập nhật thông tin khách hàng thành công.";
                return RedirectToAction("Index", "Home");
            }
            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", error);
            return View(model);
        }

        [HttpGet]
        public IActionResult UpdateOrganization() => View();

        [HttpPost]
        public async Task<IActionResult> UpdateOrganization(UpdateOrganizationByCustomerDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _httpClient.PutAsJsonAsync("customer/update-organization", model);
            if (response.IsSuccessStatusCode)
            {
                ViewBag.Message = "Cập nhật thông tin tổ chức thành công.";
                return RedirectToAction("Index", "Home");
            }
            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", error);
            return View(model);
        }
    }
}