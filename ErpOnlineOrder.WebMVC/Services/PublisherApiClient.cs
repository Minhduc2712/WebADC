using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.PublisherDTOs;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class PublisherApiClient : BaseApiClient, IPublisherApiClient
    {
        public PublisherApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<PublisherDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<PublisherDto>>("publisher", cancellationToken) ?? Array.Empty<PublisherDto>();
        }

        public async Task<PublisherDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<PublisherDto>($"publisher/{id}", cancellationToken);
        }

        public async Task<(PublisherDto? Data, string? Error)> CreateAsync(CreatePublisherDto dto, CancellationToken cancellationToken = default)
        {
            return await PostAsync<CreatePublisherDto, PublisherDto>("publisher", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdatePublisherDto dto, CancellationToken cancellationToken = default)
        {
            return await PutAsync($"publisher/{id}", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync($"publisher/{id}", cancellationToken);
        }
    }
}
