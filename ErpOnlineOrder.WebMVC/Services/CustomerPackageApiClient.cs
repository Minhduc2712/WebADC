using ErpOnlineOrder.Application.DTOs.CustomerPackageDTOs;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class CustomerPackageApiClient : BaseApiClient, ICustomerPackageApiClient
    {
        public CustomerPackageApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<CustomerPackageDto>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<CustomerPackageDto>>($"customerpackage/customer/{customerId}", cancellationToken) ?? new List<CustomerPackageDto>();
        }

        public async Task<IEnumerable<CustomerPackageDto>> GetByPackageIdAsync(int packageId, CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<CustomerPackageDto>>($"customerpackage/package/{packageId}", cancellationToken) ?? new List<CustomerPackageDto>();
        }

        public async Task<CustomerPackageDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<CustomerPackageDto>($"customerpackage/{id}", cancellationToken);
        }

        public async Task<(CustomerPackageDto? Data, string? Error)> AssignPackageAsync(CreateCustomerPackageDto dto, CancellationToken cancellationToken = default)
        {
            return await PostAsync<CreateCustomerPackageDto, CustomerPackageDto>("customerpackage", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateCustomerPackageDto dto, CancellationToken cancellationToken = default)
        {
            return await PutAsync($"customerpackage/{id}", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UnassignPackageAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync($"customerpackage/{id}", cancellationToken);
        }
    }
}
