
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
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IWarehouseExportRepository _warehouseExportRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IEmailService _emailService;
        private readonly IPermissionService _permissionService;
        private readonly IDbTransactionManager _transactionManager;

        public OrderService(
            IOrderRepository orderRepository,
            IStaffRepository staffRepository,
            ICustomerManagementRepository customerManagementRepository,
            ICustomerProductRepository customerProductRepository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            IOrganizationRepository organizationRepository,
            IWarehouseRepository warehouseRepository,
            IWarehouseExportRepository warehouseExportRepository,
            IStockRepository stockRepository,
            IEmailService emailService,
            IPermissionService permissionService,
            IDbTransactionManager transactionManager)
        {
            _orderRepository = orderRepository;
            _staffRepository = staffRepository;
            _customerManagementRepository = customerManagementRepository;
            _customerProductRepository = customerProductRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _organizationRepository = organizationRepository;
            _warehouseRepository = warehouseRepository;
            _warehouseExportRepository = warehouseExportRepository;
            _stockRepository = stockRepository;
            _emailService = emailService;
            _permissionService = permissionService;
            _transactionManager = transactionManager;
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

            // Kiểm tra nếu người dùng đang đăng nhập chính là khách hàng chủ đơn
            var customer = await _customerRepository.GetByUserIdAsync(userId.Value);
            if (customer != null && customer.Id == customerId) return true;

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
            
            try { await _emailService.SendOrderUpdatedNotificationForCustomerAsync(order.Id); } catch { }
            
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

            var productIds = order.Order_Details.Where(x => !x.Is_deleted).Select(x => x.Product_id).ToList();

            foreach (var warehouse in warehouses)
            {
                var stocks = await _stockRepository.GetByWarehouseAndProductsAsync(warehouse.Id, productIds);
                var stockDict = stocks.ToDictionary(s => s.Product_id, s => s);
                var enoughStock = true;
                foreach (var detail in order.Order_Details.Where(x => !x.Is_deleted))
                {
                    if (!stockDict.TryGetValue(detail.Product_id, out var stock) || stock.Quantity < detail.Quantity)
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
            int staffId = 0;
            var staff = await _staffRepository.GetByUserIdAsync(userId);
            if (staff != null)
            {
                staffId = staff.Id;
            }
            else
            {
                // Nếu Khách hàng tự duyệt, lấy cán bộ đang phụ trách khách hàng này
                var mgmt = await _customerManagementRepository.GetByCustomerBasicAsync(order.Customer_id);
                var assignedStaff = mgmt?.FirstOrDefault()?.Staff_id;
                if (assignedStaff.HasValue)
                {
                    staffId = assignedStaff.Value;
                }
                else
                {
                    // Nếu chưa có ai phụ trách, lấy mặc định cán bộ đầu tiên
                    var anyStaff = await _staffRepository.GetAllAsync();
                    staffId = anyStaff.FirstOrDefault()?.Id ?? throw new Exception("Không có cán bộ nào trong hệ thống để gán cho phiếu xuất.");
                }
            }

            var now = DateTime.UtcNow;
            var export = new Warehouse_export
            {
                Warehouse_export_code = $"EXP-{now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                Warehouse_id = warehouseId,
                Order_id = order.Id,
                Customer_id = order.Customer_id,
                Staff_id = staffId,
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

            var productIds = order.Order_Details.Where(d => !d.Is_deleted).Select(d => d.Product_id).ToList();
            var stocks = await _stockRepository.GetByWarehouseAndProductsAsync(warehouseId, productIds);
            var stockDict = stocks.ToDictionary(s => s.Product_id, s => s);

            foreach (var od in order.Order_Details.Where(d => !d.Is_deleted))
            {
                if (!stockDict.TryGetValue(od.Product_id, out var stock) || stock.Quantity < od.Quantity)
                {
                    throw new Exception($"Tồn kho không đủ cho sản phẩm {od.Product_id}.");
                }

                stock.Quantity -= od.Quantity;
                stock.Updated_by = userId;
                stock.Updated_at = now;

                stock.Warehouse = null;
                stock.Product = null;

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
            var approvedDetails = new List<Order_detail>();
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

                if (approvedQuantity > 0)
                {
                    approvedDetails.Add(new Order_detail
                    {
                        Product_id = detail.Product_id,
                        Quantity = approvedQuantity,
                        Unit_price = detail.Unit_price,
                        Total_price = approvedQuantity * detail.Unit_price,
                        Tax_rate = detail.Tax_rate,
                        Created_by = dto.Updated_by,
                        Updated_by = dto.Updated_by,
                        Created_at = now,
                        Updated_at = now,
                        Is_deleted = false
                    });
                }
            }

            ConfirmOrderResultDto result = new ConfirmOrderResultDto { Success = true };

            bool shouldCreateExport = false;

            if (remainingDetails.Count > 0)
            {
                if (order.Is_auto_confirm) shouldCreateExport = true;
            }
            else
            {
                shouldCreateExport = true;
            }

            int? targetWarehouseId = null;
            if (shouldCreateExport)
            {
                var tempOrder = new Order { Order_Details = approvedDetails };
                targetWarehouseId = await ResolveWarehouseForOrderAsync(tempOrder);
                if (!targetWarehouseId.HasValue)
                    return new ConfirmOrderResultDto { Success = false, Message = "Thao tác thất bại: Không có kho nào đủ tồn kho để đáp ứng số lượng sản phẩm được duyệt. Vui lòng nhập thêm tồn kho trước khi duyệt." };
            }

            Order targetExportOrder = order;

            await using var transaction = await _transactionManager.BeginTransactionAsync();
            try
            {
            if (remainingDetails.Count > 0)
            {
                // Tách thành 2 đơn con
                var childRemainingOrder = new Order
                {
                    Order_code = $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                    Parent_order_id = order.Id,
                    Order_status = "Pending",
                    Order_Details = remainingDetails,
                    Order_date = now,
                    Customer_id = order.Customer_id,
                    Shipping_address = order.Shipping_address,
                    note = $"Đơn hàng con tách từ {order.Order_code}",
                    Total_amount = remainingDetails.Sum(x => x.Quantity),
                    Total_price = remainingDetails.Sum(x => x.Total_price),
                    Is_auto_confirm = order.Is_auto_confirm,
                    Created_by = dto.Updated_by,
                    Updated_by = dto.Updated_by,
                    Created_at = now,
                    Updated_at = now,
                    Is_deleted = false,
                };
                await _orderRepository.AddAsync(childRemainingOrder);

                var childApprovedOrder = new Order
                {
                    Order_code = $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                    Parent_order_id = order.Id,
                    Order_status = order.Is_auto_confirm ? "Exporting" : "WaitingCustomer",
                    Order_Details = approvedDetails,
                    Order_date = now,
                    Customer_id = order.Customer_id,
                    Shipping_address = order.Shipping_address,
                    note = $"Phần đã duyệt từ đơn {order.Order_code}",
                    Total_amount = approvedDetails.Sum(x => x.Quantity),
                    Total_price = approvedDetails.Sum(x => x.Total_price),
                    Is_auto_confirm = order.Is_auto_confirm,
                    Created_by = dto.Updated_by,
                    Updated_by = dto.Updated_by,
                    Created_at = now,
                    Updated_at = now,
                    Is_deleted = false,
                };
                await _orderRepository.AddAsync(childApprovedOrder);

                // Giữ nguyên đơn cha, đổi trạng thái thành Split
                order.Order_status = "Split";
                order.Updated_by = dto.Updated_by;
                order.Updated_at = now;
                await _orderRepository.UpdateAsync(order);

                result.Is_split = true;
                result.Child_order_id = childRemainingOrder.Id;
                result.Child_order_code = childRemainingOrder.Order_code;

                targetExportOrder = childApprovedOrder;

                if (order.Is_auto_confirm)
                {
                    result.Message = "Đã tách đơn và tự động đẩy phần có sẵn sang kho.";
                }
                else
                {
                    result.Message = "Đơn đã tách. Đang chờ khách hàng xác nhận phần có sẵn.";
                }
            }
            else
            {
                // Duyệt toàn bộ, không tách
                order.Order_status = "Exporting";
                order.Updated_by = dto.Updated_by;
                order.Updated_at = now;
                await _orderRepository.UpdateAsync(order);
                
                targetExportOrder = order;
                result.Message = "Đã duyệt toàn bộ đơn hàng.";
            }

                if (shouldCreateExport && targetWarehouseId.HasValue)
                {
                    var export = await CreateExportFromOrderAsync(targetExportOrder, targetWarehouseId.Value, dto.Updated_by);
                    result.Warehouse_export_id = export.Id;
                    result.Warehouse_export_code = export.Warehouse_export_code;
                    result.Message += $" Đã tạo phiếu xuất {export.Warehouse_export_code}.";
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ConfirmOrderResultDto { Success = false, Message = $"Lỗi hệ thống khi duyệt đơn: {ex.Message}" };
            }

            if (shouldCreateExport && result.Warehouse_export_id > 0)
            {
                try { await _emailService.SendWarehouseExportNotificationForStaffAndAdminAsync(result.Warehouse_export_id.Value); } catch { }
            }

            if (!string.Equals(dto.Notify_method, "download", StringComparison.OrdinalIgnoreCase))
            {
                try 
                { 
                    if (targetExportOrder.Order_status == "WaitingCustomer")
                    {
                        await _emailService.SendOrderWaitingCustomerNotificationAsync(targetExportOrder.Id);
                    }
                    else
                    {
                        await _emailService.SendOrderConfirmedNotificationForCustomerAsync(targetExportOrder.Id); 
                    }
                } 
                catch { }
            }

            return result;
        }

        public async Task<bool> CustomerApproveOrderAsync(int orderId, int userId)
        {
            var order = await _orderRepository.GetByIdForApprovalAsync(orderId);
            if (order == null || order.Is_deleted) return false;

            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null || order.Customer_id != customer.Id) return false;

            if (order.Order_status != "WaitingCustomer") return false;

            var warehouseId = await ResolveWarehouseForOrderAsync(order);
            if (!warehouseId.HasValue)
            {
                throw new InvalidOperationException("Xin lỗi, hiện tại không đủ tồn kho ở bất kỳ kho nào để đáp ứng đơn hàng này. Vui lòng liên hệ với nhân viên hỗ trợ.");
            }

            Warehouse_export? export = null;
            await using var transaction = await _transactionManager.BeginTransactionAsync();
            try
            {
            order.Order_status = "Exporting";
            order.Updated_by = userId;
            order.Updated_at = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

                export = await CreateExportFromOrderAsync(order, warehouseId.Value, userId);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException($"Không thể tạo phiếu xuất kho: {ex.Message}");
            }
            
            if (export != null)
            {
                try { await _emailService.SendWarehouseExportNotificationForStaffAndAdminAsync(export.Id); } catch { }
            }

            try { await _emailService.SendOrderConfirmedNotificationForCustomerAsync(order.Id); } catch { }

            return true;
        }

        public async Task<bool> CustomerRejectOrderAsync(int orderId, int userId)
        {
            var order = await _orderRepository.GetByIdForApprovalAsync(orderId);
            if (order == null || order.Is_deleted) return false;

            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null || order.Customer_id != customer.Id) return false;

            if (order.Order_status != "WaitingCustomer") return false;

            if (order.Parent_order_id.HasValue)
            {
                var parentOrder = await _orderRepository.GetByIdForStatusCheckAsync(order.Parent_order_id.Value);
                if (parentOrder != null && parentOrder.Order_status == "Split")
                {
                    // Hoàn tác tách: Xóa các đơn con và khôi phục đơn cha về Pending
                    var allCustomerOrders = await _orderRepository.GetByCustomerIdsAsync(new[] { order.Customer_id });
                    var childOrders = allCustomerOrders.Where(o => o.Parent_order_id == parentOrder.Id).ToList();

                    foreach (var child in childOrders)
                    {
                        await _orderRepository.DeleteAsync(child.Id);
                    }

                    parentOrder.Order_status = "Pending";
                    parentOrder.Updated_by = userId;
                    parentOrder.Updated_at = DateTime.UtcNow;
                    await _orderRepository.UpdateAsync(parentOrder);

                    try { await _emailService.SendOrderRejectedByCustomerNotificationAsync(parentOrder.Id); } catch { }

                    return true;
                }
            }

            // Fallback: Nếu không phải đơn tách, chuyển chính nó về Pending
            order.Order_status = "Pending";
            order.Updated_by = userId;
            order.Updated_at = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            try { await _emailService.SendOrderRejectedByCustomerNotificationAsync(order.Id); } catch { }

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
                if (staff != null)
                {
                    bool isManaged = await _customerManagementRepository.ExistsAsync(staff.Id, order.Customer_id);
                    if (!isManaged) return false;
                }
                else
                {
                    var orderWithCustomer = await _orderRepository.GetByIdAsync(dto.OrderId);
                    if (orderWithCustomer?.Customer?.User_id != dto.Updated_by)
                        return false;
                }
            }

            order.Order_status = "Cancelled";
            order.Updated_by = dto.Updated_by;
            order.Updated_at = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);
            
            try { await _emailService.SendOrderUpdatedNotificationForCustomerAsync(order.Id); } catch { }
            
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
