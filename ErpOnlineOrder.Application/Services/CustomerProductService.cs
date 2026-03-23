using DocumentFormat.OpenXml.VariantTypes;
using ErpOnlineOrder.Application.DTOs.CustomerProductDTOs;
using ErpOnlineOrder.Application.DTOs.EmailDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Services
{
    public class CustomerProductService : ICustomerProductService
    {
        private readonly ICustomerProductRepository _customerProductRepository;
        private readonly IEmailQueue _emailQueue;

        public CustomerProductService(
            ICustomerProductRepository customerProductRepository,
            IEmailQueue emailQueue)
        {
            _customerProductRepository = customerProductRepository;
            _emailQueue = emailQueue;
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
                bool wasDeleted = existing.Is_deleted;
                existing.Is_active = dto.Is_active;
                existing.Is_deleted = false; 
                existing.Max_quantity = dto.Max_quantity;
                existing.Updated_by = createdBy;

                await _customerProductRepository.UpdateAsync(existing);
                
                var updatedResult = await _customerProductRepository.GetByIdAsync(existing.Id);
                if (updatedResult != null && wasDeleted)
                    await _emailQueue.EnqueueAsync(new EmailMessage
                    {
                        ActionType = EmailActionType.ProductAssignedToCustomer,
                        PrimaryId = dto.Customer_id,
                        IdList = new List<int> { dto.Product_id }
                    });
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
            if (result != null)
                await _emailQueue.EnqueueAsync(new EmailMessage
                {
                    ActionType = EmailActionType.ProductAssignedToCustomer,
                    PrimaryId = dto.Customer_id,
                    IdList = new List<int> { dto.Product_id }
                });
            return result != null ? EntityMappers.ToCustomerProductDto(result) : null;
        }

        public async Task<bool> AssignProductsToCustomerAsync(AssignProductsToCustomerDto dto, int createdBy)
        {
            var requestedProductIds = dto.Product_ids.Distinct().ToList();
            if (requestedProductIds.Count == 0)
            {
                return false;
            }

            // Lấy tất cả bản ghi hiện có (cả đã xóa mềm) để tránh vi phạm ràng buộc unique
            var existingRecords = await _customerProductRepository.GetAllByProductIdsAsync(dto.Customer_id, requestedProductIds);
            var existingMap = existingRecords.ToDictionary(cp => cp.Product_id);

            var toRestore = new List<Customer_product>();
            var toInsertIds = new List<int>();

            foreach (var productId in requestedProductIds)
            {
                if (existingMap.TryGetValue(productId, out var existing))
                {
                    if (existing.Is_deleted)
                    {
                        existing.Is_deleted = false;
                        existing.Is_active = true;
                        existing.Updated_by = createdBy;
                        toRestore.Add(existing);
                    }
                    // Đã tồn tại và đang active → bỏ qua
                }
                else
                {
                    toInsertIds.Add(productId);
                }
            }

            var notifyProductIds = new List<int>();

            if (toRestore.Any())
            {
                await _customerProductRepository.UpdateRangeAsync(toRestore);
                notifyProductIds.AddRange(toRestore.Select(cp => cp.Product_id));
            }

            if (toInsertIds.Any())
            {
                var now = DateTime.Now;
                var newRecords = toInsertIds.Select(pid => new Customer_product
                {
                    Customer_id = dto.Customer_id,
                    Product_id = pid,
                    Is_active = true,
                    Created_by = createdBy,
                    Updated_by = createdBy,
                    Created_at = now,
                    Updated_at = now
                }).ToList();

                await _customerProductRepository.AddRangeAsync(newRecords);
                notifyProductIds.AddRange(toInsertIds);
            }

            if (notifyProductIds.Any())
                await _emailQueue.EnqueueAsync(new EmailMessage
                {
                    ActionType = EmailActionType.ProductAssignedToCustomer,
                    PrimaryId = dto.Customer_id,
                    IdList = notifyProductIds
                });

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
