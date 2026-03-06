using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Mappers;
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
        private readonly IProductRepository _productRepository;
        private readonly IEmailService _emailService;
        private readonly IPermissionService _permissionService;

        public OrderService(
            IOrderRepository orderRepository,
            IStaffRepository staffRepository,
            ICustomerManagementRepository customerManagementRepository,
            ICustomerProductRepository customerProductRepository,
            IProductRepository productRepository,
            IEmailService emailService,
            IPermissionService permissionService)
        {
            _orderRepository = orderRepository;
            _staffRepository = staffRepository;
            _customerManagementRepository = customerManagementRepository;
            _customerProductRepository = customerProductRepository;
            _productRepository = productRepository;
            _emailService = emailService;
            _permissionService = permissionService;
        }

        private async Task<bool> IsUserAllowedForCustomerAsync(int? userId, int customerId)
        {
            if (!userId.HasValue || userId <= 0) return true;
            if (await _permissionService.IsAdminAsync(userId.Value)) return true;

            var staff = await _staffRepository.GetByUserIdAsync(userId.Value);
            if (staff == null) return false;

            return await _customerManagementRepository.IsStaffManagingCustomerAsync(staff.Id, customerId);
        }

        public async Task<OrderDTO?> GetByIdAsync(int id, int? userId = null)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return null;

            // Kiểm tra quyền xem
            if (!await IsUserAllowedForCustomerAsync(userId, order.Customer_id)) return null;

            var dto = EntityMappers.ToOrderDto(order);
            if (userId.HasValue && userId.Value > 0)
                await RecordPermissionEnricher.EnrichOrderAsync(dto, userId.Value, _permissionService);
                
            return dto;
        }

        public async Task<IEnumerable<OrderDTO>> GetAllAsync(int? userId = null)
        {
            IEnumerable<OrderDTO> orders;
            
            if (userId.HasValue && userId > 0 && !await _permissionService.IsAdminAsync(userId.Value))
            {
                var staff = await _staffRepository.GetByUserIdAsync(userId.Value);
                var customerIds = staff != null 
                    ? await _customerManagementRepository.GetCustomerIdsByStaffAsync(staff.Id) 
                    : new List<int>();
                
                orders = await _orderRepository.GetByCustomerIdsDTOAsync(customerIds);
            }
            else
            {
                orders = await _orderRepository.GetAllAsync();
            }

            var list = orders.ToList();
            if (userId.HasValue && userId > 0)
            {
                var permissions = (await _permissionService.GetUserPermissionsAsync(userId.Value)).ToHashSet();
                foreach (var dto in list)
                    RecordPermissionEnricher.EnrichOrder(dto, permissions);
            }
            return list;
        }

        public async Task<PagedResult<OrderDTO>> GetAllPagedAsync(OrderFilterRequest request, int? userId = null)
        {
            IEnumerable<int>? customerIds = null;
            if (userId.HasValue && userId.Value > 0 && !await _permissionService.IsAdminAsync(userId.Value))
            {
                var staff = await _staffRepository.GetByUserIdAsync(userId.Value);
                if (staff != null)
                {
                    customerIds = await _customerManagementRepository.GetCustomerIdsByStaffAsync(staff.Id);
                }
            }

            var paged = await _orderRepository.GetPagedOrdersDTOAsync(request, customerIds);
            var dtos = paged.Items.ToList();
            if (userId.HasValue && userId.Value > 0)
            {
                var permissions = (await _permissionService.GetUserPermissionsAsync(userId.Value)).ToHashSet();
                foreach (var dto in dtos)
                    RecordPermissionEnricher.EnrichOrder(dto, permissions);
            }
            return new PagedResult<OrderDTO>
            {
                Items = dtos,
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount
            };
        }

        public async Task<Order?> GetByCodeAsync(string code)
        {
            return await _orderRepository.GetByCodeAsync(code);
        }

        public async Task<CreateOrderResultDto> CreateOrderAsync(CreateOrderDto dto)
        {
            if (!dto.Customer_id.HasValue)
                return new CreateOrderResultDto { Success = false, Message = "Vui lòng chọn khách hàng." };

            int customerId = dto.Customer_id.Value;
            var productIds = dto.Order_details.Select(od => od.Product_id).Distinct().ToList();

            var productDataMap = await _productRepository.GetProductValidationMapAsync(customerId, productIds);

            var invalidProducts = new List<ProductValidationError>();
            foreach (var detail in dto.Order_details)
            {
                if (!productDataMap.TryGetValue(detail.Product_id, out var info))
                {
                    invalidProducts.Add(new ProductValidationError {
                        Product_id = detail.Product_id, Product_name = "Không xác định", Error_message = "Sản phẩm không tồn tại"
                    });
                    continue;
                }

                if (!info.HasSetting)
                {
                    invalidProducts.Add(new ProductValidationError {
                        Product_id = detail.Product_id, Product_name = info.Product_name, Error_message = "Khách hàng không được phép đặt sản phẩm này"
                    });
                    continue;
                }

                if (info.Max_quantity != null && detail.Quantity > info.Max_quantity)
                {
                    invalidProducts.Add(new ProductValidationError {
                        Product_id = detail.Product_id, Product_name = info.Product_name,
                        Error_message = $"Số lượng ({detail.Quantity}) vượt quá giới hạn ({info.Max_quantity})"
                    });
                }
            }

            if (invalidProducts.Any())
            {
                return new CreateOrderResultDto { Success = false, Message = "Một số sản phẩm không hợp lệ", Invalid_products = invalidProducts };
            }

            var orderDetails = dto.Order_details.Select(detail => {
                var info = productDataMap[detail.Product_id];
                var finalPrice = info.Price ?? 0;
                return new Order_detail {
                    Product_id = detail.Product_id,
                    Quantity = detail.Quantity,
                    Unit_price = finalPrice,
                    Total_price = detail.Quantity * finalPrice,
                    Created_at = DateTime.UtcNow,
                    Updated_at = DateTime.UtcNow,
                    Is_deleted = false
                };
            }).ToList();

            var order = new Order {
                Order_code = $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                Order_date = dto.Order_date,
                Customer_id = customerId,
                Shipping_address = dto.Shipping_address?.Trim(),
                note = dto.note?.Trim(),
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

            try 
            {
                await _orderRepository.AddAsync(order);
            }
            catch (Exception ex)
            {
                return new CreateOrderResultDto { Success = false, Message = "Lỗi hệ thống khi lưu đơn hàng." };
            }

            _ = Task.Run(() => _emailService.SendOrderNotificationForStaffAndAdminAsync(order.Id));
            _ = Task.Run(() => _emailService.SendOrderNotificationForCustomerAsync(order.Id));

            return new CreateOrderResultDto {
                Success = true,
                Message = "Tạo đơn hàng thành công",
                Order_id = order.Id,
                Order_code = order.Order_code
            };
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
            var customerIds = managedCustomers.Select(cm => cm.Customer_id).Distinct().ToList();
            if (customerIds.Count == 0) return new List<OrderDTO>();
            return await _orderRepository.GetByCustomerIdsDTOAsync(customerIds);
        }

        public async Task<IEnumerable<OrderDTO>> GetOrdersByCustomerAsync(int customerId)
        {
            return await _orderRepository.GetByCustomerIdsDTOAsync(new[] { customerId });
        }

        public async Task<PagedResult<OrderDTO>> GetOrdersByCustomerPagedAsync(int customerId, OrderFilterRequest request)
        {
            var paged = await _orderRepository.GetPagedOrdersDTOAsync(request, new[] { customerId });
            var dtos = paged.Items.ToList();
            return new PagedResult<OrderDTO>
            {
                Items = dtos,
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount
            };
        }

        private async Task<bool> HasPermissionOnOrderAsync(int orderCustomerId, int userId)
        {
            if (userId <= 0 || await _permissionService.IsAdminAsync(userId)) return true;

            var staff = await _staffRepository.GetByUserIdAsync(userId);
            if (staff == null) return false;

            return await _customerManagementRepository.IsStaffManagingCustomerAsync(staff.Id, orderCustomerId);
        }

        public async Task<bool> ConfirmOrderAsync(ConfirmOrderDto dto)
        {
            var order = await _orderRepository.GetByIdForStatusCheckAsync(dto.OrderId); 
    
            if (order == null) return false;

            if (dto.Updated_by > 0 && !await _permissionService.IsAdminAsync(dto.Updated_by))
            {
                var staff = await _staffRepository.GetByUserIdAsync(dto.Updated_by);
                if (staff != null)
                {
                    var isManaged = await _customerManagementRepository.IsStaffManagingCustomerAsync(staff.Id, order.Customer_id);
                    if (!isManaged) return false;
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
            var order = await _orderRepository.GetByIdForStatusCheckAsync(dto.OrderId);
            if (order == null || order.Is_deleted) return false;

            if (order.Order_status == "Cancelled" || order.Order_status == "Delivered") return true;

            if (dto.Updated_by > 0 && !await _permissionService.IsAdminAsync(dto.Updated_by))
            {
                var staff = await _staffRepository.GetByUserIdAsync(dto.Updated_by);
                if (staff == null) return false;

                bool isManaged = await _customerManagementRepository.IsStaffManagingCustomerAsync(staff.Id, order.Customer_id);
                if (!isManaged) return false;
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
                ExcelHelper.SetCellValue(ws.Cell(row, 1), o.Order_code);
                ExcelHelper.SetCellValue(ws.Cell(row, 2), o.Order_date.ToString("dd/MM/yyyy HH:mm"));
                ExcelHelper.SetCellValue(ws.Cell(row, 3), o.Customer_name ?? "");
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
            IEnumerable<OrderDTO> orders;
            if (userId.HasValue && userId.Value > 0 && !await _permissionService.IsAdminAsync(userId.Value))
            {
                var staff = await _staffRepository.GetByUserIdAsync(userId.Value);
                if (staff != null)
                {
                    var customerIds = await _customerManagementRepository.GetCustomerIdsByStaffAsync(staff.Id);
                    orders = await _orderRepository.GetByCustomerIdsDTOAsync(customerIds);
                }
                else
                    orders = await _orderRepository.GetAllAsync();
            }
            else
                orders = await _orderRepository.GetAllAsync();

            var filtered = orders.Where(o => o.Order_status == status);
            return filtered.ToList();
        }

        public async Task<bool> DeletePendingOrderAsync(int id)
        {
            var order = await _orderRepository.GetByIdForStatusCheckAsync(id);
            if (order == null || order.Order_status != "Pending") return false;
            await _orderRepository.DeleteAsync(id);
            return true;
        }

        public async Task<IEnumerable<CustomerAllowedProductDto>> GetAllowedProductsForCustomerAsync(int customerId)
        {
            var customerProducts = await _customerProductRepository.GetByCustomerIdWithDetailsAsync(customerId);
            
            return customerProducts
                .Where(cp => cp.Is_active && cp.Product != null)
                .Select(cp => {
                    var originalPrice = cp.Product!.Product_price;
                    return new CustomerAllowedProductDto {
                        Product_id = cp.Product_id,
                        Product_code = cp.Product.Product_code,
                        Product_name = cp.Product.Product_name,
                        Original_price = originalPrice,
                        Custom_price = cp.Custom_price,
                        Discount_percent = cp.Discount_percent,
                        Final_price = CalculateFinalPrice(originalPrice, cp.Custom_price, cp.Discount_percent),
                        Max_quantity = cp.Max_quantity,
                        Category_name = cp.Product.Product_Categories?.FirstOrDefault()?.Category?.Category_name ?? ""
                    };
                });
        }

        public async Task<bool> CanCustomerOrderProductAsync(int customerId, int productId)
        {
            return await _customerProductRepository.ExistsAsync(customerId, productId);
        }

        private static decimal CalculateFinalPrice(decimal originalPrice, decimal? customPrice, decimal? discountPercent)
        {
            if (customPrice.HasValue && customPrice.Value != 0)
                return customPrice.Value;
            if (discountPercent.HasValue && discountPercent.Value > 0)
                return originalPrice * (1 - discountPercent.Value / 100);
            return originalPrice;
        }
    }
}
