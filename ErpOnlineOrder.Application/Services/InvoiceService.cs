using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceService(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        #region CRUD cơ bản

        public async Task<InvoiceDto?> GetByIdAsync(int id)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(id);
            return invoice != null ? MapToDto(invoice) : null;
        }

        public async Task<IEnumerable<InvoiceDto>> GetAllAsync()
        {
            var invoices = await _invoiceRepository.GetAllAsync();
            return invoices.Select(MapToDto);
        }

        public async Task<IEnumerable<InvoiceDto>> GetByCustomerIdAsync(int customerId)
        {
            var invoices = await _invoiceRepository.GetByCustomerIdAsync(customerId);
            return invoices.Select(MapToDto);
        }

        #endregion

        #region Tách/Gộp hóa đơn

        public async Task<SplitInvoiceResultDto> SplitInvoiceAsync(SplitInvoiceDto dto, int userId)
        {
            var result = new SplitInvoiceResultDto();

            // 1. Lấy hóa đơn gốc
            var sourceInvoice = await _invoiceRepository.GetByIdAsync(dto.Source_invoice_id);
            if (sourceInvoice == null)
            {
                result.Success = false;
                result.Message = "Hóa đơn không tồn tại";
                return result;
            }

            // 2. Kiểm tra trạng thái
            if (sourceInvoice.Status == "Split" || sourceInvoice.Status == "Merged")
            {
                result.Success = false;
                result.Message = "Không thể tách hóa đơn đã được tách/gộp";
                return result;
            }

            var now = DateTime.UtcNow;
            var newInvoices = new List<Invoice>();

            // 3. Tạo các hóa đơn mới từ các phần tách
            foreach (var (part, index) in dto.Split_parts.Select((p, i) => (p, i)))
            {
                var newInvoice = new Invoice
                {
                    Invoice_code = $"{sourceInvoice.Invoice_code}-S{index + 1}",
                    Invoice_date = now,
                    Customer_id = sourceInvoice.Customer_id,
                    Staff_id = sourceInvoice.Staff_id,
                    Status = "Draft",
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
                        .FirstOrDefault(d => d.Id == item.Invoice_detail_id);
                    
                    if (sourceDetail == null) continue;

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
            sourceInvoice.Status = "Split";
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
            result.Original_invoice = MapToDto(sourceInvoice);
            result.New_invoices = newInvoices.Select(MapToDto).ToList();

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

            // 1. Lấy tất cả hóa đơn cần gộp
            var invoices = new List<Invoice>();
            foreach (var id in dto.Invoice_ids)
            {
                var invoice = await _invoiceRepository.GetByIdAsync(id);
                if (invoice == null)
                {
                    result.Success = false;
                    result.Message = $"Hóa đơn {id} không tồn tại";
                    return result;
                }

                if (invoice.Customer_id != dto.Customer_id)
                {
                    result.Success = false;
                    result.Message = "Tất cả hóa đơn phải cùng một khách hàng";
                    return result;
                }

                if (invoice.Status == "Merged" || invoice.Status == "Cancelled")
                {
                    result.Success = false;
                    result.Message = $"Hóa đơn {invoice.Invoice_code} không thể gộp (trạng thái: {invoice.Status})";
                    return result;
                }

                invoices.Add(invoice);
            }

            var now = DateTime.UtcNow;

            // 2. Tạo hóa đơn gộp mới
            var mergedInvoice = new Invoice
            {
                Invoice_code = $"INV-M-{now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}",
                Invoice_date = now,
                Customer_id = dto.Customer_id,
                Staff_id = dto.Staff_id,
                Status = "Draft",
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
                invoice.Status = "Merged";
                invoice.Merged_into_invoice_id = mergedInvoice.Id;
                invoice.Split_merge_note = $"Đã gộp vào hóa đơn {mergedInvoice.Invoice_code}";
                invoice.Updated_by = userId;
                invoice.Updated_at = now;
                await _invoiceRepository.UpdateAsync(invoice);
            }

            result.Success = true;
            result.Message = $"Đã gộp thành công {invoices.Count} hóa đơn";
            result.Merged_invoice = MapToDto(mergedInvoice);
            result.Merged_invoice_ids = dto.Invoice_ids;

            return result;
        }

        public async Task<bool> UndoSplitAsync(int parentInvoiceId, int userId)
        {
            var parentInvoice = await _invoiceRepository.GetByIdAsync(parentInvoiceId);
            if (parentInvoice == null || parentInvoice.Status != "Split")
            {
                return false;
            }

            // Lấy các hóa đơn con
            var childInvoices = await _invoiceRepository.GetChildInvoicesAsync(parentInvoiceId);
            
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
            parentInvoice.Status = "Draft";
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
            var allInvoices = await _invoiceRepository.GetAllAsync();
            var sourceInvoices = allInvoices.Where(i => i.Merged_into_invoice_id == mergedInvoiceId).ToList();

            // Khôi phục trạng thái các hóa đơn gốc
            foreach (var invoice in sourceInvoices)
            {
                invoice.Status = "Draft";
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

        #endregion

        #region Mapping

        private static InvoiceDto MapToDto(Invoice invoice)
        {
            return new InvoiceDto
            {
                Id = invoice.Id,
                Invoice_code = invoice.Invoice_code,
                Invoice_date = invoice.Invoice_date,
                Customer_id = invoice.Customer_id,
                Customer_name = invoice.Customer?.Full_name ?? "",
                Total_amount = invoice.Total_amount,
                Tax_amount = invoice.Tax_amount,
                Status = invoice.Status,
                Parent_invoice_id = invoice.Parent_invoice_id,
                Parent_invoice_code = invoice.Parent_invoice?.Invoice_code,
                Details = invoice.Invoice_Details
                    .Where(d => !d.Is_deleted)
                    .Select(d => new InvoiceDetailDto
                    {
                        Id = d.Id,
                        Product_id = d.Product_id,
                        Product_name = d.Product?.Product_name ?? "",
                        Quantity = d.Quantity,
                        Unit_price = d.Unit_price,
                        Total_price = d.Total_price,
                        Tax_rate = d.Tax_rate
                    }).ToList()
            };
        }

        #endregion
    }
}