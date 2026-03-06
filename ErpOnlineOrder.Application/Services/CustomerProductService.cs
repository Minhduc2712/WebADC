using DocumentFormat.OpenXml.VariantTypes;
using ErpOnlineOrder.Application.DTOs.CustomerProductDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Services
{
    public class CustomerProductService : ICustomerProductService
    {
        private readonly ICustomerProductRepository _customerProductRepository;

        public CustomerProductService(ICustomerProductRepository customerProductRepository)
        {
            _customerProductRepository = customerProductRepository;
        }

        public async Task<IEnumerable<CustomerProductDto>> GetProductsByCustomerIdAsync(int customerId)
        {
            return await _customerProductRepository.GetDtosByCustomerIdAsync(customerId);
        }

        public async Task<IEnumerable<CustomerProductDto>> GetCustomersByProductIdAsync(int productId)
        {
            var customerProducts = await _customerProductRepository.GetByProductIdAsync(productId);
            return customerProducts.Select(EntityMappers.ToCustomerProductDto);
        }
        public async Task<CustomerProductDto?> GetByIdAsync(int id)
        {
            var customerProduct = await _customerProductRepository.GetByIdAsync(id);
            return customerProduct != null ? EntityMappers.ToCustomerProductDto(customerProduct) : null;
        }

        public async Task<CustomerProductDto?> AddProductToCustomerAsync(CreateCustomerProductDto dto, int createdBy)
        {
            var existing = await _customerProductRepository.GetWithFiltersAsync(dto.Customer_id, dto.Product_id);

            if (existing != null)
            {
                existing.Is_active = dto.Is_active;
                existing.Is_deleted = false; 
                existing.Max_quantity = dto.Max_quantity;
                existing.Updated_by = createdBy;

                await _customerProductRepository.UpdateAsync(existing);
                
                var updatedResult = await _customerProductRepository.GetByIdAsync(existing.Id);
                return updatedResult != null ? EntityMappers.ToCustomerProductDto(updatedResult) : null;
            }

            var customerProduct = new Customer_product
            {
                Customer_id = dto.Customer_id,
                Product_id = dto.Product_id,
                Max_quantity = dto.Max_quantity,
                Is_active = dto.Is_active,
                Is_deleted = false,
                Created_by = createdBy,
                Updated_by = createdBy
            };

            var created = await _customerProductRepository.AddAsync(customerProduct);
            
            var result = await _customerProductRepository.GetByIdAsync(created.Id);
            return result != null ? EntityMappers.ToCustomerProductDto(result) : null;
        }

        public async Task<bool> AssignProductsToCustomerAsync(AssignProductsToCustomerDto dto, int createdBy)
        {
            var requestedProductIds = dto.Product_ids.Distinct().ToList();
            if (requestedProductIds.Count == 0)
            {
                return false;
            }

            var existingProductIds = await _customerProductRepository.GetExistingProductIdsAsync(dto.Customer_id, requestedProductIds);

            var newProductIds = requestedProductIds.Except(existingProductIds).ToList();
            
            if (newProductIds.Any())
            {
                var now = DateTime.Now;
                var customerProducts = newProductIds.Select(productId => new Customer_product
                {
                    Customer_id = dto.Customer_id,
                    Product_id = productId,
                    Is_active = true,
                    Created_by = createdBy,
                    Updated_by = createdBy,
                    Created_at = now, // Gán trực tiếp ở đây hoặc để AddRangeAsync xử lý
                    Updated_at = now
                }).ToList();

                await _customerProductRepository.AddRangeAsync(customerProducts);
            }

            return true;
        }

        public async Task<bool> UpdateCustomerProductAsync(UpdateCustomerProductDto dto, int updatedBy)
        {
            var customerProduct = await _customerProductRepository.GetByIdBasicAsync(dto.Id);
            if (customerProduct == null)
            {
                return false;
            }

            customerProduct.Max_quantity = dto.Max_quantity;
            customerProduct.Is_active = dto.Is_active;
            customerProduct.Updated_by = updatedBy;

            await _customerProductRepository.UpdateAsync(customerProduct);
            return true;
        }

        public async Task<bool> RemoveProductFromCustomerAsync(int id, int deletedBy)
        {
            var customerProduct = await _customerProductRepository.GetByIdBasicAsync(id);
            if (customerProduct == null)
            {
                return false;
            }

            customerProduct.Updated_by = deletedBy;
            await _customerProductRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> CanCustomerOrderProductAsync(int customerId, int productId)
        {
            return await _customerProductRepository.ExistsAsync(customerId, productId, onlyActive: true);
        }


        public async Task<IEnumerable<CustomerProductDto>> GetAllAsync()
        {
            var customerProducts = await _customerProductRepository.GetAllAsync();
            return customerProducts.Select(EntityMappers.ToCustomerProductDto);
        }

    }
}
