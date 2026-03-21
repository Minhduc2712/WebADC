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
        private readonly IEmailService _emailService;
        private readonly ICustomerRepository _customerRepository;

        public InvoiceService(IInvoiceRepository invoiceRepository, IOrderRepository orderRepository,
            IStaffRepository staffRepository,
            ICustomerManagementRepository customerManagementRepository, IPermissionService permissionService,
            IWarehouseExportRepository warehouseExportRepository,
            IEmailService emailService,
            ICustomerRepository customerRepository)
        {
            _invoiceRepository = invoiceRepository;
            _orderRepository = orderRepository;
            _staffRepository = staffRepository;
            _customerManagementRepository = customerManagementRepository;
            _permissionService = permissionService;
            _warehouseExportRepository = warehouseExportRepository;
            _emailService = emailService;
            _customerRepository = customerRepository;
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

            var remainingDetails = sourceInvoice.Invoice_Details
                .Where(d => !d.Is_deleted)
                .ToDictionary(d => d.Id, d => d.Quantity);

            // 3. Tạo các hóa đơn mới từ các phần tách
            int partIndex = 0;
            foreach (var part in validParts)
            {
                var newInvoice = new Invoice
                {
                    Invoice_code = $"{codePrefix}{startIndex + partIndex + 1}",
                    Invoice_date = now,
                    Customer_id = sourceInvoice.Customer_id,
                    Staff_id = sourceInvoice.Staff_id,
                    Order_id = sourceInvoice.Order_id,
                    Warehouse_export_id = sourceInvoice.Warehouse_export_id,
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

                    if (remainingDetails.ContainsKey(item.Invoice_detail_id))
                    {
                        remainingDetails[item.Invoice_detail_id] -= item.Quantity;
                    }
                }

                newInvoice.Total_amount = totalAmount;
                newInvoice.Tax_amount = taxAmount;

                await _invoiceRepository.AddAsync(newInvoice);
                newInvoices.Add(newInvoice);
                partIndex++;
            }

            // TẠO HÓA ĐƠN CON THỨ 2: Chứa số lượng còn lại
            if (remainingDetails.Any(r => r.Value > 0))
            {
                var remainInvoice = new Invoice
                {
                    Invoice_code = $"{codePrefix}{startIndex + partIndex + 1}",
                    Invoice_date = now, Customer_id = sourceInvoice.Customer_id, Staff_id = sourceInvoice.Staff_id,
                    Order_id = sourceInvoice.Order_id, Warehouse_export_id = sourceInvoice.Warehouse_export_id,
                    Status = InvoiceStatuses.Draft, Parent_invoice_id = sourceInvoice.Id,
                    Split_merge_note = dto.Note != null ? $"{dto.Note} (Phần còn lại)" : $"Phần còn lại từ hóa đơn {sourceInvoice.Invoice_code}",
                    Created_by = userId, Updated_by = userId, Created_at = now, Updated_at = now, Is_deleted = false,
                    Invoice_Details = new List<Invoice_detail>()
                };
                decimal totalAmount = 0; decimal taxAmount = 0;
                foreach (var remain in remainingDetails.Where(r => r.Value > 0))
                {
                    var sourceDetail = sourceInvoice.Invoice_Details.First(d => d.Id == remain.Key);
                    var newDetail = new Invoice_detail
                    {
                        Product_id = sourceDetail.Product_id, Quantity = remain.Value, Unit_price = sourceDetail.Unit_price,
                        Total_price = remain.Value * sourceDetail.Unit_price, Tax_rate = sourceDetail.Tax_rate,
                        Created_by = userId, Updated_by = userId, Created_at = now, Updated_at = now, Is_deleted = false
                    };
                    totalAmount += newDetail.Total_price; taxAmount += newDetail.Total_price * newDetail.Tax_rate / 100;
                    remainInvoice.Invoice_Details.Add(newDetail);
                }
                remainInvoice.Total_amount = totalAmount; remainInvoice.Tax_amount = taxAmount;
                await _invoiceRepository.AddAsync(remainInvoice);
                newInvoices.Add(remainInvoice);
            }

            // 4. Cập nhật hóa đơn gốc (GIỮ NGUYÊN SỐ LƯỢNG)
            sourceInvoice.Status = InvoiceStatuses.Split;
            sourceInvoice.Split_merge_note = dto.Note ?? $"Đã tách thành {newInvoices.Count} hóa đơn con";
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

                if (child.Warehouse_export_id.HasValue && child.Warehouse_export_id != parentInvoice.Warehouse_export_id)
                {
                    throw new InvalidOperationException(
                        $"Không thể hoàn tác tách hóa đơn vì hóa đơn con {child.Invoice_code} đã liên kết với phiếu xuất kho khác");
                }
            }

            var now = DateTime.UtcNow;

            // Khôi phục số lượng về hóa đơn gốc
            foreach (var childInvoice in childInvoices)
            {
                // Xóa hóa đơn con
                await _invoiceRepository.DeleteAsync(childInvoice.Id);
            }

            // Cập nhật hóa đơn gốc
            parentInvoice.Status = InvoiceStatuses.Draft;
            parentInvoice.Split_merge_note = null;
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
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null || !invoice.Warehouse_export_id.HasValue) return;

            var export = await _warehouseExportRepository.GetByIdAsync(invoice.Warehouse_export_id.Value);
            if (export == null || export.Is_deleted || export.Status == ExportStatuses.Cancelled) return;

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
                var allInvoicesInExport = await _invoiceRepository.GetByWarehouseExportIdAsync(export.Id);
                var hasOtherActive = allInvoicesInExport.Any(i => i.Id != invoiceId && i.Status != InvoiceStatuses.Cancelled);
                if (!hasOtherActive)
                {
                    export.Status = ExportStatuses.Cancelled;
                    changed = true;
                }
            }

            if (changed)
            {
                export.Updated_by = userId;
                export.Updated_at = DateTime.UtcNow;
                await _warehouseExportRepository.UpdateAsync(export);
            }
        }

        #endregion

        #region Tạo hóa đơn từ phiếu xuất kho

        public async Task<InvoiceDto?> CreateInvoiceFromExportAsync(int exportId, int userId)
        {
            var export = await _warehouseExportRepository.GetByIdAsync(exportId);
            if (export == null)
                throw new Exception("Phiếu xuất kho không tồn tại.");

            // Ràng buộc phải giao hàng xong mới xuất hóa đơn thủ công
            if (export.Delivery_status != DeliveryStatuses.Delivered && export.Status != ExportStatuses.Completed)
                throw new Exception("Chỉ có thể xuất hóa đơn cho phiếu xuất kho đã giao hàng thành công.");

            var existingInvoices = await _invoiceRepository.GetByWarehouseExportIdAsync(exportId);
            if (existingInvoices.Any(i => i.Status != InvoiceStatuses.Cancelled))
                throw new Exception("Phiếu xuất kho này đã được xuất hóa đơn.");

            var staff = await _staffRepository.GetByUserIdAsync(userId);
            if (staff == null)
                throw new Exception("Không tìm thấy thông tin cán bộ. Chỉ cán bộ mới có thể tạo hóa đơn.");

            var now = DateTime.UtcNow;
            var invoice = new Invoice
            {
                Invoice_code = $"INV-{now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                Invoice_date = now,
                Customer_id = export.Customer_id,
                Staff_id = staff.Id,
                Order_id = export.Order_id,
                Warehouse_export_id = export.Id, // Liên kết chặt chẽ với phiếu xuất kho
                Status = InvoiceStatuses.Draft,
                Created_by = userId,
                Updated_by = userId,
                Created_at = now,
                Updated_at = now,
                Is_deleted = false,
                Invoice_Details = new List<Invoice_detail>()
            };

            decimal totalAmount = 0;
            decimal taxAmount = 0;

            foreach (var detail in export.Warehouse_Export_Details.Where(d => !d.Is_deleted))
            {
                var taxRate = detail.Product?.Tax_rate ?? 0;

                var invDetail = new Invoice_detail
                {
                    Product_id = detail.Product_id,
                    Quantity = detail.Quantity_shipped, // Lấy số lượng xuất thực tế
                    Unit_price = detail.Unit_price,
                    Total_price = detail.Total_price,
                    Tax_rate = taxRate,
                    Created_by = userId,
                    Updated_by = userId,
                    Created_at = now,
                    Updated_at = now,
                    Is_deleted = false
                };

                totalAmount += invDetail.Total_price;
                taxAmount += invDetail.Total_price * taxRate / 100;

                invoice.Invoice_Details.Add(invDetail);
            }

            invoice.Total_amount = totalAmount;
            invoice.Tax_amount = taxAmount;

            await _invoiceRepository.AddAsync(invoice);
            
            return EntityMappers.ToInvoiceDto(invoice);
        }

        public async Task<List<InvoiceDto>> CustomerRequestInvoiceAsync(CustomerInvoiceRequestDto dto, int customerId, int userId)
        {
            var export = await _warehouseExportRepository.GetByIdAsync(dto.WarehouseExportId);
            if (export == null || export.Customer_id != customerId)
                throw new Exception("Phiếu xuất kho không tồn tại hoặc không thuộc quyền sở hữu của bạn.");

            var existingInvoices = await _invoiceRepository.GetByWarehouseExportIdAsync(export.Id);
            if (existingInvoices.Any(i => i.Status != InvoiceStatuses.Cancelled))
                throw new Exception("Phiếu xuất kho này đã được xử lý hóa đơn.");

            var now = DateTime.UtcNow;
            var createdInvoices = new List<Invoice>();

            // TRƯỜNG HỢP 1: Yêu cầu tách phiếu thành nhiều hóa đơn
            if (dto.SplitParts != null && dto.SplitParts.Any())
            {
                // TẠO HÓA ĐƠN CHA TRƯỚC THEO YÊU CẦU
                var parentInvoice = new Invoice
                {
                    Invoice_code = $"INV-REQ-{now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                    Invoice_date = now,
                    Customer_id = customerId,
                    Staff_id = export.Staff_id,
                    Order_id = export.Order_id,
                    Warehouse_export_id = export.Id,
                    Status = InvoiceStatuses.Split, // Đã tách
                    Split_merge_note = $"Hóa đơn cha (Khách yêu cầu tách): {dto.Note}",
                    Created_by = userId, Updated_by = userId, Created_at = now, Updated_at = now, Is_deleted = false,
                    Invoice_Details = new List<Invoice_detail>()
                };

                decimal parentTotalAmount = 0; decimal parentTaxAmount = 0;
                foreach (var detail in export.Warehouse_Export_Details.Where(d => !d.Is_deleted))
                {
                    var taxRate = detail.Product?.Tax_rate ?? 0;
                    parentInvoice.Invoice_Details.Add(new Invoice_detail { Product_id = detail.Product_id, Quantity = detail.Quantity_shipped, Unit_price = detail.Unit_price, Total_price = detail.Total_price, Tax_rate = taxRate, Created_by = userId, Updated_by = userId, Created_at = now, Updated_at = now, Is_deleted = false });
                    parentTotalAmount += detail.Total_price; parentTaxAmount += detail.Total_price * taxRate / 100;
                }
                parentInvoice.Total_amount = parentTotalAmount; parentInvoice.Tax_amount = parentTaxAmount;
                await _invoiceRepository.AddAsync(parentInvoice);
                createdInvoices.Add(parentInvoice);

                var exportDetailsDict = export.Warehouse_Export_Details.Where(d => !d.Is_deleted).ToDictionary(d => d.Product_id, d => d);
                int partIndex = 1;
                
                var remainingDetails = exportDetailsDict.Values.ToDictionary(d => d.Product_id, d => d.Quantity_shipped);

                foreach (var part in dto.SplitParts)
                {
                    var invoice = new Invoice
                    {
                        Invoice_code = $"{parentInvoice.Invoice_code}-S{partIndex}",
                        Invoice_date = now,
                        Customer_id = customerId,
                        Staff_id = export.Staff_id, // Chuyển giao cho Staff đang quản lý phiếu xuất kho này
                        Order_id = export.Order_id,
                        Warehouse_export_id = export.Id,
                        Parent_invoice_id = parentInvoice.Id, // Link đến hóa đơn cha
                        Status = InvoiceStatuses.Draft, // Lưu dưới dạng Nháp để chờ Kế toán duyệt
                        Split_merge_note = $"Khách yêu cầu tách: {dto.Note} (Phần {partIndex})",
                        Created_by = userId, Updated_by = userId, Created_at = now, Updated_at = now, Is_deleted = false,
                        Invoice_Details = new List<Invoice_detail>()
                    };

                    decimal totalAmount = 0; decimal taxAmount = 0;

                    foreach (var item in part.Items)
                    {
                        if (!exportDetailsDict.TryGetValue(item.ProductId, out var exportDetail))
                            throw new Exception($"Sản phẩm ID {item.ProductId} không có trong phiếu xuất kho.");
                        if (item.Quantity <= 0 || item.Quantity > exportDetail.Quantity_shipped)
                            throw new Exception($"Số lượng yêu cầu cho sản phẩm ID {item.ProductId} không hợp lệ.");

                        var taxRate = exportDetail.Product?.Tax_rate ?? 0;
                        var detailTotal = item.Quantity * exportDetail.Unit_price;

                        invoice.Invoice_Details.Add(new Invoice_detail
                        {
                            Product_id = item.ProductId, Quantity = item.Quantity, Unit_price = exportDetail.Unit_price,
                            Total_price = detailTotal, Tax_rate = taxRate,
                            Created_by = userId, Updated_by = userId, Created_at = now, Updated_at = now, Is_deleted = false
                        });
                        totalAmount += detailTotal; taxAmount += detailTotal * taxRate / 100;

                        if (remainingDetails.ContainsKey(item.ProductId))
                        {
                            remainingDetails[item.ProductId] -= item.Quantity;
                        }
                    }
                    invoice.Total_amount = totalAmount; invoice.Tax_amount = taxAmount;
                    await _invoiceRepository.AddAsync(invoice);
                    createdInvoices.Add(invoice);
                    partIndex++;
                }

                // Tách hóa đơn con cho phần số lượng còn lại
                if (remainingDetails.Any(r => r.Value > 0))
                {
                    var invoice = new Invoice
                    {
                        Invoice_code = $"{parentInvoice.Invoice_code}-S{partIndex}",
                        Invoice_date = now, Customer_id = customerId, Staff_id = export.Staff_id, Order_id = export.Order_id, Warehouse_export_id = export.Id,
                        Parent_invoice_id = parentInvoice.Id, // Link đến hóa đơn cha
                        Status = InvoiceStatuses.Draft, Split_merge_note = $"Khách yêu cầu tách (Phần còn lại)",
                        Created_by = userId, Updated_by = userId, Created_at = now, Updated_at = now, Is_deleted = false,
                        Invoice_Details = new List<Invoice_detail>()
                    };
                    decimal totalAmount = 0; decimal taxAmount = 0;
                    foreach (var remain in remainingDetails.Where(r => r.Value > 0))
                    {
                        var exportDetail = exportDetailsDict[remain.Key];
                        var taxRate = exportDetail.Product?.Tax_rate ?? 0;
                        invoice.Invoice_Details.Add(new Invoice_detail
                        {
                            Product_id = remain.Key, Quantity = remain.Value, Unit_price = exportDetail.Unit_price,
                            Total_price = remain.Value * exportDetail.Unit_price, Tax_rate = taxRate,
                            Created_by = userId, Updated_by = userId, Created_at = now, Updated_at = now, Is_deleted = false
                        });
                        totalAmount += remain.Value * exportDetail.Unit_price;
                        taxAmount += remain.Value * exportDetail.Unit_price * taxRate / 100;
                    }
                    invoice.Total_amount = totalAmount; invoice.Tax_amount = taxAmount;
                    await _invoiceRepository.AddAsync(invoice);
                    createdInvoices.Add(invoice);
                }
            }
            // TRƯỜNG HỢP 2: Yêu cầu xuất 1 hóa đơn tổng
            else
            {
                var invoice = new Invoice
                {
                    Invoice_code = $"INV-REQ-{now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                    Invoice_date = now, Customer_id = customerId, Staff_id = export.Staff_id, Order_id = export.Order_id, Warehouse_export_id = export.Id,
                    Status = InvoiceStatuses.Draft, Split_merge_note = $"Khách yêu cầu xuất HĐ: {dto.Note}",
                    Created_by = userId, Updated_by = userId, Created_at = now, Updated_at = now, Is_deleted = false,
                    Invoice_Details = new List<Invoice_detail>()
                };
                decimal totalAmount = 0; decimal taxAmount = 0;
                foreach (var detail in export.Warehouse_Export_Details.Where(d => !d.Is_deleted))
                {
                    var taxRate = detail.Product?.Tax_rate ?? 0;
                    invoice.Invoice_Details.Add(new Invoice_detail { Product_id = detail.Product_id, Quantity = detail.Quantity_shipped, Unit_price = detail.Unit_price, Total_price = detail.Total_price, Tax_rate = taxRate, Created_by = userId, Updated_by = userId, Created_at = now, Updated_at = now, Is_deleted = false });
                    totalAmount += detail.Total_price; taxAmount += detail.Total_price * taxRate / 100;
                }
                invoice.Total_amount = totalAmount; invoice.Tax_amount = taxAmount;
                await _invoiceRepository.AddAsync(invoice);
                createdInvoices.Add(invoice);
            }

            // Gửi thông báo cho Kế toán / Admin
            try { await _emailService.SendCustomerInvoiceRequestNotificationAsync(export.Id, createdInvoices.Count); } catch { }

            return createdInvoices.Select(EntityMappers.ToInvoiceDto).ToList();
        }
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