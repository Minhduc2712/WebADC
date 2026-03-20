using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.DistributorDTOs;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class DistributorApiClient : BaseApiClient, IDistributorApiClient
    {
        public DistributorApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<DistributorDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<DistributorDto>>("distributor", cancellationToken) ?? new List<DistributorDto>();
        }

        public async Task<DistributorDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<DistributorDto>($"distributor/{id}", cancellationToken);
        }

        public async Task<(DistributorDto? Data, string? Error)> CreateAsync(CreateDistributorDto dto, CancellationToken cancellationToken = default)
        {
            return await PostAsync<CreateDistributorDto, DistributorDto>("distributor", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateDistributorDto dto, CancellationToken cancellationToken = default)
        {
            return await PutAsync($"distributor/{id}", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync($"distributor/{id}", cancellationToken);
        }
    }
}
