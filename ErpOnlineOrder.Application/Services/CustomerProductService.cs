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
        private readonly ICustomerPackageRepository _customerPackageRepository;
        private readonly IEmailQueue _emailQueue;

        public CustomerProductService(
            ICustomerProductRepository customerProductRepository,
            ICustomerPackageRepository customerPackageRepository,
            IEmailQueue emailQueue)
        {
            _customerProductRepository = customerProductRepository;
            _customerPackageRepository = customerPackageRepository;
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
            if (dto == null || dto.Customer_id <= 0 || dto.Product_id <= 0)
                return null;

            var existing = await _customerProductRepository.GetWithFiltersAsync(dto.Customer_id, dto.Product_id);

            if (existing != null)
            {
                bool wasDeleted = existing.Is_deleted;
                existing.Is_active = dto.Is_active;
                existing.Is_deleted = false;
                existing.Is_Excluded = false;
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
            if (dto == null || dto.Customer_id <= 0)
                return false;

            var requestedProductIds = dto.Product_ids?.Distinct().ToList();
            if (requestedProductIds == null || requestedProductIds.Count == 0 || requestedProductIds.Any(id => id <= 0))
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
                    if (existing.Is_deleted || existing.Is_Excluded)
                    {
                        existing.Is_deleted = false;
                        existing.Is_active = true;
                        existing.Is_Excluded = false;
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
            if (dto == null || dto.Id <= 0) return false;

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
            // 1. Nếu có bản ghi trực tiếp với Is_Excluded = true → từ chối
            var direct = await _customerProductRepository.GetWithFiltersAsync(customerId, productId);
            if (direct != null && !direct.Is_deleted && direct.Is_active && direct.Is_Excluded)
                return false;

            // 2. Sản phẩm có trong một gói đang gán cho khách hàng → cho phép
            var packageProductIds = await _customerPackageRepository.GetProductIdsByCustomerIdAsync(customerId);
            if (packageProductIds.Contains(productId))
                return true;

            // 3. Có bản ghi trực tiếp active không Excluded → cho phép (gán ngoài gói)
            if (direct != null && !direct.Is_deleted && direct.Is_active && !direct.Is_Excluded)
                return true;

            return false;
        }


        public async Task<IEnumerable<CustomerProductDto>> GetAllAsync()
        {
            var customerProducts = await _customerProductRepository.GetAllAsync();
            return customerProducts.Select(EntityMappers.ToCustomerProductDto);
        }

        public async Task<bool> ExcludeProductForCustomerAsync(int customerId, int productId, int updatedBy)
        {
            if (customerId <= 0 || productId <= 0) return false;

            // Kiểm tra sản phẩm có thuộc gói nào của khách hàng không
            var packageProductIds = await _customerPackageRepository.GetProductIdsByCustomerIdAsync(customerId);
            if (!packageProductIds.Contains(productId))
                return false;

            var existing = await _customerProductRepository.GetWithFiltersAsync(customerId, productId);
            if (existing != null)
            {
                existing.Is_Excluded = true;
                existing.Is_active = true;
                existing.Is_deleted = false;
                existing.Updated_by = updatedBy;
                await _customerProductRepository.UpdateAsync(existing);
                return true;
            }

            var entity = new Customer_product
            {
                Customer_id = customerId,
                Product_id = productId,
                Is_Excluded = true,
                Is_active = true,
                Is_deleted = false,
                Created_by = updatedBy,
                Updated_by = updatedBy
            };
            await _customerProductRepository.AddAsync(entity);
            return true;
        }

        public async Task<bool> UnexcludeProductForCustomerAsync(int customerId, int productId, int updatedBy)
        {
            if (customerId <= 0 || productId <= 0) return false;

            // Kiểm tra sản phẩm có thuộc gói của khách hàng không
            var packageProductIds = await _customerPackageRepository.GetProductIdsByCustomerIdAsync(customerId);
            if (!packageProductIds.Contains(productId))
                return false;

            var existing = await _customerProductRepository.GetWithFiltersAsync(customerId, productId);
            if (existing == null || !existing.Is_Excluded) return false;

            existing.Is_Excluded = false;
            existing.Updated_by = updatedBy;
            await _customerProductRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<IEnumerable<int>> GetExcludedProductIdsByCustomerAsync(int customerId)
        {
            return await _customerProductRepository.GetExcludedProductIdsByCustomerAsync(customerId);
        }
    }
}
