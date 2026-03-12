using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.CustomerDTOs;

namespace ErpOnlineOrder.WebMVC.Services.Interfaces
{
    public interface ICustomerApiClient
    {
        Task<IEnumerable<CustomerDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<CustomerSelectDto>> GetForSelectAsync(CancellationToken cancellationToken = default);
        Task<PagedResult<CustomerDTO>> GetPagedAsync(int page = 1, int pageSize = 20, string? searchTerm = null, int? regionId = null, CancellationToken cancellationToken = default);
        Task<CustomerDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<CustomerDTO?> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateCustomerByAdminDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
