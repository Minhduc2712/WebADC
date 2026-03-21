using System.Net.Http.Json;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class CustomerManagementApiClient : BaseApiClient, ICustomerManagementApiClient
    {
        public CustomerManagementApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<Customer_management>> GetAllAsync(int? staffId, int? provinceId, CancellationToken cancellationToken = default)
        {
            var query = new List<string>();
            if (staffId.HasValue) query.Add($"staffId={staffId.Value}");
            if (provinceId.HasValue) query.Add($"provinceId={provinceId.Value}");
            var path = query.Count > 0 ? "customermanagement?" + string.Join("&", query) : "customermanagement";
            return await GetAsync<IEnumerable<Customer_management>>(path, cancellationToken) ?? Array.Empty<Customer_management>();
        }

        public async Task<Customer_management?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<Customer_management>($"customermanagement/{id}", cancellationToken);
        }

        public async Task<IEnumerable<Customer_management>> GetByCustomerAsync(int customerId, CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<Customer_management>>($"customermanagement/customer/{customerId}", cancellationToken) ?? Array.Empty<Customer_management>();
        }

        public async Task<(Customer_management? Data, string? Error)> CreateAsync(Customer_management model, CancellationToken cancellationToken = default)
        {
            return await PostAsync<Customer_management, Customer_management>("customermanagement", model, cancellationToken);
        }

        public async Task<(Customer_management? Result, string? Error)> AssignStaffAsync(int staffId, int customerId, int provinceId, int? wardId = null, CancellationToken cancellationToken = default)
        {
            var body = new { Staff_id = staffId, Customer_id = customerId, Province_id = provinceId, Ward_id = wardId };
            return await PostAsync<object, Customer_management>("customermanagement/assign", body, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, Customer_management model, CancellationToken cancellationToken = default)
        {
            return await PutAsync($"customermanagement/{id}", model, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync($"customermanagement/{id}", cancellationToken);
        }

        public async Task<bool> IsAlreadyAssignedAsync(int staffId, int customerId, CancellationToken cancellationToken = default)
        {
            var obj = await GetAsync<CheckAssignedResponse>($"customermanagement/check?staffId={staffId}&customerId={customerId}", cancellationToken);
            return obj?.already_assigned ?? false;
        }

        private class CheckAssignedResponse
        {
            public bool already_assigned { get; set; }
        }
    }
}
