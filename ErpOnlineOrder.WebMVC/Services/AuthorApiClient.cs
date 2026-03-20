using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.AuthorDTOs;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class AuthorApiClient : BaseApiClient, IAuthorApiClient
    {
        public AuthorApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<AuthorDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<AuthorDto>>("author", cancellationToken) ?? Array.Empty<AuthorDto>();
        }

        public async Task<AuthorDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<AuthorDto>($"author/{id}", cancellationToken);
        }

        public async Task<(AuthorDto? Data, string? Error)> CreateAsync(CreateAuthorDto dto, CancellationToken cancellationToken = default)
        {
            return await PostAsync<CreateAuthorDto, AuthorDto>("author", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateAuthorDto dto, CancellationToken cancellationToken = default)
        {
            return await PutAsync($"author/{id}", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync($"author/{id}", cancellationToken);
        }
    }
}
