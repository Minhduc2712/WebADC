using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class CategoryApiClient : BaseApiClient, ICategoryApiClient
    {
        public CategoryApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<CategoryDto>>("category", cancellationToken) ?? Array.Empty<CategoryDto>();
        }

        public async Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<CategoryDto>($"category/{id}", cancellationToken);
        }

        public async Task<(CategoryDto? Data, string? Error)> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
        {
            return await PostAsync<CreateCategoryDto, CategoryDto>("category", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateCategoryDto dto, CancellationToken cancellationToken = default)
        {
            return await PutAsync($"category/{id}", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync($"category/{id}", cancellationToken);
        }
    }
}
