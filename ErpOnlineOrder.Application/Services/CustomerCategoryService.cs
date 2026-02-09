using ErpOnlineOrder.Application.DTOs.CustomerCategoryDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Services
{
    public class CustomerCategoryService : ICustomerCategoryService
    {
        private readonly ICustomerCategoryRepository _customerCategoryRepository;

        public CustomerCategoryService(ICustomerCategoryRepository customerCategoryRepository)
        {
            _customerCategoryRepository = customerCategoryRepository;
        }

        public async Task<IEnumerable<CustomerCategoryDto>> GetCategoriesByCustomerIdAsync(int customerId)
        {
            var customerCategories = await _customerCategoryRepository.GetByCustomerIdAsync(customerId);
            return customerCategories.Select(MapToDto);
        }

        public async Task<IEnumerable<CustomerCategoryDto>> GetCustomersByCategoryIdAsync(int categoryId)
        {
            var customerCategories = await _customerCategoryRepository.GetByCategoryIdAsync(categoryId);
            return customerCategories.Select(MapToDto);
        }

        public async Task<CustomerCategoryDto?> GetByIdAsync(int id)
        {
            var customerCategory = await _customerCategoryRepository.GetByIdAsync(id);
            return customerCategory != null ? MapToDto(customerCategory) : null;
        }

        public async Task<CustomerCategoryDto?> AddCategoryToCustomerAsync(CreateCustomerCategoryDto dto, int createdBy)
        {
            // Ki?m tra xem ?ã t?n t?i ch?a
            if (await _customerCategoryRepository.ExistsAsync(dto.Customer_id, dto.Category_id))
            {
                return null;
            }

            var customerCategory = new Customer_category
            {
                Customer_id = dto.Customer_id,
                Category_id = dto.Category_id,
                Discount_percent = dto.Discount_percent,
                Is_active = dto.Is_active,
                Created_by = createdBy,
                Updated_by = createdBy
            };

            var created = await _customerCategoryRepository.AddAsync(customerCategory);
            
            // Load l?i ?? l?y thông tin ??y ??
            var result = await _customerCategoryRepository.GetByIdAsync(created.Id);
            return result != null ? MapToDto(result) : null;
        }

        public async Task<bool> AssignCategoriesToCustomerAsync(AssignCategoriesToCustomerDto dto, int createdBy)
        {
            var customerCategories = new List<Customer_category>();

            foreach (var categoryId in dto.Category_ids)
            {
                // B? qua n?u ?ã t?n t?i
                if (await _customerCategoryRepository.ExistsAsync(dto.Customer_id, categoryId))
                {
                    continue;
                }

                customerCategories.Add(new Customer_category
                {
                    Customer_id = dto.Customer_id,
                    Category_id = categoryId,
                    Discount_percent = dto.Default_discount_percent,
                    Is_active = true,
                    Created_by = createdBy,
                    Updated_by = createdBy
                });
            }

            if (customerCategories.Any())
            {
                await _customerCategoryRepository.AddRangeAsync(customerCategories);
            }

            return true;
        }

        public async Task<bool> UpdateCustomerCategoryAsync(UpdateCustomerCategoryDto dto, int updatedBy)
        {
            var customerCategory = await _customerCategoryRepository.GetByIdAsync(dto.Id);
            if (customerCategory == null)
            {
                return false;
            }

            customerCategory.Discount_percent = dto.Discount_percent;
            customerCategory.Is_active = dto.Is_active;
            customerCategory.Updated_by = updatedBy;

            await _customerCategoryRepository.UpdateAsync(customerCategory);
            return true;
        }

        public async Task<bool> RemoveCategoryFromCustomerAsync(int id, int deletedBy)
        {
            var customerCategory = await _customerCategoryRepository.GetByIdAsync(id);
            if (customerCategory == null)
            {
                return false;
            }

            customerCategory.Updated_by = deletedBy;
            await _customerCategoryRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> CanCustomerOrderFromCategoryAsync(int customerId, int categoryId)
        {
            var customerCategory = await _customerCategoryRepository.GetByCustomerAndCategoryAsync(customerId, categoryId);
            return customerCategory != null && customerCategory.Is_active;
        }

        public async Task<IEnumerable<CustomerCategoryDto>> GetAllAsync()
        {
            var customerCategories = await _customerCategoryRepository.GetAllAsync();
            return customerCategories.Select(MapToDto);
        }

        private static CustomerCategoryDto MapToDto(Customer_category cc)
        {
            return new CustomerCategoryDto
            {
                Id = cc.Id,
                Customer_id = cc.Customer_id,
                Customer_name = cc.Customer?.Full_name ?? string.Empty,
                Category_id = cc.Category_id,
                Category_code = cc.Category?.Category_code ?? string.Empty,
                Category_name = cc.Category?.Category_name ?? string.Empty,
                Discount_percent = cc.Discount_percent,
                Is_active = cc.Is_active,
                Created_at = cc.Created_at
            };
        }
    }
}
