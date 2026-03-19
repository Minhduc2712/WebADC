using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;
using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Domain.Models;
using ClosedXML.Excel;
using ErpOnlineOrder.Application.Helpers;
using ErpOnlineOrder.Domain.Constants;

namespace ErpOnlineOrder.Application.Services
{
    public class WarehouseExportService : IWarehouseExportService
    {
        private readonly IWarehouseExportRepository _exportRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IInvoiceService _invoiceService;
        private readonly IPermissionService _permissionService;
        private readonly IStaffRepository _staffRepository;
        private readonly ICustomerManagementRepository _customerManagementRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IStockRepository _stockRepository;

        public WarehouseExportService(
            IWarehouseExportRepository exportRepository,
            IInvoiceRepository invoiceRepository,
            IInvoiceService invoiceService,
            IPermissionService permissionService,
            IStaffRepository staffRepository,
            ICustomerManagementRepository customerManagementRepository,
            IOrderRepository orderRepository,
            IStockRepository stockRepository)
        {
            _exportRepository = exportRepository;
            _invoiceRepository = invoiceRepository;
            _invoiceService = invoiceService;
            _permissionService = permissionService;
            _staffRepository = staffRepository;
            _customerManagementRepository = customerManagementRepository;
            _orderRepository = orderRepository;
            _stockRepository = stockRepository;
        }

        private async Task<IEnumerable<int>?> GetAllowedCustomerIdsAsync(int userId)
        {
            if (await _permissionService.IsAdminAsync(userId)) return null;
            var staff = await _staffRepository.GetByUserIdAsync(userId);
            if (staff == null) return new List<int>();
            return await _customerManagementRepository.GetCustomerIdsByStaffAsync(staff.Id);
        }

        public async Task<WarehouseExportDto?> GetByIdAsync(int id, int? userId = null)
        {
            var export = await _exportRepository.GetByIdAsync(id);
            if (export == null) return null;

            if (userId.HasValue && userId.Value > 0)
            {
                var customerIds = await GetAllowedCustomerIdsAsync(userId.Value);
                if (customerIds != null && !customerIds.Contains(export.Customer_id))
                    return null;
            }

            var dto = EntityMappers.ToWarehouseExportDto(export);
            if (userId.HasValue && userId.Value > 0)
                await RecordPermissionEnricher.EnrichWarehouseExportAsync(dto, userId.Value, _permissionService);
            return dto;
        }

        public async Task<IEnumerable<WarehouseExportDto>> GetAllAsync(int? userId = null)
        {
            IEnumerable<Warehouse_export> exports;
            if (userId.HasValue && userId.Value > 0)
            {
                var customerIds = await GetAllowedCustomerIdsAsync(userId.Value);
                if (customerIds != null)
                {
                    var idList = customerIds.ToList();
                    if (idList.Count == 0) return new List<WarehouseExportDto>();
                    exports = (await _exportRepository.GetAllAsync()).Where(e => idList.Contains(e.Customer_id));
                }
                else
                    exports = await _exportRepository.GetAllAsync();
            }
            else
                exports = await _exportRepository.GetAllAsync();

            var list = exports.Select(EntityMappers.ToWarehouseExportDto).ToList();
            if (userId.HasValue && userId.Value > 0)
            {
                var userPermissions = (await _permissionService.GetUserPermissionsAsync(userId.Value)).ToHashSet();
                foreach (var dto in list)
                    RecordPermissionEnricher.EnrichWarehouseExport(dto, userPermissions);
            }
            return list;
        }

        public async Task<PagedResult<WarehouseExportDto>> GetAllPagedAsync(WarehouseExportFilterRequest request, int? userId = null)
        {
            IEnumerable<int>? customerIds = null;
            if (userId.HasValue && userId.Value > 0)
                customerIds = await GetAllowedCustomerIdsAsync(userId.Value);

            var paged = await _exportRepository.GetPagedWarehouseExportsAsync(request, customerIds);
            var dtos = paged.Items.Select(EntityMappers.ToWarehouseExportDto).ToList();
            if (userId.HasValue && userId.Value > 0)
            {
                var userPermissions = (await _permissionService.GetUserPermissionsAsync(userId.Value)).ToHashSet();
                foreach (var dto in dtos)
                    RecordPermissionEnricher.EnrichWarehouseExport(dto, userPermissions);
            }
            return new PagedResult<WarehouseExportDto>
            {
                Items = dtos,
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount
            };
        }

        public async Task<IEnumerable<WarehouseExportDto>> GetByInvoiceIdAsync(int invoiceId)
        {
            var exports = await _exportRepository.GetByInvoiceIdAsync(invoiceId);
            return exports.Select(EntityMappers.ToWarehouseExportDto);
        }

        public async Task<IEnumerable<WarehouseExportDto>> GetByCustomerIdAsync(int customerId)
        {
            var exports = await _exportRepository.GetByCustomerIdAsync(customerId);
            return exports.Select(EntityMappers.ToWarehouseExportDto);
        }

        public async Task<PagedResult<WarehouseExportDto>> GetByCustomerIdPagedAsync(int customerId, WarehouseExportFilterRequest request)
        {
            var paged = await _exportRepository.GetPagedWarehouseExportsAsync(request, new[] { customerId });
            var dtos = paged.Items.Select(EntityMappers.ToWarehouseExportDto).ToList();
            return new PagedResult<WarehouseExportDto>
            {
                Items = dtos,
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount
            };
        }

        public async Task<IEnumerable<WarehouseExportDto>> GetByWarehouseIdAsync(int warehouseId)
        {
            var exports = await _exportRepository.GetByWarehouseIdAsync(warehouseId);
            return exports.Select(EntityMappers.ToWarehouseExportDto);
        }

        public async Task<WarehouseExportDto?> CreateExportFromInvoiceAsync(CreateWarehouseExportDto dto, int userId)
        {
            // Kiểm tra hóa đơn tồn tại
            var invoice = await _invoiceRepository.GetByIdAsync(dto.Invoice_id);
            if (invoice == null)
            {
                throw new Exception("Hóa đơn không tồn tại");
            }

            // Kiểm tra hóa đơn đã có phiếu xuất kho chưa
            var existingExport = await HasExportForInvoiceAsync(dto.Invoice_id);
            if (existingExport)
            {
                throw new Exception("Hóa đơn này đã có phiếu xuất kho");
            }

            // Kiểm tra trạng thái hóa đơn
            if (invoice.Status != InvoiceStatuses.Confirmed && invoice.Status != InvoiceStatuses.Draft && invoice.Status != InvoiceStatuses.Split)
            {
                throw new Exception($"Không thể tạo phiếu xuất kho cho hóa đơn có trạng thái: {InvoiceStatuses.ToDisplayText(invoice.Status)}");
            }

            // Xác định Staff_id: ưu tiên từ dto, nếu không có thì lấy từ userId
            var staffId = dto.Staff_id;
            if (staffId <= 0)
            {
                var staff = await _staffRepository.GetByUserIdAsync(userId);
                staffId = staff?.Id ?? throw new Exception("Không tìm thấy nhân viên tương ứng với tài khoản đang đăng nhập");
            }

            var now = DateTime.UtcNow;

            var export = new Warehouse_export
            {
                Warehouse_export_code = $"EXP-{now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}",
                Warehouse_id = dto.Warehouse_id,
                Invoice_id = dto.Invoice_id,
                Order_id = invoice.Order_id,
                Customer_id = invoice.Customer_id,
                Staff_id = staffId,
                Export_date = dto.Export_date ?? now,
                Arrival_date = dto.Arrival_date,
                Delivery_status = "Pending",
                Status = "Draft",
                Created_by = userId,
                Updated_by = userId,
                Created_at = now,
                Updated_at = now,
                Is_deleted = false,
                Warehouse_Export_Details = new List<Warehouse_export_detail>()
            };

            // Nếu có chi tiết, sử dụng chi tiết được cung cấp
            if (dto.Details != null && dto.Details.Any())
            {
                foreach (var detail in dto.Details)
                {
                    export.Warehouse_Export_Details.Add(new Warehouse_export_detail
                    {
                        Warehouse_id = dto.Warehouse_id,
                        Product_id = detail.Product_id,
                        Quantity_shipped = detail.Quantity_shipped,
                        Unit_price = detail.Unit_price,
                        Total_price = detail.Quantity_shipped * detail.Unit_price,
                        Created_by = userId,
                        Updated_by = userId,
                        Created_at = now,
                        Updated_at = now,
                        Is_deleted = false
                    });
                }
            }
            else
            {
                // Tự động lấy từ hóa đơn
                foreach (var invoiceDetail in invoice.Invoice_Details.Where(d => !d.Is_deleted))
                {
                    export.Warehouse_Export_Details.Add(new Warehouse_export_detail
                    {
                        Warehouse_id = dto.Warehouse_id,
                        Product_id = invoiceDetail.Product_id,
                        Quantity_shipped = invoiceDetail.Quantity,
                        Unit_price = invoiceDetail.Unit_price,
                        Total_price = invoiceDetail.Total_price,
                        Created_by = userId,
                        Updated_by = userId,
                        Created_at = now,
                        Updated_at = now,
                        Is_deleted = false
                    });
                }
            }

            // Kiểm tra tồn kho theo kho + sản phẩm và trừ tồn khi tạo phiếu xuất
            var requestedByProduct = export.Warehouse_Export_Details
                .Where(d => !d.Is_deleted)
                .GroupBy(d => d.Product_id)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity_shipped) })
                .ToList();

            foreach (var item in requestedByProduct)
            {
                if (item.Quantity <= 0)
                {
                    throw new Exception($"Số lượng xuất không hợp lệ cho sản phẩm {item.ProductId}");
                }

                var stock = await _stockRepository.GetByWarehouseAndProductAsync(dto.Warehouse_id, item.ProductId);
                if (stock == null)
                {
                    throw new Exception($"Kho chưa được gán sản phẩm {item.ProductId} trong tồn kho");
                }

                if (stock.Quantity < item.Quantity)
                {
                    throw new Exception($"Tồn kho không đủ cho sản phẩm {item.ProductId}. Tồn hiện tại: {stock.Quantity}, yêu cầu xuất: {item.Quantity}");
                }
            }

            foreach (var item in requestedByProduct)
            {
                var stock = await _stockRepository.GetByWarehouseAndProductAsync(dto.Warehouse_id, item.ProductId);
                if (stock == null) continue;

                stock.Quantity -= item.Quantity;
                stock.Updated_by = userId;
                stock.Updated_at = now;
                await _stockRepository.UpdateAsync(stock);
            }

            await _exportRepository.AddAsync(export);

            var created = await _exportRepository.GetByIdAsync(export.Id);
            return created != null ? EntityMappers.ToWarehouseExportDto(created) : null;
        }

        public async Task<bool> UpdateExportAsync(int id, UpdateWarehouseExportDto dto, int userId)
        {
            var export = await _exportRepository.GetByIdAsync(id);
            if (export == null) return false;

            if (export.Status != ExportStatuses.Draft)
            {
                throw new InvalidOperationException("Chỉ được chỉnh sửa phiếu xuất kho ở trạng thái Nháp");
            }

            export.Warehouse_id = dto.Warehouse_id;
            export.Export_date = dto.Export_date;
            export.Arrival_date = dto.Arrival_date;
            export.Split_merge_note = string.IsNullOrWhiteSpace(dto.Split_merge_note)
                ? null
                : dto.Split_merge_note.Trim();
            export.Updated_by = userId;
            export.Updated_at = DateTime.UtcNow;

            await _exportRepository.UpdateAsync(export);
            return true;
        }

        private static readonly HashSet<string> ValidDeliveryStatuses = new(DeliveryStatuses.All);

        public async Task<bool> UpdateDeliveryStatusAsync(int id, string status, int userId)
        {
            if (!ValidDeliveryStatuses.Contains(status))
                throw new Exception($"Trạng thái giao hàng không hợp lệ: {status}. Chỉ chấp nhận: {string.Join(", ", ValidDeliveryStatuses)}");

            var export = await _exportRepository.GetByIdAsync(id);
            if (export == null) return false;

            if (!DeliveryStatuses.CanTransition(export.Delivery_status, status))
                throw new Exception($"Không thể chuyển trạng thái giao hàng từ '{DeliveryStatuses.ToDisplayText(export.Delivery_status)}' sang '{DeliveryStatuses.ToDisplayText(status)}'");

            // Sync: phải xác nhận phiếu trước khi giao hàng
            if (status == DeliveryStatuses.Shipped && export.Status == ExportStatuses.Draft)
                throw new Exception("Phải xác nhận phiếu xuất kho trước khi giao hàng");

            export.Delivery_status = status;

            // Sync: Delivered → export auto Completed
            var autoCompleted = false;
            if (status == DeliveryStatuses.Delivered && export.Status == ExportStatuses.Confirmed)
            {
                export.Status = ExportStatuses.Completed;
                autoCompleted = true;
            }

            export.Updated_by = userId;
            export.Updated_at = DateTime.UtcNow;

            await _exportRepository.UpdateAsync(export);

            // Sync: auto Completed → đơn hàng cũng hoàn thành
            if (autoCompleted && export.Order_id.HasValue)
            {
                var order = await _orderRepository.GetByIdForStatusCheckAsync(export.Order_id.Value);
                if (order != null && order.Order_status != "Completed" && order.Order_status != "Cancelled")
                {
                    order.Order_status = "Completed";
                    order.Updated_by = userId;
                    await _orderRepository.UpdateAsync(order);
                }
            }

            // Sync: auto Completed → hóa đơn cũng hoàn thành
            if (autoCompleted)
                await SyncStatusToInvoiceAsync(export, ExportStatuses.Completed, userId);

            return true;
        }

        public async Task<bool> ConfirmExportAsync(int id, int userId)
        {
            var export = await _exportRepository.GetByIdAsync(id);
            if (export == null) return false;

            if (export.Status != ExportStatuses.Draft)
            {
                throw new Exception("Chỉ có thể xác nhận phiếu xuất kho ở trạng thái Draft");
            }

            export.Status = ExportStatuses.Confirmed;
            export.Updated_by = userId;
            export.Updated_at = DateTime.UtcNow;

            await _exportRepository.UpdateAsync(export);

            // Sync: xác nhận phiếu xuất kho → xác nhận hóa đơn
            await SyncStatusToInvoiceAsync(export, ExportStatuses.Confirmed, userId);

            return true;
        }

        public async Task<bool> CancelExportAsync(int id, int userId)
        {
            var export = await _exportRepository.GetByIdAsync(id);
            if (export == null) return false;

            if (export.Delivery_status == DeliveryStatuses.Delivered)
            {
                throw new Exception("Không thể hủy phiếu xuất kho đã giao hàng");
            }

            export.Status = ExportStatuses.Cancelled;
            export.Updated_by = userId;
            export.Updated_at = DateTime.UtcNow;

            await _exportRepository.UpdateAsync(export);

            // Sync: hủy phiếu xuất kho → kiểm tra hủy hóa đơn
            await SyncStatusToInvoiceAsync(export, ExportStatuses.Cancelled, userId);

            return true;
        }

        public async Task<bool> UpdateStatusAsync(int id, string newStatus, int userId)
        {
            var export = await _exportRepository.GetByIdAsync(id);
            if (export == null) return false;

            if (!ExportStatuses.CanTransition(export.Status, newStatus))
                throw new Exception($"Không thể chuyển trạng thái phiếu xuất kho từ '{ExportStatuses.ToDisplayText(export.Status)}' sang '{ExportStatuses.ToDisplayText(newStatus)}'");

            export.Status = newStatus;

            // Sync: Completed → delivery auto Delivered
            if (newStatus == ExportStatuses.Completed && export.Delivery_status != DeliveryStatuses.Delivered)
                export.Delivery_status = DeliveryStatuses.Delivered;

            export.Updated_by = userId;
            export.Updated_at = DateTime.UtcNow;
            await _exportRepository.UpdateAsync(export);

            // Sync: Completed → đơn hàng cũng hoàn thành
            if (newStatus == ExportStatuses.Completed && export.Order_id.HasValue)
            {
                var order = await _orderRepository.GetByIdForStatusCheckAsync(export.Order_id.Value);
                if (order != null && order.Order_status != "Completed" && order.Order_status != "Cancelled")
                {
                    order.Order_status = "Completed";
                    order.Updated_by = userId;
                    await _orderRepository.UpdateAsync(order);
                }
            }

            // Sync: trạng thái phiếu xuất kho → hóa đơn
            await SyncStatusToInvoiceAsync(export, newStatus, userId);

            return true;
        }

        public async Task<bool> DeleteExportAsync(int id)
        {
            var export = await _exportRepository.GetByIdAsync(id);
            if (export == null) return false;

            if (export.Status == ExportStatuses.Confirmed || export.Status == ExportStatuses.Completed || export.Status == "Shipping")
                throw new Exception($"Không thể xóa phiếu xuất kho ở trạng thái: {export.Status}");

            if (export.Delivery_status == DeliveryStatuses.Delivered)
                throw new Exception("Không thể xóa phiếu xuất kho đã giao hàng");

            await _exportRepository.DeleteAsync(id);
            return true;
        }


        public async Task<SplitExportResultDto> SplitExportAsync(SplitWarehouseExportDto dto, int userId)
        {
            var result = new SplitExportResultDto();

            // 1. Lấy phiếu xuất kho gốc
            var sourceExport = await _exportRepository.GetByIdAsync(dto.Source_export_id);
            if (sourceExport == null)
            {
                result.Success = false;
                result.Message = "Phiếu xuất kho không tồn tại";
                return result;
            }

            // 2. Kiểm tra trạng thái (cho phép Split để tách nhiều tầng)
            if (sourceExport.Status == ExportStatuses.Merged
                || sourceExport.Status == ExportStatuses.Cancelled
                || sourceExport.Status == ExportStatuses.Completed)
            {
                result.Success = false;
                result.Message = "Không thể tách phiếu xuất kho đã gộp/hủy/hoàn thành";
                return result;
            }

            if (sourceExport.Delivery_status == DeliveryStatuses.Delivered)
            {
                result.Success = false;
                result.Message = "Không thể tách phiếu xuất kho đã giao hàng";
                return result;
            }

            var now = DateTime.UtcNow;
            var newExports = new List<Warehouse_export>();
            var newInvoiceIds = new List<int>();

            // 3. Tách hóa đơn trước (nếu cần)
            if (dto.Auto_split_invoice && sourceExport.Invoice != null)
            {
                var splitInvoiceDto = new SplitInvoiceDto
                {
                    Source_invoice_id = sourceExport.Invoice_id,
                    Note = dto.Note ?? $"Tách theo phiếu xuất kho {sourceExport.Warehouse_export_code}",
                    Split_parts = dto.Split_parts.Select(part => new SplitInvoicePart
                    {
                        Items = part.Items.Select(item =>
                        {
                            var exportDetail = sourceExport.Warehouse_Export_Details
                                .FirstOrDefault(d => d.Id == item.Export_detail_id);
                            
                            // Tìm invoice detail tương ứng
                            var invoiceDetail = sourceExport.Invoice?.Invoice_Details
                                .FirstOrDefault(d => d.Product_id == exportDetail?.Product_id);
                            
                            return new SplitInvoiceItem
                            {
                                Invoice_detail_id = invoiceDetail?.Id ?? 0,
                                Quantity = item.Quantity
                            };
                        }).Where(i => i.Invoice_detail_id > 0).ToList()
                    }).ToList()
                };

                var invoiceSplitResult = await _invoiceService.SplitInvoiceAsync(splitInvoiceDto, userId);
                if (invoiceSplitResult.Success)
                {
                    newInvoiceIds = invoiceSplitResult.New_invoices.Select(i => i.Id).ToList();
                }
            }

            // 4. Tạo các phiếu xuất kho mới từ các phần tách
            var codePrefix = $"{sourceExport.Warehouse_export_code}-S";
            var startIndex = await _exportRepository.GetMaxChildSuffixAsync(codePrefix);

            int partIndex = 0;
            foreach (var part in dto.Split_parts)
            {
                var newExport = new Warehouse_export
                {
                    Warehouse_export_code = $"{codePrefix}{startIndex + partIndex + 1}",
                    Warehouse_id = sourceExport.Warehouse_id,
                    Invoice_id = newInvoiceIds.Count > partIndex ? newInvoiceIds[partIndex] : sourceExport.Invoice_id,
                    Order_id = sourceExport.Order_id,
                    Customer_id = sourceExport.Customer_id,
                    Staff_id = sourceExport.Staff_id,
                    Export_date = now,
                    Delivery_status = DeliveryStatuses.Pending,
                    Status = ExportStatuses.Draft,
                    Parent_export_id = sourceExport.Id,
                    Split_merge_note = dto.Note ?? $"Tách từ phiếu {sourceExport.Warehouse_export_code}",
                    Created_by = userId,
                    Updated_by = userId,
                    Created_at = now,
                    Updated_at = now,
                    Is_deleted = false,
                    Warehouse_Export_Details = new List<Warehouse_export_detail>()
                };

                foreach (var item in part.Items)
                {
                    var sourceDetail = sourceExport.Warehouse_Export_Details
                        .FirstOrDefault(d => d.Id == item.Export_detail_id);

                    if (sourceDetail == null) continue;

                    var newDetail = new Warehouse_export_detail
                    {
                        Warehouse_id = sourceDetail.Warehouse_id,
                        Product_id = sourceDetail.Product_id,
                        Quantity_shipped = item.Quantity,
                        Unit_price = sourceDetail.Unit_price,
                        Total_price = item.Quantity * sourceDetail.Unit_price,
                        Created_by = userId,
                        Updated_by = userId,
                        Created_at = now,
                        Updated_at = now,
                        Is_deleted = false
                    };

                    newExport.Warehouse_Export_Details.Add(newDetail);

                    // Giảm số lượng trong phiếu gốc
                    sourceDetail.Quantity_shipped -= item.Quantity;
                    if (sourceDetail.Quantity_shipped <= 0)
                    {
                        sourceDetail.Is_deleted = true;
                    }
                    else
                    {
                        sourceDetail.Total_price = sourceDetail.Quantity_shipped * sourceDetail.Unit_price;
                    }
                }

                await _exportRepository.AddAsync(newExport);
                newExports.Add(newExport);
                partIndex++;
            }

            // 5. Cập nhật phiếu xuất kho gốc
            sourceExport.Status = ExportStatuses.Split;
            sourceExport.Split_merge_note = dto.Note ?? $"Đã tách thành {newExports.Count} phiếu";
            sourceExport.Updated_by = userId;
            sourceExport.Updated_at = now;

            await _exportRepository.UpdateAsync(sourceExport);

            result.Success = true;
            result.Message = $"Đã tách thành công thành {newExports.Count} phiếu xuất kho mới";
            result.Original_export = EntityMappers.ToWarehouseExportDto(sourceExport);
            result.New_exports = newExports.Select(e => EntityMappers.ToWarehouseExportDto(e)).ToList();
            result.New_invoice_ids = newInvoiceIds;

            return result;
        }

        public async Task<MergeExportResultDto> MergeExportsAsync(MergeWarehouseExportsDto dto, int userId)
        {
            var result = new MergeExportResultDto();

            if (dto.Export_ids.Count < 2)
            {
                result.Success = false;
                result.Message = "Cần ít nhất 2 phiếu xuất kho để gộp";
                return result;
            }

            // 1. Lấy tất cả phiếu xuất kho cần gộp
            var exports = new List<Warehouse_export>();
            int? customerId = null;

            foreach (var id in dto.Export_ids)
            {
                var export = await _exportRepository.GetByIdAsync(id);
                if (export == null)
                {
                    result.Success = false;
                    result.Message = $"Phiếu xuất kho {id} không tồn tại";
                    return result;
                }

                if (export.Status == ExportStatuses.Merged || export.Status == ExportStatuses.Cancelled)
                {
                    result.Success = false;
                    result.Message = $"Phiếu {export.Warehouse_export_code} không thể gộp (trạng thái: {export.Status})";
                    return result;
                }

                if (export.Delivery_status == DeliveryStatuses.Delivered)
                {
                    result.Success = false;
                    result.Message = $"Phiếu {export.Warehouse_export_code} đã giao hàng, không thể gộp";
                    return result;
                }

                if (customerId == null)
                {
                    customerId = export.Customer_id;
                }
                else if (export.Customer_id != customerId)
                {
                    result.Success = false;
                    result.Message = "Tất cả phiếu xuất kho phải cùng một khách hàng";
                    return result;
                }

                exports.Add(export);
            }

            var now = DateTime.UtcNow;
            int? mergedInvoiceId = dto.Merged_invoice_id;

            // 2. Gộp hóa đơn trước (nếu cần)
            if (dto.Auto_merge_invoices && !mergedInvoiceId.HasValue)
            {
                var invoiceIds = exports.Select(e => e.Invoice_id).Distinct().ToList();
                if (invoiceIds.Count > 1)
                {
                    var mergeInvoiceDto = new MergeInvoicesDto
                    {
                        Invoice_ids = invoiceIds,
                        Customer_id = customerId!.Value,
                        Staff_id = dto.Staff_id,
                        Note = dto.Note ?? "Gộp theo phiếu xuất kho"
                    };

                    var invoiceMergeResult = await _invoiceService.MergeInvoicesAsync(mergeInvoiceDto, userId);
                    if (invoiceMergeResult.Success && invoiceMergeResult.Merged_invoice != null)
                    {
                        mergedInvoiceId = invoiceMergeResult.Merged_invoice.Id;
                    }
                }
                else
                {
                    mergedInvoiceId = invoiceIds.First();
                }
            }

            // 3. Tạo phiếu xuất kho gộp mới
            var mergedExport = new Warehouse_export
            {
                Warehouse_export_code = $"EXP-M-{now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}",
                Warehouse_id = dto.Warehouse_id,
                Invoice_id = mergedInvoiceId ?? exports.First().Invoice_id,
                Order_id = null, // Phiếu gộp không liên kết trực tiếp với order
                Customer_id = customerId!.Value,
                Staff_id = dto.Staff_id,
                Export_date = now,
                Delivery_status = DeliveryStatuses.Pending,
                Status = ExportStatuses.Draft,
                Split_merge_note = dto.Note ?? $"Gộp từ {exports.Count} phiếu: {string.Join(", ", exports.Select(e => e.Warehouse_export_code))}",
                Created_by = userId,
                Updated_by = userId,
                Created_at = now,
                Updated_at = now,
                Is_deleted = false,
                Warehouse_Export_Details = new List<Warehouse_export_detail>()
            };

            // 4. Gộp tất cả chi tiết
            foreach (var export in exports)
            {
                foreach (var detail in export.Warehouse_Export_Details.Where(d => !d.Is_deleted))
                {
                    // Kiểm tra sản phẩm đã có trong phiếu gộp chưa
                    var existingDetail = mergedExport.Warehouse_Export_Details
                        .FirstOrDefault(d => d.Product_id == detail.Product_id && d.Unit_price == detail.Unit_price);

                    if (existingDetail != null)
                    {
                        // Cộng dồn số lượng
                        existingDetail.Quantity_shipped += detail.Quantity_shipped;
                        existingDetail.Total_price = existingDetail.Quantity_shipped * existingDetail.Unit_price;
                    }
                    else
                    {
                        // Thêm mới
                        mergedExport.Warehouse_Export_Details.Add(new Warehouse_export_detail
                        {
                            Warehouse_id = dto.Warehouse_id,
                            Product_id = detail.Product_id,
                            Quantity_shipped = detail.Quantity_shipped,
                            Unit_price = detail.Unit_price,
                            Total_price = detail.Total_price,
                            Created_by = userId,
                            Updated_by = userId,
                            Created_at = now,
                            Updated_at = now,
                            Is_deleted = false
                        });
                    }
                }
            }

            await _exportRepository.AddAsync(mergedExport);

            // 5. Cập nhật trạng thái các phiếu đã gộp
            foreach (var export in exports)
            {
                export.Status = ExportStatuses.Merged;
                export.Merged_into_export_id = mergedExport.Id;
                export.Split_merge_note = $"Đã gộp vào phiếu {mergedExport.Warehouse_export_code}";
                export.Updated_by = userId;
                export.Updated_at = now;
                await _exportRepository.UpdateAsync(export);
            }

            result.Success = true;
            result.Message = $"Đã gộp thành công {exports.Count} phiếu xuất kho";
            result.Merged_export = EntityMappers.ToWarehouseExportDto(mergedExport);
            result.Merged_export_ids = dto.Export_ids;
            result.Merged_invoice_id = mergedInvoiceId;

            return result;
        }

        public async Task<bool> UndoSplitAsync(int parentExportId, int userId)
        {
            var parentExport = await _exportRepository.GetByIdAsync(parentExportId);
            if (parentExport == null || parentExport.Status != ExportStatuses.Split)
            {
                return false;
            }

            // Lấy các phiếu con
            var childExports = await _exportRepository.GetChildExportsAsync(parentExportId);

            // Kiểm tra: không cho hoàn tác nếu bất kỳ phiếu con nào đã chuyển trạng thái hoặc đã giao hàng
            foreach (var child in childExports)
            {
                if (child.Status != ExportStatuses.Draft)
                {
                    throw new InvalidOperationException(
                        $"Không thể hoàn tác tách phiếu xuất kho vì phiếu con {child.Warehouse_export_code} đã có trạng thái: {ExportStatuses.ToDisplayText(child.Status)}");
                }

                if (child.Delivery_status == DeliveryStatuses.Delivered)
                {
                    throw new InvalidOperationException(
                        $"Không thể hoàn tác tách phiếu xuất kho vì phiếu con {child.Warehouse_export_code} đã giao hàng");
                }
            }

            var now = DateTime.UtcNow;

            // Khôi phục số lượng về phiếu gốc
            foreach (var childExport in childExports)
            {
                foreach (var childDetail in childExport.Warehouse_Export_Details.Where(d => !d.Is_deleted))
                {
                    var parentDetail = parentExport.Warehouse_Export_Details
                        .FirstOrDefault(d => d.Product_id == childDetail.Product_id);

                    if (parentDetail != null)
                    {
                        if (parentDetail.Is_deleted)
                        {
                            parentDetail.Is_deleted = false;
                            parentDetail.Quantity_shipped = childDetail.Quantity_shipped;
                        }
                        else
                        {
                            parentDetail.Quantity_shipped += childDetail.Quantity_shipped;
                        }
                        parentDetail.Total_price = parentDetail.Quantity_shipped * parentDetail.Unit_price;
                    }
                }

                // Xóa phiếu con
                childExport.Is_deleted = true;
                childExport.Updated_by = userId;
                childExport.Updated_at = now;
                await _exportRepository.UpdateAsync(childExport);
            }

            // Cập nhật phiếu gốc
            parentExport.Status = ExportStatuses.Draft;
            parentExport.Split_merge_note = null;
            parentExport.Updated_by = userId;
            parentExport.Updated_at = now;
            await _exportRepository.UpdateAsync(parentExport);

            return true;
        }

        public async Task<bool> UndoMergeAsync(int mergedExportId, int userId)
        {
            var mergedExport = await _exportRepository.GetByIdAsync(mergedExportId);
            if (mergedExport == null)
            {
                return false;
            }

            if (mergedExport.Delivery_status == DeliveryStatuses.Delivered)
            {
                throw new Exception("Không thể hoàn tác phiếu đã giao hàng");
            }

            var now = DateTime.UtcNow;

            // Lấy các phiếu đã gộp vào
            var sourceExports = (await _exportRepository.GetByMergedIntoExportIdAsync(mergedExportId)).ToList();

            // Khôi phục trạng thái các phiếu gốc
            foreach (var export in sourceExports)
            {
                export.Status = ExportStatuses.Draft;
                export.Merged_into_export_id = null;
                export.Split_merge_note = null;
                export.Updated_by = userId;
                export.Updated_at = now;
                await _exportRepository.UpdateAsync(export);
            }

            // Xóa phiếu gộp
            mergedExport.Is_deleted = true;
            mergedExport.Updated_by = userId;
            mergedExport.Updated_at = now;
            await _exportRepository.UpdateAsync(mergedExport);

            return true;
        }


        public async Task<bool> HasExportForInvoiceAsync(int invoiceId)
        {
            var exports = await _exportRepository.GetByInvoiceIdAsync(invoiceId);
            return exports.Any(e => !e.Is_deleted && e.Status != ExportStatuses.Cancelled);
        }

        private async Task SyncStatusToInvoiceAsync(Warehouse_export export, string exportStatus, int userId)
        {
            if (export.Invoice_id <= 0) return;

            var invoice = await _invoiceRepository.GetByIdAsync(export.Invoice_id);
            if (invoice == null || invoice.Status == InvoiceStatuses.Cancelled) return;

            var changed = false;

            if (exportStatus == ExportStatuses.Confirmed && invoice.Status == InvoiceStatuses.Draft)
            {
                invoice.Status = InvoiceStatuses.Confirmed;
                changed = true;
            }
            else if (exportStatus == ExportStatuses.Completed && invoice.Status != InvoiceStatuses.Completed)
            {
                invoice.Status = InvoiceStatuses.Completed;
                changed = true;
            }
            else if (exportStatus == ExportStatuses.Cancelled)
            {
                // Chỉ hủy hóa đơn nếu không còn phiếu xuất kho active nào khác
                var otherExports = await _exportRepository.GetByInvoiceIdAsync(export.Invoice_id);
                var hasOtherActive = otherExports.Any(e => !e.Is_deleted && e.Id != export.Id && e.Status != ExportStatuses.Cancelled);
                if (!hasOtherActive)
                {
                    invoice.Status = InvoiceStatuses.Cancelled;
                    changed = true;
                }
            }

            if (changed)
            {
                invoice.Updated_by = userId;
                invoice.Updated_at = DateTime.UtcNow;
                await _invoiceRepository.UpdateAsync(invoice);
            }
        }

        public async Task<IEnumerable<WarehouseExportDto>> GetChildExportsAsync(int parentExportId)
        {
            var childExports = await _exportRepository.GetChildExportsAsync(parentExportId);
            return childExports.Select(EntityMappers.ToWarehouseExportDto);
        }

        public async Task<byte[]> ExportWarehouseExportsToExcelAsync(string? status = null)
        {
            var exports = !string.IsNullOrEmpty(status)
                ? await _exportRepository.GetByStatusAsync(status)
                : await _exportRepository.GetAllAsync();
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Phiếu xuất kho");

            ws.Cell(1, 1).Value = "Mã phiếu";
            ws.Cell(1, 2).Value = "Ngày xuất";
            ws.Cell(1, 3).Value = "Ngày vận chuyển đến";
            ws.Cell(1, 4).Value = "Hóa đơn";
            ws.Cell(1, 5).Value = "Khách hàng";
            ws.Cell(1, 6).Value = "Kho";
            ws.Cell(1, 7).Value = "Tổng SL";
            ws.Cell(1, 8).Value = "Tổng tiền";
            ws.Cell(1, 9).Value = "Trạng thái";
            ws.Cell(1, 10).Value = "Giao hàng";
            ws.Range(1, 1, 1, 10).Style.Font.Bold = true;

            int row = 2;
            foreach (var exp in exports)
            {
                var dto = EntityMappers.ToWarehouseExportDto(exp);
                ExcelHelper.SetCellValue(ws.Cell(row, 1), dto.Warehouse_export_code);
                ExcelHelper.SetCellValue(ws.Cell(row, 2), dto.Export_date.ToString("dd/MM/yyyy"));
                ExcelHelper.SetCellValue(ws.Cell(row, 3), dto.Arrival_date?.ToString("dd/MM/yyyy") ?? "");
                ExcelHelper.SetCellValue(ws.Cell(row, 4), dto.Invoice_code ?? "");
                ExcelHelper.SetCellValue(ws.Cell(row, 5), dto.Customer_name ?? "");
                ExcelHelper.SetCellValue(ws.Cell(row, 6), dto.Warehouse_name ?? "");
                ws.Cell(row, 7).Value = dto.Total_quantity;
                ws.Cell(row, 8).Value = dto.Total_amount;
                ExcelHelper.SetCellValue(ws.Cell(row, 9), dto.Status ?? "");
                ExcelHelper.SetCellValue(ws.Cell(row, 10), dto.Delivery_status ?? "");
                row++;
            }
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream, false);
            return stream.ToArray();
        }
    }
}
