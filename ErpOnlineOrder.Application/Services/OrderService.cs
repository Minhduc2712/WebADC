using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.OrderDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerManagementRepository _customerManagementRepository;
        private readonly ICustomerProductRepository _customerProductRepository;
        private readonly ICustomerCategoryRepository _customerCategoryRepository;
        private readonly IProductRepository _productRepository;

        public OrderService(
            IOrderRepository orderRepository, 
            ICustomerManagementRepository customerManagementRepository,
            ICustomerProductRepository customerProductRepository,
            ICustomerCategoryRepository customerCategoryRepository,
            IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _customerManagementRepository = customerManagementRepository;
            _customerProductRepository = customerProductRepository;
            _customerCategoryRepository = customerCategoryRepository;
            _productRepository = productRepository;
        }

        public async Task<OrderDTO?> GetByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return null;
            return MapToOrderDTO(order);
        }

        public async Task<IEnumerable<OrderDTO>> GetAllAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(MapToOrderDTO);
        }

        public async Task<Order?> GetByCodeAsync(string code)
        {
            return await _orderRepository.GetByCodeAsync(code);
        }

        public async Task<CreateOrderResultDto> CreateOrderAsync(CreateOrderDto dto)
        {
            if (!dto.Customer_id.HasValue)
            {
                return new CreateOrderResultDto { Success = false, Message = "Vui lòng chọn khách hàng." };
            }
            int customerId = dto.Customer_id.Value;

            var result = new CreateOrderResultDto();
            var invalidProducts = new List<ProductValidationError>();

            // Kiểm tra từng sản phẩm trong đơn hàng
            foreach (var detail in dto.Order_details)
            {
                var canOrder = await CanCustomerOrderProductAsync(customerId, detail.Product_id);
                if (!canOrder)
                {
                    var product = await _productRepository.GetByIdAsync(detail.Product_id);
                    invalidProducts.Add(new ProductValidationError
                    {
                        Product_id = detail.Product_id,
                        Product_name = product?.Product_name ?? "Unknown",
                        Error_message = "Khách hàng không được phép đặt sản phẩm này"
                    });
                    continue;
                }

                // Kiểm tra số lượng tối đa
                var customerProduct = await _customerProductRepository.GetByCustomerAndProductAsync(customerId, detail.Product_id);
                if (customerProduct?.Max_quantity != null && detail.Quantity > customerProduct.Max_quantity)
                {
                    var product = await _productRepository.GetByIdAsync(detail.Product_id);
                    invalidProducts.Add(new ProductValidationError
                    {
                        Product_id = detail.Product_id,
                        Product_name = product?.Product_name ?? "Unknown",
                        Error_message = $"Số lượng đặt ({detail.Quantity}) vượt quá giới hạn tối đa ({customerProduct.Max_quantity})"
                    });
                }
            }

            // Nếu có sản phẩm không hợp lệ, trả về lỗi
            if (invalidProducts.Any())
            {
                result.Success = false;
                result.Message = "Một số sản phẩm không hợp lệ";
                result.Invalid_products = invalidProducts;
                return result;
            }

            // Tạo đơn hàng với giá đã được tính cho khách hàng
            var orderDetails = new List<Order_detail>();
            foreach (var detail in dto.Order_details)
            {
                var finalPrice = await GetProductPriceForCustomerAsync(customerId, detail.Product_id);
                orderDetails.Add(new Order_detail
                {
                    Product_id = detail.Product_id,
                    Quantity = detail.Quantity,
                    Unit_price = finalPrice,
                    Total_price = detail.Quantity * finalPrice,
                    Created_at = DateTime.UtcNow,
                    Updated_at = DateTime.UtcNow,
                    Is_deleted = false
                });
            }

            var order = new Order
            {
                Order_code = $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}",
                Order_date = dto.Order_date,
                Customer_id = customerId,
                Shipping_address = string.IsNullOrWhiteSpace(dto.Shipping_address) ? null : dto.Shipping_address.Trim(),
                note = string.IsNullOrWhiteSpace(dto.note) ? null : dto.note.Trim(),
                Total_amount = orderDetails.Sum(od => od.Quantity),
                Total_price = orderDetails.Sum(od => od.Total_price),
                Order_status = "Pending",
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Is_deleted = false,
                Order_Details = orderDetails
            };

            await _orderRepository.AddAsync(order);

            result.Success = true;
            result.Message = "Tạo đơn hàng thành công";
            result.Order_id = order.Id;
            result.Order_code = order.Order_code;
            return result;
        }

        public async Task<CreateOrderResultDto> CreateOrderWithoutValidationAsync(CreateOrderDto dto)
        {
            if (!dto.Customer_id.HasValue)
            {
                return new CreateOrderResultDto { Success = false, Message = "Vui lòng chọn khách hàng." };
            }
            int customerId = dto.Customer_id.Value;

            var result = new CreateOrderResultDto();

            var orderDetails = dto.Order_details.Select(od => new Order_detail
            {
                Product_id = od.Product_id,
                Quantity = od.Quantity,
                Unit_price = od.Unit_price,
                Total_price = od.Quantity * od.Unit_price,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Is_deleted = false
            }).ToList();

            var order = new Order
            {
                Order_code = $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}",
                Order_date = dto.Order_date,
                Customer_id = customerId,
                Shipping_address = string.IsNullOrWhiteSpace(dto.Shipping_address) ? null : dto.Shipping_address.Trim(),
                note = string.IsNullOrWhiteSpace(dto.note) ? null : dto.note.Trim(),
                Total_amount = orderDetails.Sum(od => od.Quantity),
                Total_price = orderDetails.Sum(od => od.Total_price),
                Order_status = "Pending",
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Is_deleted = false,
                Order_Details = orderDetails
            };

            await _orderRepository.AddAsync(order);

            result.Success = true;
            result.Message = "Tạo đơn hàng thành công";
            result.Order_id = order.Id;
            result.Order_code = order.Order_code;
            return result;
        }

        public async Task<bool> UpdateOrderAsync(UpdateOrderDto dto)
        {
            var order = await _orderRepository.GetByIdAsync(dto.Id);
            if (order == null) return false;

            order.Order_code = dto.Order_code;
            order.Order_date = dto.Order_date;
            order.Order_Details = dto.Order_details.Select(od => new Order_detail
            {
                Product_id = od.Product_id,
                Quantity = od.Quantity,
                Unit_price = od.Unit_price,
                Total_price = od.Quantity * od.Unit_price,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Is_deleted = false
            }).ToList();
            order.Total_price = order.Order_Details.Sum(od => od.Total_price);
            order.Updated_at = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);
            return true;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            await _orderRepository.DeleteAsync(id);
            return true;
        }

        public async Task<Order> CopyOrderAsync(int id)
        {
            var original = await _orderRepository.GetByIdAsync(id);
            if (original == null) throw new Exception("Order not found");

            var copy = new Order
            {
                Order_code = $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}",
                Order_date = DateTime.UtcNow,
                Customer_id = original.Customer_id,
                Total_price = original.Total_price,
                Order_status = "Pending",
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Is_deleted = false,
                Order_Details = original.Order_Details.Select(od => new Order_detail
                {
                    Product_id = od.Product_id,
                    Quantity = od.Quantity,
                    Unit_price = od.Unit_price,
                    Total_price = od.Total_price,
                    Created_at = DateTime.UtcNow,
                    Updated_at = DateTime.UtcNow,
                    Is_deleted = false
                }).ToList()
            };

            await _orderRepository.AddAsync(copy);
            return copy;
        }

        public async Task<IEnumerable<OrderDTO>> GetOrdersByStaffAsync(int staffId)
        {
            var managedCustomers = await _customerManagementRepository.GetByStaffAsync(staffId);
            var customerIds = managedCustomers.Select(cm => cm.Customer_id).Distinct();
            var orders = await _orderRepository.GetAllAsync();
            var filteredOrders = orders.Where(o => customerIds.Contains(o.Customer_id));
            return filteredOrders.Select(MapToOrderDTO);
        }

        public async Task<IEnumerable<OrderDTO>> GetOrdersByCustomerAsync(int customerId)
        {
            var orders = await _orderRepository.GetAllAsync();
            var customerOrders = orders.Where(o => o.Customer_id == customerId);
            return customerOrders.Select(MapToOrderDTO);
        }

        public async Task<bool> ConfirmOrderAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return false;
            order.Order_status = "Confirmed";
            order.Updated_at = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);
            return true;
        }

        public async Task<bool> CancelOrderAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return false;
            order.Order_status = "Cancelled";
            order.Updated_at = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);
            return true;
        }

        public async Task<byte[]> ExportOrdersToExcelAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            // TODO: Implement Excel export logic
            return Array.Empty<byte>();
        }

        public async Task<IEnumerable<OrderDTO>> GetOrdersByStatusAsync(string status)
        {
            var orders = await _orderRepository.GetAllAsync();
            var filtered = orders.Where(o => o.Order_status == status);
            return filtered.Select(MapToOrderDTO);
        }

        public async Task<bool> DeletePendingOrderAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null || order.Order_status != "Pending") return false;
            await _orderRepository.DeleteAsync(id);
            return true;
        }

        public async Task<IEnumerable<CustomerAllowedProductDto>> GetAllowedProductsForCustomerAsync(int customerId)
        {
            var allowedProducts = new List<CustomerAllowedProductDto>();

            // Lấy sản phẩm được gán trực tiếp cho khách hàng
            var customerProducts = await _customerProductRepository.GetByCustomerIdAsync(customerId);
            foreach (var cp in customerProducts.Where(x => x.Is_active))
            {
                if (cp.Product == null) continue;
                
                var originalPrice = decimal.TryParse(cp.Product.Product_price, out var price) ? price : 0;
                var finalPrice = CalculateFinalPrice(originalPrice, cp.Custom_price, cp.Discount_percent);
                
                allowedProducts.Add(new CustomerAllowedProductDto
                {
                    Product_id = cp.Product_id,
                    Product_code = cp.Product.Product_code,
                    Product_name = cp.Product.Product_name,
                    Original_price = originalPrice,
                    Custom_price = cp.Custom_price,
                    Discount_percent = cp.Discount_percent,
                    Final_price = finalPrice,
                    Max_quantity = cp.Max_quantity,
                    Category_name = cp.Product.Product_Categories?.FirstOrDefault()?.Category?.Category_name ?? ""
                });
            }

            // Lấy sản phẩm từ danh mục được gán cho khách hàng
            var customerCategories = await _customerCategoryRepository.GetByCustomerIdAsync(customerId);
            foreach (var cc in customerCategories.Where(x => x.Is_active))
            {
                if (cc.Category == null) continue;

                var productsInCategory = await _productRepository.GetByCategoryIdAsync(cc.Category_id);
                foreach (var product in productsInCategory)
                {
                    // Bỏ qua nếu sản phẩm đã có trong danh sách (ưu tiên cấu hình trực tiếp)
                    if (allowedProducts.Any(p => p.Product_id == product.Id)) continue;

                    var originalPrice = decimal.TryParse(product.Product_price, out var price) ? price : 0;
                    var finalPrice = CalculateFinalPrice(originalPrice, null, cc.Discount_percent);

                    allowedProducts.Add(new CustomerAllowedProductDto
                    {
                        Product_id = product.Id,
                        Product_code = product.Product_code,
                        Product_name = product.Product_name,
                        Original_price = originalPrice,
                        Custom_price = null,
                        Discount_percent = cc.Discount_percent,
                        Final_price = finalPrice,
                        Max_quantity = null,
                        Category_name = cc.Category.Category_name
                    });
                }
            }

            return allowedProducts;
        }

        public async Task<bool> CanCustomerOrderProductAsync(int customerId, int productId)
        {
            // Kiểm tra sản phẩm được gán trực tiếp
            var customerProduct = await _customerProductRepository.GetByCustomerAndProductAsync(customerId, productId);
            if (customerProduct != null && customerProduct.Is_active)
            {
                return true;
            }

            // Kiểm tra sản phẩm thuộc danh mục được gán
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return false;

            var productCategoryIds = product.Product_Categories?.Select(pc => pc.Category_id).ToList() ?? new List<int>();
            var customerCategories = await _customerCategoryRepository.GetByCustomerIdAsync(customerId);
            
            return customerCategories.Any(cc => cc.Is_active && productCategoryIds.Contains(cc.Category_id));
        }

        public async Task<decimal> GetProductPriceForCustomerAsync(int customerId, int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return 0;

            var originalPrice = decimal.TryParse(product.Product_price, out var price) ? price : 0;

            // Ưu tiên 1: Giá được cấu hình trực tiếp cho khách hàng
            var customerProduct = await _customerProductRepository.GetByCustomerAndProductAsync(customerId, productId);
            if (customerProduct != null && customerProduct.Is_active)
            {
                return CalculateFinalPrice(originalPrice, customerProduct.Custom_price, customerProduct.Discount_percent);
            }

            // Ưu tiên 2: Giảm giá theo danh mục
            var productCategoryIds = product.Product_Categories?.Select(pc => pc.Category_id).ToList() ?? new List<int>();
            var customerCategories = await _customerCategoryRepository.GetByCustomerIdAsync(customerId);
            var matchingCategory = customerCategories.FirstOrDefault(cc => cc.Is_active && productCategoryIds.Contains(cc.Category_id));
            
            if (matchingCategory != null)
            {
                return CalculateFinalPrice(originalPrice, null, matchingCategory.Discount_percent);
            }

            // Không có cấu hình riêng, trả về giá gốc
            return originalPrice;
        }

        private static decimal CalculateFinalPrice(decimal originalPrice, decimal? customPrice, decimal? discountPercent)
        {
            // Nếu có giá riêng, sử dụng giá riêng
            if (customPrice.HasValue)
            {
                return customPrice.Value;
            }

            // Nếu có giảm giá, áp dụng giảm giá
            if (discountPercent.HasValue && discountPercent.Value > 0)
            {
                return originalPrice * (1 - discountPercent.Value / 100);
            }

            return originalPrice;
        }

        private static OrderDTO MapToOrderDTO(Order order)
        {
            var customer = order.Customer;
            var customerName = customer != null
                ? (!string.IsNullOrWhiteSpace(customer.Full_name) ? customer.Full_name : customer.Customer_code ?? "—")
                : "—";

            return new OrderDTO
            {
                Id = order.Id,
                Order_code = order.Order_code,
                Order_date = order.Order_date,
                Total_price = order.Total_price,
                Order_status = order.Order_status ?? "",
                Customer_name = customerName,
                Shipping_address = order.Shipping_address,
                note = order.note,
                Order_details = order.Order_Details.Select(od => new OrderDetailDTO
                {
                    Product_name = od.Product?.Product_name ?? "",
                    Quantity = od.Quantity,
                    Unit_price = od.Unit_price,
                    Total_price = od.Total_price
                }).ToList()
            };
        }
    }
}