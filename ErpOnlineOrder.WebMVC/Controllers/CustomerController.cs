using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using ErpOnlineOrder.Application.DTOs.CustomerDTOs;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    public class CustomerController : Controller
    {
        private readonly HttpClient _httpClient;

        public CustomerController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:5001/api/");
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
                ViewBag.Message = "C?p nh?t thông tin khách hàng thành công.";
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
                ViewBag.Message = "C?p nh?t thông tin t? ch?c thành công.";
                return RedirectToAction("Index", "Home");
            }
            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", error);
            return View(model);
        }
    }
}