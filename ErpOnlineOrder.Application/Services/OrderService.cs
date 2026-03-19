using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.OrderDTOs;
using ClosedXML.Excel;
using ErpOnlineOrder.Application.Helpers;
using ErpOnlineOrder.Domain.Constants;
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
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IWarehouseExportRepository _warehouseExportRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IEmailService _emailService;
        private readonly IPermissionService _permissionService;

        public OrderService(
            IOrderRepository orderRepository,
            IStaffRepository staffRepository,
            ICustomerManagementRepository customerManagementRepository,
            ICustomerProductRepository customerProductRepository,
            IProductRepository productRepository,
            IOrganizationRepository organizationRepository,
            IWarehouseRepository warehouseRepository,
            IWarehouseExportRepository warehouseExportRepository,
            IStockRepository stockRepository,
            IEmailService emailService,
            IPermissionService permissionService)
        {
            _orderRepository = orderRepository;
            _staffRepository = staffRepository;
            _customerManagementRepository = customerManagementRepository;
            _customerProductRepository = customerProductRepository;
            _productRepository = productRepository;
            _organizationRepository = organizationRepository;
            _warehouseRepository = warehouseRepository;
            _warehouseExportRepository = warehouseExportRepository;
            _stockRepository = stockRepository;
            _emailService = emailService;
            _permissionService = permissionService;
        }

        private async Task<string?> ResolveShippingAddressAsync(int customerId, string? requestedAddress)
        {
            var inputAddress = string.IsNullOrWhiteSpace(requestedAddress) ? null : requestedAddress.Trim();
            if (!string.IsNullOrWhiteSpace(inputAddress))
                return inputAddress;

            var org = await _organizationRepository.GetByCustomerIdAsync(customerId);
            var orgAddress = org?.Recipient_address;
            if (string.IsNullOrWhiteSpace(orgAddress))
                orgAddress = org?.Address;

            return string.IsNullOrWhiteSpace(orgAddress) ? null : orgAddress.Trim();
        }

        private async Task<bool> IsUserAllowedForCustomerAsync(int? userId, int customerId)
        {
            if (!userId.HasValue || userId <= 0) return true;
            if (await _permissionService.IsAdminAsync(userId.Value)) return true;

            var staff = await _staffRepository.GetByUserIdAsync(userId.Value);
            if (staff == null) return false;

            return await _customerManagementRepository.ExistsAsync(staff.Id, customerId);
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
            var shippingAddress = await ResolveShippingAddressAsync(customerId, dto.Shipping_address);
            if (string.IsNullOrWhiteSpace(shippingAddress))
                return new CreateOrderResultDto
                {
                    Success = false,
                    Message = "Vui lòng nhập địa chỉ giao hàng hoặc cập nhật địa chỉ trong thông tin tổ chức."
                };

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
                var finalPrice = info.Price;
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
                Shipping_address = shippingAddress,
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
            catch (Exception)
            {
                return new CreateOrderResultDto { Success = false, Message = "Lỗi hệ thống khi lưu đơn hàng." };
            }

            try { await _emailService.SendOrderNotificationForStaffAndAdminAsync(order.Id); } catch { }
            try { await _emailService.SendOrderNotificationForCustomerAsync(order.Id); } catch { }

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
            var shippingAddress = await ResolveShippingAddressAsync(customerId, dto.Shipping_address);
            if (string.IsNullOrWhiteSpace(shippingAddress))
            {
                return new CreateOrderResultDto
                {
                    Success = false,
                    Message = "Vui lòng nhập địa chỉ giao hàng hoặc cập nhật địa chỉ trong thông tin tổ chức."
                };
            }

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
                Shipping_address = shippingAddress,
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
            try { await _emailService.SendOrderNotificationForStaffAndAdminAsync(order.Id); } catch { }
            try { await _emailService.SendOrderNotificationForCustomerAsync(order.Id); } catch { }
            return result;
        }

        public async Task<UpdateOrderResultDto> UpdateOrderAsync(UpdateOrderDto dto)
        {
            var result = new UpdateOrderResultDto();

            var order = await _orderRepository.GetByIdForCopyAsync(dto.Id);
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
            try { await _emailService.SendOrderNotificationForStaffAndAdminAsync(order.Id); } catch { }
            try { await _emailService.SendOrderNotificationForCustomerAsync(order.Id); } catch { }
            return result;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            await _orderRepository.DeleteAsync(id);
            return true;
        }

        public async Task<Order> CopyOrderAsync(CopyOrderDto dto)
        {
            var original = await _orderRepository.GetByIdForCopyAsync(dto.SourceOrderId);
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
            var customerIds = (await _customerManagementRepository.GetCustomerIdsByStaffAsync(staffId)).ToList();
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

            return await _customerManagementRepository.ExistsAsync(staff.Id, orderCustomerId);
        }

        private async Task<int?> ResolveWarehouseForOrderAsync(Order order)
        {
            var warehouses = (await _warehouseRepository.GetAllAsync()).ToList();
            if (warehouses.Count == 0) return null;

            foreach (var warehouse in warehouses)
            {
                var enoughStock = true;
                foreach (var detail in order.Order_Details.Where(x => !x.Is_deleted))
                {
                    var stock = await _stockRepository.GetByWarehouseAndProductAsync(warehouse.Id, detail.Product_id);
                    if (stock == null || stock.Quantity < detail.Quantity)
                    {
                        enoughStock = false;
                        break;
                    }
                }

                if (enoughStock)
                {
                    return warehouse.Id;
                }
            }

            return null;
        }

        private async Task<Warehouse_export> CreateExportFromOrderAsync(Order order, int warehouseId, int userId)
        {
            var staff = await _staffRepository.GetByUserIdAsync(userId);
            if (staff == null)
            {
                throw new Exception("Không tìm thấy thông tin cán bộ để tạo phiếu xuất kho.");
            }

            var now = DateTime.UtcNow;
            var export = new Warehouse_export
            {
                Warehouse_export_code = $"EXP-{now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                Warehouse_id = warehouseId,
                Invoice_id = null,
                Order_id = order.Id,
                Customer_id = order.Customer_id,
                Staff_id = staff.Id,
                Export_date = now,
                Arrival_date = null,
                Delivery_status = DeliveryStatuses.Shipped,
                Status = ExportStatuses.Confirmed,
                Created_by = userId,
                Updated_by = userId,
                Created_at = now,
                Updated_at = now,
                Is_deleted = false,
                Warehouse_Export_Details = new List<Warehouse_export_detail>()
            };

            foreach (var od in order.Order_Details.Where(d => !d.Is_deleted))
            {
                var stock = await _stockRepository.GetByWarehouseAndProductAsync(warehouseId, od.Product_id);
                if (stock == null || stock.Quantity < od.Quantity)
                {
                    throw new Exception($"Tồn kho không đủ cho sản phẩm {od.Product_id}.");
                }

                stock.Quantity -= od.Quantity;
                stock.Updated_by = userId;
                stock.Updated_at = now;
                await _stockRepository.UpdateAsync(stock);

                export.Warehouse_Export_Details.Add(new Warehouse_export_detail
                {
                    Warehouse_id = warehouseId,
                    Product_id = od.Product_id,
                    Quantity_shipped = od.Quantity,
                    Unit_price = od.Unit_price,
                    Total_price = od.Total_price,
                    Created_by = userId,
                    Updated_by = userId,
                    Created_at = now,
                    Updated_at = now,
                    Is_deleted = false
                });
            }

            await _warehouseExportRepository.AddAsync(export);
            return export;
        }

        public async Task<ConfirmOrderResultDto> ConfirmOrderAsync(ConfirmOrderDto dto)
        {
            var order = await _orderRepository.GetByIdForApprovalAsync(dto.OrderId);

            if (order == null || order.Is_deleted)
            {
                return new ConfirmOrderResultDto
                {
                    Success = false,
                    Message = "Không tìm thấy đơn hàng."
                };
            }

            if (order.Order_status != "Pending")
            {
                return new ConfirmOrderResultDto
                {
                    Success = false,
                    Message = "Chỉ có thể duyệt đơn hàng ở trạng thái chờ xử lý."
                };
            }

            if (dto.Updated_by > 0 && !await _permissionService.IsAdminAsync(dto.Updated_by))
            {
                var staff = await _staffRepository.GetByUserIdAsync(dto.Updated_by);
                if (staff != null)
                {
                    var isManaged = await _customerManagementRepository.ExistsAsync(staff.Id, order.Customer_id);
                    if (!isManaged)
                    {
                        return new ConfirmOrderResultDto
                        {
                            Success = false,
                            Message = "Bạn không có quyền duyệt đơn hàng này."
                        };
                    }
                }
            }

            var selectedItems = (dto.Approved_items ?? new List<ConfirmOrderItemDto>())
                .Where(x => x.Is_selected)
                .ToList();

            if (selectedItems.Count == 0)
            {
                if ((dto.Approved_items ?? new List<ConfirmOrderItemDto>()).Count > 0)
                {
                    return new ConfirmOrderResultDto
                    {
                        Success = false,
                        Message = "Vui lòng chọn ít nhất một sản phẩm để duyệt."
                    };
                }

                selectedItems = order.Order_Details
                    .Select(x => new ConfirmOrderItemDto
                    {
                        Product_id = x.Product_id,
                        Quantity = x.Quantity,
                        Is_selected = true
                    })
                    .ToList();
            }

            var selectedByProduct = selectedItems
                .GroupBy(x => x.Product_id)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            if (selectedByProduct.Any(x => x.Value <= 0))
            {
                return new ConfirmOrderResultDto
                {
                    Success = false,
                    Message = "Số lượng duyệt phải lớn hơn 0."
                };
            }

            var orderDetailByProduct = order.Order_Details
                .GroupBy(x => x.Product_id)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var selected in selectedByProduct)
            {
                if (!orderDetailByProduct.TryGetValue(selected.Key, out var detail))
                {
                    return new ConfirmOrderResultDto
                    {
                        Success = false,
                        Message = "Sản phẩm được chọn không tồn tại trong đơn hàng."
                    };
                }

                if (selected.Value > detail.Quantity)
                {
                    return new ConfirmOrderResultDto
                    {
                        Success = false,
                        Message = "Số lượng duyệt không được vượt quá số lượng đặt."
                    };
                }
            }

            var now = DateTime.UtcNow;
            var remainingDetails = new List<Order_detail>();
            var currentDetails = order.Order_Details.ToList();

            foreach (var detail in currentDetails)
            {
                var approvedQuantity = selectedByProduct.TryGetValue(detail.Product_id, out var qty) ? qty : 0;
                var remainingQuantity = detail.Quantity - approvedQuantity;

                if (remainingQuantity > 0)
                {
                    remainingDetails.Add(new Order_detail
                    {
                        Product_id = detail.Product_id,
                        Quantity = remainingQuantity,
                        Unit_price = detail.Unit_price,
                        Total_price = remainingQuantity * detail.Unit_price,
                        Tax_rate = detail.Tax_rate,
                        Created_by = dto.Updated_by,
                        Updated_by = dto.Updated_by,
                        Created_at = now,
                        Updated_at = now,
                        Is_deleted = false
                    });
                }

                if (approvedQuantity <= 0)
                {
                    order.Order_Details.Remove(detail);
                    continue;
                }

                detail.Quantity = approvedQuantity;
                detail.Total_price = approvedQuantity * detail.Unit_price;
                detail.Updated_by = dto.Updated_by;
                detail.Updated_at = now;
            }

            order.Total_amount = order.Order_Details.Sum(od => od.Quantity);
            order.Total_price = order.Order_Details.Sum(od => od.Total_price);

            order.Order_status = "Exporting";
            order.Updated_by = dto.Updated_by;
            order.Updated_at = now;

            ConfirmOrderResultDto result;
            if (remainingDetails.Count > 0)
            {
                var childOrder = new Order
                {
                    Order_code = $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                    Order_date = now,
                    Customer_id = order.Customer_id,
                    Shipping_address = order.Shipping_address,
                    note = string.IsNullOrWhiteSpace(order.note)
                        ? $"Đơn hàng con tách từ {order.Order_code}"
                        : $"{order.note} | Đơn hàng con tách từ {order.Order_code}",
                    Total_amount = remainingDetails.Sum(x => x.Quantity),
                    Total_price = remainingDetails.Sum(x => x.Total_price),
                    Order_status = "Pending",
                    Created_by = dto.Updated_by,
                    Updated_by = dto.Updated_by,
                    Created_at = now,
                    Updated_at = now,
                    Is_deleted = false,
                    Order_Details = remainingDetails
                };

                await _orderRepository.AddAsync(childOrder);

                result = new ConfirmOrderResultDto
                {
                    Success = true,
                    Message = "Đã duyệt đơn và tách phần còn lại thành đơn hàng con.",
                    Is_split = true,
                    Child_order_id = childOrder.Id,
                    Child_order_code = childOrder.Order_code
                };
            }
            else
            {
                await _orderRepository.UpdateAsync(order);

                result = new ConfirmOrderResultDto
                {
                    Success = true,
                    Message = "Đã duyệt đơn hàng thành công.",
                    Is_split = false
                };
            }

            Warehouse_export? export = null;
            try
            {
                var warehouseId = await ResolveWarehouseForOrderAsync(order);
                if (!warehouseId.HasValue)
                {
                    throw new Exception("Không đủ tồn kho ở bất kỳ kho nào để tạo phiếu xuất kho.");
                }

                export = await CreateExportFromOrderAsync(order, warehouseId.Value, dto.Updated_by);
                result.Warehouse_export_id = export.Id;
                result.Warehouse_export_code = export.Warehouse_export_code;

                result.Message = $"{result.Message} Đã tạo phiếu xuất kho {export.Warehouse_export_code} và gửi cho bên vận chuyển.";

                try { await _emailService.SendWarehouseExportNotificationForStaffAndAdminAsync(export.Id); } catch { }
            }
            catch (Exception ex)
            {
                result.Message = $"{result.Message} Cảnh báo: chưa tạo được luồng xuất kho tự động ({ex.Message}).";
            }

            if (!string.Equals(dto.Notify_method, "download", StringComparison.OrdinalIgnoreCase))
            {
                try { await _emailService.SendOrderConfirmedNotificationForCustomerAsync(order.Id); } catch { }
            }

            return result;
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

                bool isManaged = await _customerManagementRepository.ExistsAsync(staff.Id, order.Customer_id);
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
                .Select(cp => new CustomerAllowedProductDto {
                    Product_id = cp.Product_id,
                    Product_code = cp.Product!.Product_code,
                    Product_name = cp.Product.Product_name,
                    Original_price = cp.Product.Product_price,
                    Max_quantity = cp.Max_quantity,
                    Category_name = cp.Product.Product_Categories?.FirstOrDefault()?.Category?.Category_name ?? ""
                });
        }

        public async Task<bool> CanCustomerOrderProductAsync(int customerId, int productId)
        {
            return await _customerProductRepository.ExistsAsync(customerId, productId);
        }

        public async Task<decimal> GetProductPriceForCustomerAsync(int customerId, int productId)
        {
            return await _productRepository.GetPriceByIdAsync(productId);
        }
    }
}
