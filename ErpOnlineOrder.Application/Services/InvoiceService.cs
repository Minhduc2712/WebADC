using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using ClosedXML.Excel;
using ErpOnlineOrder.Application.Helpers;
using ErpOnlineOrder.Domain.Constants;

namespace ErpOnlineOrder.Application.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly ICustomerManagementRepository _customerManagementRepository;
        private readonly IPermissionService _permissionService;
        private readonly IWarehouseExportRepository _warehouseExportRepository;

        public InvoiceService(IInvoiceRepository invoiceRepository, IOrderRepository orderRepository,
            IStaffRepository staffRepository,
            ICustomerManagementRepository customerManagementRepository, IPermissionService permissionService,
            IWarehouseExportRepository warehouseExportRepository)
        {
            _invoiceRepository = invoiceRepository;
            _orderRepository = orderRepository;
            _staffRepository = staffRepository;
            _customerManagementRepository = customerManagementRepository;
            _permissionService = permissionService;
            _warehouseExportRepository = warehouseExportRepository;
        }

        #region CRUD cơ bản

        public async Task<InvoiceDto?> GetByIdAsync(int id, int? userId = null)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(id);
            if (invoice == null) return null;

            if (userId.HasValue && userId.Value > 0 && !await _permissionService.IsAdminAsync(userId.Value))
            {
                var staff = await _staffRepository.GetByUserIdAsync(userId.Value);
                if (staff != null)
                {
                    var customerIds = await _customerManagementRepository.GetCustomerIdsByStaffAsync(staff.Id);
                    if (!customerIds.Contains(invoice.Customer_id))
                        return null;
                }
            }

            var dto = EntityMappers.ToInvoiceDto(invoice);
            if (userId.HasValue && userId.Value > 0)
                await RecordPermissionEnricher.EnrichInvoiceAsync(dto, userId.Value, _permissionService);
            return dto;
        }

        public async Task<IEnumerable<InvoiceDto>> GetAllAsync(int? userId = null)
        {
            IEnumerable<InvoiceDto> invoices;
            if (userId.HasValue && userId.Value > 0 && !await _permissionService.IsAdminAsync(userId.Value))
            {
                var staff = await _staffRepository.GetByUserIdAsync(userId.Value);
                if (staff != null)
                {
                    var customerIds = await _customerManagementRepository.GetCustomerIdsByStaffAsync(staff.Id);
                    invoices = await _invoiceRepository.GetByCustomerIdsAsync(customerIds);
                }
                else
                    invoices = await _invoiceRepository.GetAllAsync();
            }
            else
                invoices = await _invoiceRepository.GetAllAsync();

            var list = invoices.ToList();
            if (userId.HasValue && userId.Value > 0)
            {
                var userPermissions = (await _permissionService.GetUserPermissionsAsync(userId.Value)).ToHashSet();
                foreach (var dto in list)
                    RecordPermissionEnricher.EnrichInvoice(dto, userPermissions);
            }
            return list;
        }

        public async Task<IEnumerable<InvoiceDto>> GetForMergeAsync(int? userId = null)
        {
            IEnumerable<int>? customerIds = null;
            if (userId.HasValue && userId.Value > 0 && !await _permissionService.IsAdminAsync(userId.Value))
            {
                var staff = await _staffRepository.GetByUserIdAsync(userId.Value);
                if (staff != null)
                    customerIds = await _customerManagementRepository.GetCustomerIdsByStaffAsync(staff.Id);
            }

            var list = (await _invoiceRepository.GetForMergeAsync(customerIds)).ToList();
            if (userId.HasValue && userId.Value > 0)
            {
                var userPermissions = (await _permissionService.GetUserPermissionsAsync(userId.Value)).ToHashSet();
                foreach (var dto in list)
                    RecordPermissionEnricher.EnrichInvoice(dto, userPermissions);
            }
            return list;
        }

        public async Task<IEnumerable<InvoiceSelectDto>> GetForWarehouseExportSelectAsync(int? userId = null)
        {
            IEnumerable<int>? customerIds = null;
            if (userId.HasValue && userId.Value > 0 && !await _permissionService.IsAdminAsync(userId.Value))
            {
                var staff = await _staffRepository.GetByUserIdAsync(userId.Value);
                if (staff != null)
                    customerIds = await _customerManagementRepository.GetCustomerIdsByStaffAsync(staff.Id);
            }

            return await _invoiceRepository.GetForWarehouseExportSelectAsync(customerIds);
        }

        public async Task<PagedResult<InvoiceDto>> GetAllPagedAsync(InvoiceFilterRequest request, int? userId = null)
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

            var paged = await _invoiceRepository.GetPagedInvoicesAsync(request, customerIds);
            var dtos = paged.Items.ToList();
            if (userId.HasValue && userId.Value > 0)
            {
                var userPermissions = (await _permissionService.GetUserPermissionsAsync(userId.Value)).ToHashSet();
                foreach (var dto in dtos)
                    RecordPermissionEnricher.EnrichInvoice(dto, userPermissions);
            }
            return new PagedResult<InvoiceDto>
            {
                Items = dtos,
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount
            };
        }

        public async Task<IEnumerable<InvoiceDto>> GetByCustomerIdAsync(int customerId)
        {
            return await _invoiceRepository.GetByCustomerIdAsync(customerId);
        }

        public async Task<PagedResult<InvoiceDto>> GetByCustomerIdPagedAsync(int customerId, InvoiceFilterRequest request)
        {
            var paged = await _invoiceRepository.GetPagedInvoicesAsync(request, new[] { customerId });
            var dtos = paged.Items.ToList();
            return new PagedResult<InvoiceDto>
            {
                Items = dtos,
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount
            };
        }

        #endregion

        #region Tách/Gộp hóa đơn

        public async Task<SplitInvoiceResultDto> SplitInvoiceAsync(SplitInvoiceDto dto, int userId)
        {
            var result = new SplitInvoiceResultDto();

            var sourceInvoice = await _invoiceRepository.GetByIdForUpdateAsync(dto.Source_invoice_id);
            if (sourceInvoice == null)
            {
                result.Success = false;
                result.Message = "Hóa đơn không tồn tại";
                return result;
            }

            if (sourceInvoice.Status == InvoiceStatuses.Merged
                || sourceInvoice.Status == InvoiceStatuses.Cancelled
                || sourceInvoice.Status == InvoiceStatuses.Completed)
            {
                result.Success = false;
                result.Message = "Không thể tách hóa đơn đã gộp/hủy/hoàn thành";
                return result;
            }

            var validParts = (dto.Split_parts ?? new List<SplitInvoicePart>())
                .Select(p => new SplitInvoicePart
                {
                    Items = (p.Items ?? new List<SplitInvoiceItem>())
                        .Where(i => i != null && i.Invoice_detail_id > 0 && i.Quantity > 0)
                        .ToList()
                })
                .Where(p => p.Items.Count > 0)
                .ToList();

            if (validParts.Count == 0)
            {
                result.Success = false;
                result.Message = "Vui lòng chọn ít nhất một sản phẩm với số lượng > 0 để tách";
                return result;
            }

            var now = DateTime.UtcNow;
            var newInvoices = new List<Invoice>();

            // Tìm số suffix lớn nhất đã tồn tại (bao gồm cả bản ghi đã xóa mềm) để tránh trùng mã
            var codePrefix = $"{sourceInvoice.Invoice_code}-S";
            var startIndex = await _invoiceRepository.GetMaxChildSuffixAsync(codePrefix);

            // 3. Tạo các hóa đơn mới từ các phần tách
            foreach (var (part, index) in validParts.Select((p, i) => (p, i)))
            {
                var newInvoice = new Invoice
                {
                    Invoice_code = $"{codePrefix}{startIndex + index + 1}",
                    Invoice_date = now,
                    Customer_id = sourceInvoice.Customer_id,
                    Staff_id = sourceInvoice.Staff_id,
                    Status = InvoiceStatuses.Draft,
                    Parent_invoice_id = sourceInvoice.Id,
                    Split_merge_note = dto.Note ?? $"Tách từ hóa đơn {sourceInvoice.Invoice_code}",
                    Created_by = userId,
                    Updated_by = userId,
                    Created_at = now,
                    Updated_at = now,
                    Is_deleted = false,
                    Invoice_Details = new List<Invoice_detail>()
                };

                decimal totalAmount = 0;
                decimal taxAmount = 0;

                foreach (var item in part.Items)
                {
                    var sourceDetail = sourceInvoice.Invoice_Details
                        .FirstOrDefault(d => !d.Is_deleted && d.Id == item.Invoice_detail_id);
                    
                    if (sourceDetail == null) continue;
                    if (item.Quantity > sourceDetail.Quantity)
                    {
                        result.Success = false;
                        result.Message = $"Số lượng tách vượt quá số lượng gốc (sản phẩm ID: {sourceDetail.Product_id})";
                        return result;
                    }

                    var newDetail = new Invoice_detail
                    {
                        Product_id = sourceDetail.Product_id,
                        Quantity = item.Quantity,
                        Unit_price = sourceDetail.Unit_price,
                        Total_price = item.Quantity * sourceDetail.Unit_price,
                        Tax_rate = sourceDetail.Tax_rate,
                        Created_by = userId,
                        Updated_by = userId,
                        Created_at = now,
                        Updated_at = now,
                        Is_deleted = false
                    };

                    totalAmount += newDetail.Total_price;
                    taxAmount += newDetail.Total_price * newDetail.Tax_rate / 100;

                    newInvoice.Invoice_Details.Add(newDetail);

                    // Giảm số lượng trong hóa đơn gốc
                    sourceDetail.Quantity -= item.Quantity;
                    if (sourceDetail.Quantity <= 0)
                    {
                        sourceDetail.Is_deleted = true;
                    }
                    else
                    {
                        sourceDetail.Total_price = sourceDetail.Quantity * sourceDetail.Unit_price;
                    }
                }

                newInvoice.Total_amount = totalAmount;
                newInvoice.Tax_amount = taxAmount;

                await _invoiceRepository.AddAsync(newInvoice);
                newInvoices.Add(newInvoice);
            }

            // 4. Cập nhật hóa đơn gốc
            sourceInvoice.Status = InvoiceStatuses.Split;
            sourceInvoice.Split_merge_note = dto.Note ?? $"Đã tách thành {newInvoices.Count} hóa đơn";
            sourceInvoice.Total_amount = sourceInvoice.Invoice_Details
                .Where(d => !d.Is_deleted)
                .Sum(d => d.Total_price);
            sourceInvoice.Tax_amount = sourceInvoice.Invoice_Details
                .Where(d => !d.Is_deleted)
                .Sum(d => d.Total_price * d.Tax_rate / 100);
            sourceInvoice.Updated_by = userId;
            sourceInvoice.Updated_at = now;

            await _invoiceRepository.UpdateAsync(sourceInvoice);

            result.Success = true;
            result.Message = $"Đã tách thành công thành {newInvoices.Count} hóa đơn mới";
            result.Original_invoice = EntityMappers.ToInvoiceDto(sourceInvoice);
            result.New_invoices = newInvoices.Select(EntityMappers.ToInvoiceDto).ToList();

            return result;
        }

        public async Task<MergeInvoiceResultDto> MergeInvoicesAsync(MergeInvoicesDto dto, int userId)
        {
            var result = new MergeInvoiceResultDto();

            if (dto.Invoice_ids.Count < 2)
            {
                result.Success = false;
                result.Message = "Cần ít nhất 2 hóa đơn để gộp";
                return result;
            }

            // 1. Lấy tất cả hóa đơn cần gộp (cần load Customer_managements để lấy Province)
            var invoices = new List<Invoice>();
            int? requiredProvinceId = null;

            foreach (var id in dto.Invoice_ids)
            {
                var invoice = await _invoiceRepository.GetByIdForUpdateAsync(id);
                if (invoice == null)
                {
                    result.Success = false;
                    result.Message = $"Hóa đơn {id} không tồn tại";
                    return result;
                }

                if (invoice.Status == InvoiceStatuses.Merged || invoice.Status == InvoiceStatuses.Cancelled)
                {
                    result.Success = false;
                    result.Message = $"Hóa đơn {invoice.Invoice_code} không thể gộp (trạng thái: {invoice.Status})";
                    return result;
                }

                var mgmt = invoice.Customer?.Customer_managements?
                    .FirstOrDefault(cm => !cm.Is_deleted && cm.Province != null);
                var provinceId = mgmt?.Province_id;

                if (!provinceId.HasValue)
                {
                    result.Success = false;
                    result.Message = $"Khách hàng của hóa đơn {invoice.Invoice_code} chưa được gán tỉnh/thành phố. Vui lòng gán cán bộ phụ trách theo tỉnh cho khách hàng.";
                    return result;
                }

                if (requiredProvinceId == null)
                    requiredProvinceId = provinceId;
                else if (requiredProvinceId != provinceId)
                {
                    result.Success = false;
                    result.Message = "Tất cả hóa đơn phải từ khách hàng cùng một tỉnh/thành phố";
                    return result;
                }

                invoices.Add(invoice);
            }

            var now = DateTime.UtcNow;
            var staffId = dto.Staff_id > 0 ? dto.Staff_id : invoices.First().Staff_id;
            var customerId = dto.Customer_id > 0 ? dto.Customer_id : invoices.First().Customer_id;

            // 2. Tạo hóa đơn gộp mới (dùng khách hàng của hóa đơn đầu tiên)
            var mergedInvoice = new Invoice
            {
                Invoice_code = $"INV-M-{now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}",
                Invoice_date = now,
                Customer_id = customerId,
                Staff_id = staffId,
                Status = InvoiceStatuses.Draft,
                Split_merge_note = dto.Note ?? $"Gộp từ {invoices.Count} hóa đơn: {string.Join(", ", invoices.Select(i => i.Invoice_code))}",
                Created_by = userId,
                Updated_by = userId,
                Created_at = now,
                Updated_at = now,
                Is_deleted = false,
                Invoice_Details = new List<Invoice_detail>()
            };

            decimal totalAmount = 0;
            decimal taxAmount = 0;

            // 3. Gộp tất cả chi tiết
            foreach (var invoice in invoices)
            {
                foreach (var detail in invoice.Invoice_Details.Where(d => !d.Is_deleted))
                {
                    // Kiểm tra sản phẩm đã có trong hóa đơn gộp chưa
                    var existingDetail = mergedInvoice.Invoice_Details
                        .FirstOrDefault(d => d.Product_id == detail.Product_id && d.Unit_price == detail.Unit_price);

                    if (existingDetail != null)
                    {
                        // Cộng dồn số lượng
                        existingDetail.Quantity += detail.Quantity;
                        existingDetail.Total_price = existingDetail.Quantity * existingDetail.Unit_price;
                    }
                    else
                    {
                        // Thêm mới
                        mergedInvoice.Invoice_Details.Add(new Invoice_detail
                        {
                            Product_id = detail.Product_id,
                            Quantity = detail.Quantity,
                            Unit_price = detail.Unit_price,
                            Total_price = detail.Total_price,
                            Tax_rate = detail.Tax_rate,
                            Created_by = userId,
                            Updated_by = userId,
                            Created_at = now,
                            Updated_at = now,
                            Is_deleted = false
                        });
                    }
                }
            }

            totalAmount = mergedInvoice.Invoice_Details.Sum(d => d.Total_price);
            taxAmount = mergedInvoice.Invoice_Details.Sum(d => d.Total_price * d.Tax_rate / 100);

            mergedInvoice.Total_amount = totalAmount;
            mergedInvoice.Tax_amount = taxAmount;

            await _invoiceRepository.AddAsync(mergedInvoice);

            // 4. Cập nhật trạng thái các hóa đơn đã gộp
            foreach (var invoice in invoices)
            {
                invoice.Status = InvoiceStatuses.Merged;
                invoice.Merged_into_invoice_id = mergedInvoice.Id;
                invoice.Split_merge_note = $"Đã gộp vào hóa đơn {mergedInvoice.Invoice_code}";
                invoice.Updated_by = userId;
                invoice.Updated_at = now;
                await _invoiceRepository.UpdateAsync(invoice);
            }

            result.Success = true;
            result.Message = $"Đã gộp thành công {invoices.Count} hóa đơn";
            result.Merged_invoice = EntityMappers.ToInvoiceDto(mergedInvoice);
            result.Merged_invoice_ids = dto.Invoice_ids;

            return result;
        }

        public async Task<bool> UndoSplitAsync(int parentInvoiceId, int userId)
        {
            var parentInvoice = await _invoiceRepository.GetByIdForUpdateAsync(parentInvoiceId);
            if (parentInvoice == null || parentInvoice.Status != InvoiceStatuses.Split)
            {
                return false;
            }

            // Lấy các hóa đơn con
            var childInvoices = await _invoiceRepository.GetChildInvoicesAsync(parentInvoiceId);

            // Kiểm tra: không cho hoàn tác nếu bất kỳ hóa đơn con nào đã chuyển trạng thái hoặc có phiếu xuất kho
            foreach (var child in childInvoices)
            {
                if (child.Status != InvoiceStatuses.Draft)
                {
                    throw new InvalidOperationException(
                        $"Không thể hoàn tác tách hóa đơn vì hóa đơn con {child.Invoice_code} đã có trạng thái: {InvoiceStatuses.ToDisplayText(child.Status)}");
                }

                var exports = await _warehouseExportRepository.GetByInvoiceIdAsync(child.Id);
                if (exports.Any())
                {
                    throw new InvalidOperationException(
                        $"Không thể hoàn tác tách hóa đơn vì hóa đơn con {child.Invoice_code} đã có phiếu xuất kho");
                }
            }

            var now = DateTime.UtcNow;

            // Khôi phục số lượng về hóa đơn gốc
            foreach (var childInvoice in childInvoices)
            {
                foreach (var childDetail in childInvoice.Invoice_Details.Where(d => !d.Is_deleted))
                {
                    var parentDetail = parentInvoice.Invoice_Details
                        .FirstOrDefault(d => d.Product_id == childDetail.Product_id);

                    if (parentDetail != null)
                    {
                        if (parentDetail.Is_deleted)
                        {
                            parentDetail.Is_deleted = false;
                            parentDetail.Quantity = childDetail.Quantity;
                        }
                        else
                        {
                            parentDetail.Quantity += childDetail.Quantity;
                        }
                        parentDetail.Total_price = parentDetail.Quantity * parentDetail.Unit_price;
                    }
                }

                // Xóa hóa đơn con
                await _invoiceRepository.DeleteAsync(childInvoice.Id);
            }

            // Cập nhật hóa đơn gốc
            parentInvoice.Status = InvoiceStatuses.Draft;
            parentInvoice.Split_merge_note = null;
            parentInvoice.Total_amount = parentInvoice.Invoice_Details
                .Where(d => !d.Is_deleted)
                .Sum(d => d.Total_price);
            parentInvoice.Tax_amount = parentInvoice.Invoice_Details
                .Where(d => !d.Is_deleted)
                .Sum(d => d.Total_price * d.Tax_rate / 100);
            parentInvoice.Updated_by = userId;
            parentInvoice.Updated_at = now;
            await _invoiceRepository.UpdateAsync(parentInvoice);

            return true;
        }

        public async Task<bool> UndoMergeAsync(int mergedInvoiceId, int userId)
        {
            var mergedInvoice = await _invoiceRepository.GetByIdAsync(mergedInvoiceId);
            if (mergedInvoice == null)
            {
                return false;
            }

            var now = DateTime.UtcNow;

            // Lấy các hóa đơn đã gộp vào
            var sourceInvoices = (await _invoiceRepository.GetByMergedIntoInvoiceIdAsync(mergedInvoiceId)).ToList();

            // Khôi phục trạng thái các hóa đơn gốc
            foreach (var invoice in sourceInvoices)
            {
                invoice.Status = InvoiceStatuses.Draft;
                invoice.Merged_into_invoice_id = null;
                invoice.Split_merge_note = null;
                invoice.Updated_by = userId;
                invoice.Updated_at = now;
                await _invoiceRepository.UpdateAsync(invoice);
            }

            // Xóa hóa đơn gộp
            await _invoiceRepository.DeleteAsync(mergedInvoiceId);

            return true;
        }

        public async Task<bool> UpdateStatusAsync(int id, string newStatus, int userId)
        {
            var invoice = await _invoiceRepository.GetByIdForUpdateAsync(id);
            if (invoice == null) return false;

            if (!InvoiceStatuses.CanTransition(invoice.Status, newStatus))
                throw new Exception($"Không thể chuyển trạng thái hóa đơn từ '{InvoiceStatuses.ToDisplayText(invoice.Status)}' sang '{InvoiceStatuses.ToDisplayText(newStatus)}'");

            invoice.Status = newStatus;
            invoice.Updated_by = userId;
            invoice.Updated_at = DateTime.UtcNow;
            await _invoiceRepository.UpdateAsync(invoice);

            // Đồng bộ trạng thái sang phiếu xuất kho tương ứng
            await SyncStatusToExportsAsync(invoice.Id, newStatus, userId);

            return true;
        }

        private async Task SyncStatusToExportsAsync(int invoiceId, string invoiceStatus, int userId)
        {
            var exports = await _warehouseExportRepository.GetByInvoiceIdAsync(invoiceId);
            var activeExports = exports.Where(e => !e.Is_deleted && e.Status != ExportStatuses.Cancelled).ToList();

            foreach (var export in activeExports)
            {
                var changed = false;

                if (invoiceStatus == InvoiceStatuses.Confirmed && export.Status == ExportStatuses.Draft)
                {
                    export.Status = ExportStatuses.Confirmed;
                    changed = true;
                }
                else if (invoiceStatus == InvoiceStatuses.Completed && export.Status != ExportStatuses.Completed)
                {
                    export.Status = ExportStatuses.Completed;
                    if (export.Delivery_status != DeliveryStatuses.Delivered)
                        export.Delivery_status = DeliveryStatuses.Delivered;
                    changed = true;
                }
                else if (invoiceStatus == InvoiceStatuses.Cancelled)
                {
                    export.Status = ExportStatuses.Cancelled;
                    changed = true;
                }

                if (changed)
                {
                    export.Updated_by = userId;
                    export.Updated_at = DateTime.UtcNow;
                    await _warehouseExportRepository.UpdateAsync(export);
                }
            }
        }

        #endregion

        #region Tạo hóa đơn từ đơn hàng

        // public async Task<CreateInvoiceFromOrderResultDto> CreateInvoiceFromOrderAsync(int orderId, int userId)
        // {
        //     var result = new CreateInvoiceFromOrderResultDto();

        //     var order = await _orderRepository.GetByIdAsync(orderId);
        //     if (order == null)
        //     {
        //         result.Success = false;
        //         result.Message = "Đơn hàng không tồn tại";
        //         return result;
        //     }

        //     if (order.Order_status != "Confirmed")
        //     {
        //         result.Success = false;
        //         result.Message = "Chỉ có thể tạo hóa đơn từ đơn hàng đã được xác nhận";
        //         return result;
        //     }

        //     var activeDetails = order.Order_Details?.Where(d => !d.Is_deleted).ToList();
        //     if (activeDetails == null || activeDetails.Count == 0)
        //     {
        //         result.Success = false;
        //         result.Message = "Đơn hàng không có chi tiết sản phẩm";
        //         return result;
        //     }

        //     // Lấy Staff_id từ cán bộ đang thao tác
        //     var staff = await _staffRepository.GetByUserIdAsync(userId);
        //     if (staff == null)
        //     {
        //         result.Success = false;
        //         result.Message = "Không tìm thấy thông tin cán bộ. Chỉ cán bộ mới có thể tạo hóa đơn.";
        //         return result;
        //     }
        //     var staffId = staff.Id;

        //     var now = DateTime.UtcNow;
        //     var invoice = new Invoice
        //     {
        //         Invoice_code = $"INV-{now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
        //         Invoice_date = now,
        //         Customer_id = order.Customer_id,
        //         Staff_id = staffId,
        //         Order_id = orderId,
        //         Status = InvoiceStatuses.Draft,
        //         Created_by = userId,
        //         Updated_by = userId,
        //         Created_at = now,
        //         Updated_at = now,
        //         Is_deleted = false,
        //         Invoice_Details = new List<Invoice_detail>()
        //     };

        //     decimal totalAmount = 0;
        //     decimal taxAmount = 0;

        //     foreach (var od in activeDetails)
        //     {
        //         var detail = new Invoice_detail
        //         {
        //             Product_id = od.Product_id,
        //             Quantity = od.Quantity,
        //             Unit_price = od.Unit_price,
        //             Total_price = od.Total_price,
        //             Tax_rate = od.Tax_rate,
        //             Created_by = userId,
        //             Updated_by = userId,
        //             Created_at = now,
        //             Updated_at = now,
        //             Is_deleted = false
        //         };

        //         totalAmount += detail.Total_price;
        //         taxAmount += detail.Total_price * detail.Tax_rate / 100;

        //         invoice.Invoice_Details.Add(detail);
        //     }

        //     invoice.Total_amount = totalAmount;
        //     invoice.Tax_amount = taxAmount;

        //     await _invoiceRepository.AddAsync(invoice);

        //     result.Success = true;
        //     result.Message = $"Đã tạo hóa đơn nháp {invoice.Invoice_code} từ đơn hàng {order.Order_code}";
        //     result.Invoice = EntityMappers.ToInvoiceDto(invoice);

        //     return result;
        // }

        #endregion

        public async Task<byte[]> ExportInvoicesToExcelAsync(string? status = null)
        {
            var invoices = !string.IsNullOrEmpty(status)
                ? await _invoiceRepository.GetByStatusAsync(status)
                : await _invoiceRepository.GetAllAsync();
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Hóa đơn");

            ws.Cell(1, 1).Value = "Mã HĐ";
            ws.Cell(1, 2).Value = "Ngày lập";
            ws.Cell(1, 3).Value = "Khách hàng";
            ws.Cell(1, 4).Value = "Tỉnh/TP";
            ws.Cell(1, 5).Value = "Tổng tiền";
            ws.Cell(1, 6).Value = "Thuế";
            ws.Cell(1, 7).Value = "Thành tiền";
            ws.Cell(1, 8).Value = "Trạng thái";
            ws.Range(1, 1, 1, 8).Style.Font.Bold = true;

            int row = 2;
            foreach (var dto in invoices)
            {
                ExcelHelper.SetCellValue(ws.Cell(row, 1), dto.Invoice_code);
                ExcelHelper.SetCellValue(ws.Cell(row, 2), dto.Invoice_date.ToString("dd/MM/yyyy"));
                ExcelHelper.SetCellValue(ws.Cell(row, 3), dto.Customer_name ?? "");
                ExcelHelper.SetCellValue(ws.Cell(row, 4), dto.Province_name ?? "");
                ws.Cell(row, 5).Value = dto.Total_amount;
                ws.Cell(row, 6).Value = dto.Tax_amount;
                ws.Cell(row, 7).Value = dto.Grand_total;
                ExcelHelper.SetCellValue(ws.Cell(row, 8), dto.Status ?? "");
                row++;
            }
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream, false);
            return stream.ToArray();
        }

    }
}