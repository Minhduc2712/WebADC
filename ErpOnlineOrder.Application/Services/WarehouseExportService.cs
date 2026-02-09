using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;
using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Services
{
    public class WarehouseExportService : IWarehouseExportService
    {
        private readonly IWarehouseExportRepository _exportRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IInvoiceService _invoiceService;

        public WarehouseExportService(
            IWarehouseExportRepository exportRepository,
            IInvoiceRepository invoiceRepository,
            IInvoiceService invoiceService)
        {
            _exportRepository = exportRepository;
            _invoiceRepository = invoiceRepository;
            _invoiceService = invoiceService;
        }

        #region CRUD c? b?n

        public async Task<WarehouseExportDto?> GetByIdAsync(int id)
        {
            var export = await _exportRepository.GetByIdAsync(id);
            return export != null ? MapToDto(export) : null;
        }

        public async Task<IEnumerable<WarehouseExportDto>> GetAllAsync()
        {
            var exports = await _exportRepository.GetAllAsync();
            return exports.Select(MapToDto);
        }

        public async Task<IEnumerable<WarehouseExportDto>> GetByInvoiceIdAsync(int invoiceId)
        {
            var exports = await _exportRepository.GetByInvoiceIdAsync(invoiceId);
            return exports.Select(MapToDto);
        }

        public async Task<IEnumerable<WarehouseExportDto>> GetByCustomerIdAsync(int customerId)
        {
            var exports = await _exportRepository.GetByCustomerIdAsync(customerId);
            return exports.Select(MapToDto);
        }

        public async Task<IEnumerable<WarehouseExportDto>> GetByWarehouseIdAsync(int warehouseId)
        {
            var exports = await _exportRepository.GetByWarehouseIdAsync(warehouseId);
            return exports.Select(MapToDto);
        }

        public async Task<WarehouseExportDto?> CreateExportFromInvoiceAsync(CreateWarehouseExportDto dto, int userId)
        {
            // Ki?m tra hóa ??n t?n t?i
            var invoice = await _invoiceRepository.GetByIdAsync(dto.Invoice_id);
            if (invoice == null)
            {
                throw new Exception("Hóa ??n không t?n t?i");
            }

            // Ki?m tra hóa ??n ?ã có phi?u xu?t kho ch?a
            var existingExport = await HasExportForInvoiceAsync(dto.Invoice_id);
            if (existingExport)
            {
                throw new Exception("Hóa ??n này ?ã có phi?u xu?t kho");
            }

            // Ki?m tra tr?ng thái hóa ??n
            if (invoice.Status != "Confirmed" && invoice.Status != "Draft")
            {
                throw new Exception($"Không th? t?o phi?u xu?t kho cho hóa ??n có tr?ng thái: {invoice.Status}");
            }

            var now = DateTime.UtcNow;

            var export = new Warehouse_export
            {
                Warehouse_export_code = $"EXP-{now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}",
                Warehouse_id = dto.Warehouse_id,
                Invoice_id = dto.Invoice_id,
                Order_id = invoice.Order_id,
                Customer_id = invoice.Customer_id,
                Staff_id = dto.Staff_id,
                Export_date = dto.Export_date ?? now,
                Carrier_name = dto.Carrier_name,
                Tracking_number = dto.Tracking_number,
                Delivery_status = "Pending",
                Status = "Draft",
                Created_by = userId,
                Updated_by = userId,
                Created_at = now,
                Updated_at = now,
                Is_deleted = false,
                Warehouse_Export_Details = new List<Warehouse_export_detail>()
            };

            // N?u có chi ti?t, s? d?ng chi ti?t ???c cung c?p
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
                // T? ??ng l?y t? hóa ??n
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

            await _exportRepository.AddAsync(export);

            var created = await _exportRepository.GetByIdAsync(export.Id);
            return created != null ? MapToDto(created) : null;
        }

        public async Task<bool> UpdateDeliveryStatusAsync(int id, string status, int userId)
        {
            var export = await _exportRepository.GetByIdAsync(id);
            if (export == null) return false;

            export.Delivery_status = status;
            export.Updated_by = userId;
            export.Updated_at = DateTime.UtcNow;

            await _exportRepository.UpdateAsync(export);
            return true;
        }

        public async Task<bool> ConfirmExportAsync(int id, int userId)
        {
            var export = await _exportRepository.GetByIdAsync(id);
            if (export == null) return false;

            if (export.Status != "Draft")
            {
                throw new Exception("Ch? có th? xác nh?n phi?u xu?t kho ? tr?ng thái Draft");
            }

            export.Status = "Confirmed";
            export.Updated_by = userId;
            export.Updated_at = DateTime.UtcNow;

            await _exportRepository.UpdateAsync(export);
            return true;
        }

        public async Task<bool> CancelExportAsync(int id, int userId)
        {
            var export = await _exportRepository.GetByIdAsync(id);
            if (export == null) return false;

            if (export.Delivery_status == "Delivered")
            {
                throw new Exception("Không th? h?y phi?u xu?t kho ?ã giao hàng");
            }

            export.Status = "Cancelled";
            export.Updated_by = userId;
            export.Updated_at = DateTime.UtcNow;

            await _exportRepository.UpdateAsync(export);
            return true;
        }

        public async Task<bool> DeleteExportAsync(int id)
        {
            await _exportRepository.DeleteAsync(id);
            return true;
        }

        #endregion

        #region Tách/G?p phi?u xu?t kho

        public async Task<SplitExportResultDto> SplitExportAsync(SplitWarehouseExportDto dto, int userId)
        {
            var result = new SplitExportResultDto();

            // 1. L?y phi?u xu?t kho g?c
            var sourceExport = await _exportRepository.GetByIdAsync(dto.Source_export_id);
            if (sourceExport == null)
            {
                result.Success = false;
                result.Message = "Phi?u xu?t kho không t?n t?i";
                return result;
            }

            // 2. Ki?m tra tr?ng thái
            if (sourceExport.Status == "Split" || sourceExport.Status == "Merged" || sourceExport.Status == "Cancelled")
            {
                result.Success = false;
                result.Message = $"Không th? tách phi?u xu?t kho có tr?ng thái: {sourceExport.Status}";
                return result;
            }

            if (sourceExport.Delivery_status == "Delivered")
            {
                result.Success = false;
                result.Message = "Không th? tách phi?u xu?t kho ?ã giao hàng";
                return result;
            }

            var now = DateTime.UtcNow;
            var newExports = new List<Warehouse_export>();
            var newInvoiceIds = new List<int>();

            // 3. Tách hóa ??n tr??c (n?u c?n)
            if (dto.Auto_split_invoice && sourceExport.Invoice != null)
            {
                var splitInvoiceDto = new SplitInvoiceDto
                {
                    Source_invoice_id = sourceExport.Invoice_id,
                    Note = dto.Note ?? $"Tách theo phi?u xu?t kho {sourceExport.Warehouse_export_code}",
                    Split_parts = dto.Split_parts.Select(part => new SplitInvoicePart
                    {
                        Items = part.Items.Select(item =>
                        {
                            var exportDetail = sourceExport.Warehouse_Export_Details
                                .FirstOrDefault(d => d.Id == item.Export_detail_id);
                            
                            // Tìm invoice detail t??ng ?ng
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

            // 4. T?o các phi?u xu?t kho m?i t? các ph?n tách
            int partIndex = 0;
            foreach (var part in dto.Split_parts)
            {
                var newExport = new Warehouse_export
                {
                    Warehouse_export_code = $"{sourceExport.Warehouse_export_code}-S{partIndex + 1}",
                    Warehouse_id = sourceExport.Warehouse_id,
                    Invoice_id = newInvoiceIds.Count > partIndex ? newInvoiceIds[partIndex] : sourceExport.Invoice_id,
                    Order_id = sourceExport.Order_id,
                    Customer_id = sourceExport.Customer_id,
                    Staff_id = sourceExport.Staff_id,
                    Export_date = now,
                    Carrier_name = sourceExport.Carrier_name,
                    Tracking_number = null, // Tracking m?i cho m?i phi?u
                    Delivery_status = "Pending",
                    Status = "Draft",
                    Parent_export_id = sourceExport.Id,
                    Split_merge_note = dto.Note ?? $"Tách t? phi?u {sourceExport.Warehouse_export_code}",
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

                    // Gi?m s? l??ng trong phi?u g?c
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

            // 5. C?p nh?t phi?u xu?t kho g?c
            sourceExport.Status = "Split";
            sourceExport.Split_merge_note = dto.Note ?? $"?ã tách thành {newExports.Count} phi?u";
            sourceExport.Updated_by = userId;
            sourceExport.Updated_at = now;

            await _exportRepository.UpdateAsync(sourceExport);

            result.Success = true;
            result.Message = $"?ã tách thành công thành {newExports.Count} phi?u xu?t kho m?i";
            result.Original_export = MapToDto(sourceExport);
            result.New_exports = newExports.Select(e => MapToDto(e)).ToList();
            result.New_invoice_ids = newInvoiceIds;

            return result;
        }

        public async Task<MergeExportResultDto> MergeExportsAsync(MergeWarehouseExportsDto dto, int userId)
        {
            var result = new MergeExportResultDto();

            if (dto.Export_ids.Count < 2)
            {
                result.Success = false;
                result.Message = "C?n ít nh?t 2 phi?u xu?t kho ?? g?p";
                return result;
            }

            // 1. L?y t?t c? phi?u xu?t kho c?n g?p
            var exports = new List<Warehouse_export>();
            int? customerId = null;

            foreach (var id in dto.Export_ids)
            {
                var export = await _exportRepository.GetByIdAsync(id);
                if (export == null)
                {
                    result.Success = false;
                    result.Message = $"Phi?u xu?t kho {id} không t?n t?i";
                    return result;
                }

                if (export.Status == "Merged" || export.Status == "Cancelled")
                {
                    result.Success = false;
                    result.Message = $"Phi?u {export.Warehouse_export_code} không th? g?p (tr?ng thái: {export.Status})";
                    return result;
                }

                if (export.Delivery_status == "Delivered")
                {
                    result.Success = false;
                    result.Message = $"Phi?u {export.Warehouse_export_code} ?ã giao hàng, không th? g?p";
                    return result;
                }

                if (customerId == null)
                {
                    customerId = export.Customer_id;
                }
                else if (export.Customer_id != customerId)
                {
                    result.Success = false;
                    result.Message = "T?t c? phi?u xu?t kho ph?i cùng m?t khách hàng";
                    return result;
                }

                exports.Add(export);
            }

            var now = DateTime.UtcNow;
            int? mergedInvoiceId = dto.Merged_invoice_id;

            // 2. G?p hóa ??n tr??c (n?u c?n)
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
                        Note = dto.Note ?? "G?p theo phi?u xu?t kho"
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

            // 3. T?o phi?u xu?t kho g?p m?i
            var mergedExport = new Warehouse_export
            {
                Warehouse_export_code = $"EXP-M-{now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}",
                Warehouse_id = dto.Warehouse_id,
                Invoice_id = mergedInvoiceId ?? exports.First().Invoice_id,
                Order_id = null, // Phi?u g?p không liên k?t tr?c ti?p v?i order
                Customer_id = customerId!.Value,
                Staff_id = dto.Staff_id,
                Export_date = now,
                Delivery_status = "Pending",
                Status = "Draft",
                Split_merge_note = dto.Note ?? $"G?p t? {exports.Count} phi?u: {string.Join(", ", exports.Select(e => e.Warehouse_export_code))}",
                Created_by = userId,
                Updated_by = userId,
                Created_at = now,
                Updated_at = now,
                Is_deleted = false,
                Warehouse_Export_Details = new List<Warehouse_export_detail>()
            };

            // 4. G?p t?t c? chi ti?t
            foreach (var export in exports)
            {
                foreach (var detail in export.Warehouse_Export_Details.Where(d => !d.Is_deleted))
                {
                    // Ki?m tra s?n ph?m ?ã có trong phi?u g?p ch?a
                    var existingDetail = mergedExport.Warehouse_Export_Details
                        .FirstOrDefault(d => d.Product_id == detail.Product_id && d.Unit_price == detail.Unit_price);

                    if (existingDetail != null)
                    {
                        // C?ng d?n s? l??ng
                        existingDetail.Quantity_shipped += detail.Quantity_shipped;
                        existingDetail.Total_price = existingDetail.Quantity_shipped * existingDetail.Unit_price;
                    }
                    else
                    {
                        // Thêm m?i
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

            // 5. C?p nh?t tr?ng thái các phi?u ?ã g?p
            foreach (var export in exports)
            {
                export.Status = "Merged";
                export.Merged_into_export_id = mergedExport.Id;
                export.Split_merge_note = $"?ã g?p vào phi?u {mergedExport.Warehouse_export_code}";
                export.Updated_by = userId;
                export.Updated_at = now;
                await _exportRepository.UpdateAsync(export);
            }

            result.Success = true;
            result.Message = $"?ã g?p thành công {exports.Count} phi?u xu?t kho";
            result.Merged_export = MapToDto(mergedExport);
            result.Merged_export_ids = dto.Export_ids;
            result.Merged_invoice_id = mergedInvoiceId;

            return result;
        }

        public async Task<bool> UndoSplitAsync(int parentExportId, int userId)
        {
            var parentExport = await _exportRepository.GetByIdAsync(parentExportId);
            if (parentExport == null || parentExport.Status != "Split")
            {
                return false;
            }

            // L?y các phi?u con
            var childExports = await _exportRepository.GetChildExportsAsync(parentExportId);
            
            // Ki?m tra không có phi?u nào ?ã giao hàng
            if (childExports.Any(c => c.Delivery_status == "Delivered"))
            {
                throw new Exception("Không th? hoàn tác vì có phi?u ?ã giao hàng");
            }

            var now = DateTime.UtcNow;

            // Khôi ph?c s? l??ng v? phi?u g?c
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

                // Xóa phi?u con
                childExport.Is_deleted = true;
                childExport.Updated_by = userId;
                childExport.Updated_at = now;
                await _exportRepository.UpdateAsync(childExport);
            }

            // C?p nh?t phi?u g?c
            parentExport.Status = "Draft";
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

            if (mergedExport.Delivery_status == "Delivered")
            {
                throw new Exception("Không th? hoàn tác phi?u ?ã giao hàng");
            }

            var now = DateTime.UtcNow;

            // L?y các phi?u ?ã g?p vào
            var allExports = await _exportRepository.GetAllAsync();
            var sourceExports = allExports.Where(e => e.Merged_into_export_id == mergedExportId).ToList();

            // Khôi ph?c tr?ng thái các phi?u g?c
            foreach (var export in sourceExports)
            {
                export.Status = "Draft";
                export.Merged_into_export_id = null;
                export.Split_merge_note = null;
                export.Updated_by = userId;
                export.Updated_at = now;
                await _exportRepository.UpdateAsync(export);
            }

            // Xóa phi?u g?p
            mergedExport.Is_deleted = true;
            mergedExport.Updated_by = userId;
            mergedExport.Updated_at = now;
            await _exportRepository.UpdateAsync(mergedExport);

            return true;
        }

        #endregion

        #region Ki?m tra

        public async Task<bool> HasExportForInvoiceAsync(int invoiceId)
        {
            var exports = await _exportRepository.GetByInvoiceIdAsync(invoiceId);
            return exports.Any(e => !e.Is_deleted && e.Status != "Cancelled");
        }

        public async Task<IEnumerable<WarehouseExportDto>> GetChildExportsAsync(int parentExportId)
        {
            var childExports = await _exportRepository.GetChildExportsAsync(parentExportId);
            return childExports.Select(MapToDto);
        }

        #endregion

        #region Mapping

        private static WarehouseExportDto MapToDto(Warehouse_export export)
        {
            return new WarehouseExportDto
            {
                Id = export.Id,
                Warehouse_export_code = export.Warehouse_export_code,
                Export_date = export.Export_date,
                Warehouse_id = export.Warehouse_id,
                Warehouse_name = export.Warehouse?.Warehouse_name ?? "",
                Invoice_id = export.Invoice_id,
                Invoice_code = export.Invoice?.Invoice_code ?? "",
                Order_id = export.Order_id,
                Order_code = export.Order?.Order_code,
                Customer_id = export.Customer_id,
                Customer_name = export.Customer?.Full_name ?? "",
                Staff_id = export.Staff_id,
                Staff_name = export.Staff?.Full_name ?? "",
                Carrier_name = export.Carrier_name,
                Tracking_number = export.Tracking_number,
                Delivery_status = export.Delivery_status,
                Status = export.Status,
                Parent_export_id = export.Parent_export_id,
                Parent_export_code = export.Parent_export?.Warehouse_export_code,
                Merged_into_export_id = export.Merged_into_export_id,
                Merged_into_export_code = export.Merged_into_export?.Warehouse_export_code,
                Split_merge_note = export.Split_merge_note,
                Total_amount = export.Warehouse_Export_Details.Where(d => !d.Is_deleted).Sum(d => d.Total_price),
                Total_quantity = export.Warehouse_Export_Details.Where(d => !d.Is_deleted).Sum(d => d.Quantity_shipped),
                Created_at = export.Created_at,
                Details = export.Warehouse_Export_Details
                    .Where(d => !d.Is_deleted)
                    .Select(d => new WarehouseExportDetailDto
                    {
                        Id = d.Id,
                        Warehouse_export_id = d.Warehouse_export_id,
                        Warehouse_id = d.Warehouse_id,
                        Warehouse_name = d.Warehouse?.Warehouse_name ?? "",
                        Product_id = d.Product_id,
                        Product_code = d.Product?.Product_code ?? "",
                        Product_name = d.Product?.Product_name ?? "",
                        Quantity_shipped = d.Quantity_shipped,
                        Unit_price = d.Unit_price,
                        Total_price = d.Total_price
                    }).ToList()
            };
        }

        #endregion
    }
}
