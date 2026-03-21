using ErpOnlineOrder.Application.DTOs.WardDTOs;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class WardApiClient : BaseApiClient, IWardApiClient
    {
        public WardApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<WardDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<WardDTO>>("ward", cancellationToken) ?? new List<WardDTO>();
        }

        public async Task<IEnumerable<WardDTO>> GetByProvinceIdAsync(int provinceId, CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<WardDTO>>($"ward/province/{provinceId}", cancellationToken) ?? new List<WardDTO>();
        }

        public async Task<WardDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<WardDTO>($"ward/{id}", cancellationToken);
        }

        public async Task<(WardDTO? Data, string? Error)> CreateAsync(CreateWardDto dto, CancellationToken cancellationToken = default)
        {
            return await PostAsync<CreateWardDto, WardDTO>("ward", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateWardDto dto, CancellationToken cancellationToken = default)
        {
            return await PutAsync($"ward/{id}", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync($"ward/{id}", cancellationToken);
        }
    }
}
