using ErpOnlineOrder.Application.DTOs.CustomerProductDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
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
            var customerProducts = await _customerProductRepository.GetByCustomerIdAsync(customerId);
            return customerProducts.Select(MapToDto);
        }

        public async Task<IEnumerable<CustomerProductDto>> GetCustomersByProductIdAsync(int productId)
        {
            var customerProducts = await _customerProductRepository.GetByProductIdAsync(productId);
            return customerProducts.Select(MapToDto);
        }

        public async Task<CustomerProductDto?> GetByIdAsync(int id)
        {
            var customerProduct = await _customerProductRepository.GetByIdAsync(id);
            return customerProduct != null ? MapToDto(customerProduct) : null;
        }

        public async Task<CustomerProductDto?> AddProductToCustomerAsync(CreateCustomerProductDto dto, int createdBy)
        {
            // Ki?m tra xem ?ã t?n t?i ch?a
            if (await _customerProductRepository.ExistsAsync(dto.Customer_id, dto.Product_id))
            {
                return null;
            }

            var customerProduct = new Customer_product
            {
                Customer_id = dto.Customer_id,
                Product_id = dto.Product_id,
                Custom_price = dto.Custom_price,
                Discount_percent = dto.Discount_percent,
                Max_quantity = dto.Max_quantity,
                Is_active = dto.Is_active,
                Created_by = createdBy,
                Updated_by = createdBy
            };

            var created = await _customerProductRepository.AddAsync(customerProduct);
            
            // Load l?i ?? l?y thông tin ??y ??
            var result = await _customerProductRepository.GetByIdAsync(created.Id);
            return result != null ? MapToDto(result) : null;
        }

        public async Task<bool> AssignProductsToCustomerAsync(AssignProductsToCustomerDto dto, int createdBy)
        {
            var customerProducts = new List<Customer_product>();

            foreach (var productId in dto.Product_ids)
            {
                // B? qua n?u ?ã t?n t?i
                if (await _customerProductRepository.ExistsAsync(dto.Customer_id, productId))
                {
                    continue;
                }

                customerProducts.Add(new Customer_product
                {
                    Customer_id = dto.Customer_id,
                    Product_id = productId,
                    Discount_percent = dto.Default_discount_percent,
                    Is_active = true,
                    Created_by = createdBy,
                    Updated_by = createdBy
                });
            }

            if (customerProducts.Any())
            {
                await _customerProductRepository.AddRangeAsync(customerProducts);
            }

            return true;
        }

        public async Task<bool> UpdateCustomerProductAsync(UpdateCustomerProductDto dto, int updatedBy)
        {
            var customerProduct = await _customerProductRepository.GetByIdAsync(dto.Id);
            if (customerProduct == null)
            {
                return false;
            }

            customerProduct.Custom_price = dto.Custom_price;
            customerProduct.Discount_percent = dto.Discount_percent;
            customerProduct.Max_quantity = dto.Max_quantity;
            customerProduct.Is_active = dto.Is_active;
            customerProduct.Updated_by = updatedBy;

            await _customerProductRepository.UpdateAsync(customerProduct);
            return true;
        }

        public async Task<bool> RemoveProductFromCustomerAsync(int id, int deletedBy)
        {
            var customerProduct = await _customerProductRepository.GetByIdAsync(id);
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
            var customerProduct = await _customerProductRepository.GetByCustomerAndProductAsync(customerId, productId);
            return customerProduct != null && customerProduct.Is_active;
        }

        public async Task<IEnumerable<CustomerProductDto>> GetAllAsync()
        {
            var customerProducts = await _customerProductRepository.GetAllAsync();
            return customerProducts.Select(MapToDto);
        }

        private static CustomerProductDto MapToDto(Customer_product cp)
        {
            return new CustomerProductDto
            {
                Id = cp.Id,
                Customer_id = cp.Customer_id,
                Customer_name = cp.Customer?.Full_name ?? string.Empty,
                Product_id = cp.Product_id,
                Product_code = cp.Product?.Product_code ?? string.Empty,
                Product_name = cp.Product?.Product_name ?? string.Empty,
                Original_price = cp.Product?.Product_price ?? string.Empty,
                Custom_price = cp.Custom_price,
                Discount_percent = cp.Discount_percent,
                Max_quantity = cp.Max_quantity,
                Is_active = cp.Is_active,
                Created_at = cp.Created_at
            };
        }
    }
}
