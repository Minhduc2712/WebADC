using ErpOnlineOrder.Application.DTOs.PackageDTOs;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class PackageApiClient : BaseApiClient, IPackageApiClient
    {
        public PackageApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<PackageDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<PackageDto>>("package", cancellationToken) ?? new List<PackageDto>();
        }

        public async Task<PackageDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<PackageDto>($"package/{id}", cancellationToken);
        }

        public async Task<(PackageDto? Data, string? Error)> CreateAsync(CreatePackageDto dto, CancellationToken cancellationToken = default)
        {
            return await PostAsync<CreatePackageDto, PackageDto>("package", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdatePackageDto dto, CancellationToken cancellationToken = default)
        {
            return await PutAsync($"package/{id}", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync($"package/{id}", cancellationToken);
        }

        public async Task<(bool Success, string? Error)> AddProductAsync(int packageId, CreatePackageProductDto dto, CancellationToken cancellationToken = default)
        {
            return await PostWithoutReturnAsync($"package/{packageId}/products", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> RemoveProductAsync(int packageId, int productId, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync($"package/{packageId}/products/{productId}", cancellationToken);
        }
    }
}
