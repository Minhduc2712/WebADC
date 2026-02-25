using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.OrderDTOs;
using ClosedXML.Excel;
using ErpOnlineOrder.Application.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly ICustomerManagementRepository _customerManagementRepository;
        private readonly ICustomerProductRepository _customerProductRepository;
        private readonly ICustomerCategoryRepository _customerCategoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IEmailService _emailService;
        private readonly IPermissionService _permissionService;

        public OrderService(
            IOrderRepository orderRepository,
            IStaffRepository staffRepository,
            ICustomerManagementRepository customerManagementRepository,
            ICustomerProductRepository customerProductRepository,
            ICustomerCategoryRepository customerCategoryRepository,
            IProductRepository productRepository,
            IEmailService emailService,
            IPermissionService permissionService)
        {
            _orderRepository = orderRepository;
            _staffRepository = staffRepository;
            _customerManagementRepository = customerManagementRepository;
            _customerProductRepository = customerProductRepository;
            _customerCategoryRepository = customerCategoryRepository;
            _productRepository = productRepository;
            _emailService = emailService;
            _permissionService = permissionService;
        }

        public async Task<OrderDTO?> GetByIdAsync(int id, int? userId = null)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return null;

            // Cán bộ phụ trách chỉ xem được đơn hàng của khách hàng mình quản lý
            if (userId.HasValue && userId.Value > 0 && !await _permissionService.IsAdminAsync(userId.Value))
            {
                var staff = await _staffRepository.GetByUserIdAsync(userId.Value);
                if (staff != null)
                {
                    var customerIds = await _customerManagementRepository.GetCustomerIdsByStaffAsync(staff.Id);
                    if (!customerIds.Contains(order.Customer_id))
                        return null;
                }
            }

            var dto = MapToOrderDTO(order);
            if (userId.HasValue && userId.Value > 0)
                await RecordPermissionEnricher.EnrichOrderAsync(dto, userId.Value, _permissionService);
            return dto;
        }

        public async Task<IEnumerable<OrderDTO>> GetAllAsync(int? userId = null)
        {
            IEnumerable<Order> orders;
            if (userId.HasValue && userId.Value > 0 && !await _permissionService.IsAdminAsync(userId.Value))
            {
                var staff = await _staffRepository.GetByUserIdAsync(userId.Value);
                if (staff != null)
                {
                    var customerIds = await _customerManagementRepository.GetCustomerIdsByStaffAsync(staff.Id);
                    orders = await _orderRepository.GetByCustomerIdsAsync(customerIds);
                }
                else
                    orders = await _orderRepository.GetAllAsync();
            }
            else
                orders = await _orderRepository.GetAllAsync();

            var list = orders.Select(MapToOrderDTO).ToList();
            if (userId.HasValue && userId.Value > 0)
            {
                foreach (var dto in list)
                    await RecordPermissionEnricher.EnrichOrderAsync(dto, userId.Value, _permissionService);
            }
            return list;
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
                Created_by = dto.Created_by,
                Updated_by = dto.Created_by,
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
            _ = Task.Run(() => _emailService.SendOrderNotificationForStaffAndAdminAsync(order.Id));
            _ = Task.Run(() => _emailService.SendOrderNotificationForCustomerAsync(order.Id));
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
                Created_by = dto.Created_by,
                Updated_by = dto.Created_by,
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
            _ = Task.Run(() => _emailService.SendOrderNotificationForStaffAndAdminAsync(order.Id));
            _ = Task.Run(() => _emailService.SendOrderNotificationForCustomerAsync(order.Id));
            return result;
        }

        public async Task<UpdateOrderResultDto> UpdateOrderAsync(UpdateOrderDto dto)
        {
            var result = new UpdateOrderResultDto();

            var order = await _orderRepository.GetByIdAsync(dto.Id);
            if (order == null)
                 return new UpdateOrderResultDto { Success = false, Message = "Đơn hàng không tồn tại." };

            if (order.Order_status != "Pending")
                return new UpdateOrderResultDto { Success = false, Message = "Chỉ có thể sửa đơn hàng đang chờ xử lý." };

            order.Order_code = dto.Order_code;
            order.Order_date = dto.Order_date;
            order.Shipping_address = dto.Shipping_address;
            order.note = dto.note;
            order.Order_Details.Clear();
            foreach (var od in dto.Order_details)
            {
                order.Order_Details.Add(new Order_detail
                {
                    Order_id = order.Id,
                    Product_id = od.Product_id,
                    Quantity = od.Quantity,
                    Unit_price = od.Unit_price,
                    Total_price = od.Quantity * od.Unit_price,
                    Created_at = DateTime.UtcNow,
                    Updated_at = DateTime.UtcNow,
                    Created_by = dto.Updated_by,
                    Updated_by = dto.Updated_by,
                    Is_deleted = false
                });
            }
            order.Total_amount = order.Order_Details.Sum(od => od.Quantity);
            order.Total_price = order.Order_Details.Sum(od => od.Total_price);
            order.Updated_by = dto.Updated_by;
            order.Updated_at = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            result.Success = true;
            result.Message = "Cập nhật đơn hàng thành công";
            result.Order_id = order.Id;
            result.Order_code = order.Order_code;
            _ = Task.Run(() => _emailService.SendOrderNotificationForStaffAndAdminAsync(order.Id));
            _ = Task.Run(() => _emailService.SendOrderNotificationForCustomerAsync(order.Id));
            return result;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            await _orderRepository.DeleteAsync(id);
            return true;
        }

        public async Task<Order> CopyOrderAsync(CopyOrderDto dto)
        {
            var original = await _orderRepository.GetByIdAsync(dto.SourceOrderId);
            if (original == null) throw new Exception("Order not found");

            var orderDetails = original.Order_Details.Select(od => new Order_detail
            {
                Product_id = od.Product_id,
                Quantity = od.Quantity,
                Unit_price = od.Unit_price,
                Total_price = od.Total_price,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Is_deleted = false
            }).ToList();

            var copy = new Order
            {
                Order_code = $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}",
                Order_date = DateTime.UtcNow,
                Customer_id = original.Customer_id,
                Shipping_address = original.Shipping_address,
                note = original.note,
                Total_amount = orderDetails.Sum(od => od.Quantity),
                Total_price = orderDetails.Sum(od => od.Total_price),
                Order_status = "Pending",
                Created_by = dto.Created_by,
                Updated_by = dto.Created_by,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Is_deleted = false,
                Order_Details = orderDetails
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

        public async Task<bool> ConfirmOrderAsync(ConfirmOrderDto dto)
        {
            var order = await _orderRepository.GetByIdAsync(dto.OrderId);
            if (order == null) return false;

            // Cán bộ phụ trách chỉ xác nhận được đơn hàng của khách hàng mình quản lý
            if (dto.Updated_by > 0 && !await _permissionService.IsAdminAsync(dto.Updated_by))
            {
                var staff = await _staffRepository.GetByUserIdAsync(dto.Updated_by);
                if (staff != null)
                {
                    var customerIds = await _customerManagementRepository.GetCustomerIdsByStaffAsync(staff.Id);
                    if (!customerIds.Contains(order.Customer_id))
                        return false;
                }
            }

            order.Order_status = "Confirmed";
            order.Updated_by = dto.Updated_by;
            order.Updated_at = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);
            return true;
        }

        public async Task<bool> CancelOrderAsync(CancelOrderDto dto)
        {
            var order = await _orderRepository.GetByIdAsync(dto.OrderId);
            if (order == null) return false;

            // Cán bộ phụ trách chỉ hủy được đơn hàng của khách hàng mình quản lý
            if (dto.Updated_by > 0 && !await _permissionService.IsAdminAsync(dto.Updated_by))
            {
                var staff = await _staffRepository.GetByUserIdAsync(dto.Updated_by);
                if (staff != null)
                {
                    var customerIds = await _customerManagementRepository.GetCustomerIdsByStaffAsync(staff.Id);
                    if (!customerIds.Contains(order.Customer_id))
                        return false;
                }
            }

            order.Order_status = "Cancelled";
            order.Updated_by = dto.Updated_by;
            order.Updated_at = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);
            return true;
        }

        public async Task<byte[]> ExportOrdersToExcelAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Đơn hàng");

            ws.Cell(1, 1).Value = "Mã đơn";
            ws.Cell(1, 2).Value = "Ngày đặt";
            ws.Cell(1, 3).Value = "Khách hàng";
            ws.Cell(1, 4).Value = "Địa chỉ giao";
            ws.Cell(1, 5).Value = "Trạng thái";
            ws.Cell(1, 6).Value = "Tổng tiền";
            ws.Cell(1, 7).Value = "Ghi chú";
            ws.Range(1, 1, 1, 7).Style.Font.Bold = true;

            int row = 2;
            foreach (var o in orders)
            {
                var customerName = o.Customer != null
                    ? (!string.IsNullOrWhiteSpace(o.Customer.Full_name) ? o.Customer.Full_name : o.Customer.Customer_code ?? "")
                    : "";
                ExcelHelper.SetCellValue(ws.Cell(row, 1), o.Order_code);
                ExcelHelper.SetCellValue(ws.Cell(row, 2), o.Order_date.ToString("dd/MM/yyyy HH:mm"));
                ExcelHelper.SetCellValue(ws.Cell(row, 3), customerName);
                ExcelHelper.SetCellValue(ws.Cell(row, 4), o.Shipping_address ?? "");
                ExcelHelper.SetCellValue(ws.Cell(row, 5), o.Order_status ?? "");
                ws.Cell(row, 6).Value = o.Total_price;
                ExcelHelper.SetCellValue(ws.Cell(row, 7), o.note ?? "");
                row++;
            }
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream, false);
            return stream.ToArray();
        }

        public async Task<IEnumerable<OrderDTO>> GetOrdersByStatusAsync(string status, int? userId = null)
        {
            IEnumerable<Order> orders;
            if (userId.HasValue && userId.Value > 0 && !await _permissionService.IsAdminAsync(userId.Value))
            {
                var staff = await _staffRepository.GetByUserIdAsync(userId.Value);
                if (staff != null)
                {
                    var customerIds = await _customerManagementRepository.GetCustomerIdsByStaffAsync(staff.Id);
                    orders = await _orderRepository.GetByCustomerIdsAsync(customerIds);
                }
                else
                    orders = await _orderRepository.GetAllAsync();
            }
            else
                orders = await _orderRepository.GetAllAsync();

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
                    Product_id = od.Product_id,
                    Product_name = od.Product?.Product_name ?? "",
                    Quantity = od.Quantity,
                    Unit_price = od.Unit_price,
                    Total_price = od.Total_price
                }).ToList()
            };
        }
    }
}
