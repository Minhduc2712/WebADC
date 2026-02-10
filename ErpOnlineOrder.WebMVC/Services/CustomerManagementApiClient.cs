using System.Net.Http.Json;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class CustomerManagementApiClient : ICustomerManagementApiClient
    {
        private readonly HttpClient _http;

        public CustomerManagementApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("ErpApi");
        }

        public async Task<IEnumerable<Customer_management>> GetAllAsync(int? staffId, int? provinceId, CancellationToken cancellationToken = default)
        {
            var query = new List<string>();
            if (staffId.HasValue) query.Add($"staffId={staffId.Value}");
            if (provinceId.HasValue) query.Add($"provinceId={provinceId.Value}");
            var path = query.Count > 0 ? "customermanagement?" + string.Join("&", query) : "customermanagement";
            var response = await _http.GetAsync(path, cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<Customer_management>();
            var list = await response.Content.ReadFromJsonAsync<List<Customer_management>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<Customer_management>();
        }

        public async Task<Customer_management?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"customermanagement/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<Customer_management>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<IEnumerable<Customer_management>> GetByCustomerAsync(int customerId, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"customermanagement/customer/{customerId}", cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<Customer_management>();
            var list = await response.Content.ReadFromJsonAsync<List<Customer_management>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<Customer_management>();
        }

        public async Task<Customer_management?> CreateAsync(Customer_management model, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("customermanagement", model, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<Customer_management>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<Customer_management?> AssignStaffAsync(int staffId, int customerId, int provinceId, CancellationToken cancellationToken = default)
        {
            var body = new { Staff_id = staffId, Customer_id = customerId, Province_id = provinceId };
            var response = await _http.PostAsJsonAsync("customermanagement/assign", body, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<Customer_management>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, Customer_management model, CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync($"customermanagement/{id}", model, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync($"customermanagement/{id}", cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<bool> IsAlreadyAssignedAsync(int staffId, int customerId, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"customermanagement/check?staffId={staffId}&customerId={customerId}", cancellationToken);
            if (!response.IsSuccessStatusCode) return false;
            var obj = await response.Content.ReadFromJsonAsync<CheckAssignedResponse>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return obj?.already_assigned ?? false;
        }

        private class CheckAssignedResponse
        {
            public bool already_assigned { get; set; }
        }
    }
}
