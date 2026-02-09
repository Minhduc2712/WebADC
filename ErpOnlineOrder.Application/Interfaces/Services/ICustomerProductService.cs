using ErpOnlineOrder.Application.DTOs.CustomerProductDTOs;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface ICustomerProductService
    {
        Task<IEnumerable<CustomerProductDto>> GetProductsByCustomerIdAsync(int customerId);
        Task<IEnumerable<CustomerProductDto>> GetCustomersByProductIdAsync(int productId);
        Task<CustomerProductDto?> GetByIdAsync(int id);
        Task<CustomerProductDto?> AddProductToCustomerAsync(CreateCustomerProductDto dto, int createdBy);
        Task<bool> AssignProductsToCustomerAsync(AssignProductsToCustomerDto dto, int createdBy);
        Task<bool> UpdateCustomerProductAsync(UpdateCustomerProductDto dto, int updatedBy);
        Task<bool> RemoveProductFromCustomerAsync(int id, int deletedBy);
        Task<bool> CanCustomerOrderProductAsync(int customerId, int productId);
        Task<IEnumerable<CustomerProductDto>> GetAllAsync();
    }
}
