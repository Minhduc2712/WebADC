using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.CustomerProductDTOs;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class CustomerProductApiClient : BaseApiClient, ICustomerProductApiClient
    {
        public CustomerProductApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<CustomerProductDto>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<CustomerProductDto>>($"customerproduct/customer/{customerId}", cancellationToken) ?? new List<CustomerProductDto>();
        }

        public async Task<IEnumerable<CustomerProductDto>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<CustomerProductDto>>($"customerproduct/product/{productId}", cancellationToken) ?? new List<CustomerProductDto>();
        }

        public async Task<CustomerProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<CustomerProductDto>($"customerproduct/{id}", cancellationToken);
        }

        public async Task<(CustomerProductDto? Data, string? Error)> CreateAsync(CreateCustomerProductDto dto, CancellationToken cancellationToken = default)
        {
            return await PostAsync<CreateCustomerProductDto, CustomerProductDto>("customerproduct", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> AssignProductsAsync(AssignProductsToCustomerDto dto, CancellationToken cancellationToken = default)
        {
            return await PostWithoutReturnAsync("customerproduct/assign", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateCustomerProductDto dto, CancellationToken cancellationToken = default)
        {
            return await PutAsync($"customerproduct/{id}", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync($"customerproduct/{id}", cancellationToken);
        }
    }
}
