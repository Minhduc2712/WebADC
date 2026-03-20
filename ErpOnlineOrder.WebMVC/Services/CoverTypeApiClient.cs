using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.CoverTypeDTOs;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class CoverTypeApiClient : BaseApiClient, ICoverTypeApiClient
    {
        public CoverTypeApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<CoverTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<CoverTypeDto>>("covertype", cancellationToken) ?? Array.Empty<CoverTypeDto>();
        }

        public async Task<CoverTypeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<CoverTypeDto>($"covertype/{id}", cancellationToken);
        }

        public async Task<(CoverTypeDto? Data, string? Error)> CreateAsync(CreateCoverTypeDto dto, CancellationToken cancellationToken = default)
        {
            return await PostAsync<CreateCoverTypeDto, CoverTypeDto>("covertype", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateCoverTypeDto dto, CancellationToken cancellationToken = default)
        {
            return await PutAsync($"covertype/{id}", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync($"covertype/{id}", cancellationToken);
        }
    }
}
