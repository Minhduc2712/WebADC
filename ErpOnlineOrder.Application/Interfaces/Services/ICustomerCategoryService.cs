using ErpOnlineOrder.Application.DTOs.CustomerCategoryDTOs;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface ICustomerCategoryService
    {
        Task<IEnumerable<CustomerCategoryDto>> GetCategoriesByCustomerIdAsync(int customerId);
        Task<IEnumerable<CustomerCategoryDto>> GetCustomersByCategoryIdAsync(int categoryId);
        Task<CustomerCategoryDto?> GetByIdAsync(int id);
        Task<CustomerCategoryDto?> AddCategoryToCustomerAsync(CreateCustomerCategoryDto dto, int createdBy);
        Task<bool> AssignCategoriesToCustomerAsync(AssignCategoriesToCustomerDto dto, int createdBy);
        Task<bool> UpdateCustomerCategoryAsync(UpdateCustomerCategoryDto dto, int updatedBy);
        Task<bool> RemoveCategoryFromCustomerAsync(int id, int deletedBy);
        Task<bool> CanCustomerOrderFromCategoryAsync(int customerId, int categoryId);
        Task<IEnumerable<CustomerCategoryDto>> GetAllAsync();
    }
}
