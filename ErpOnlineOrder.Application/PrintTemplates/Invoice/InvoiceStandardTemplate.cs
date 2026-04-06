using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;
using System.Xml.Linq;
using QuestPdfDocument = QuestPDF.Fluent.Document;

namespace ErpOnlineOrder.Application.PrintTemplates.Invoice
{

    public class InvoiceStandardTemplate : IPrintTemplate<InvoiceDto>
    {
        public string Name => "standard";
        public string DisplayName => "Mẫu chuẩn (đầy đủ)";
        public string DocumentType => "invoice";

        // ── PDF ──────────────────────────────────────────────────────────────
        public byte[] ToPdf(InvoiceDto data, PrintTemplateSettings settings)
        {
            return QuestPdfDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Content().Column(col =>
                    {
                        // -- Header công ty
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(settings.CompanyName).Bold().FontSize(13);
                                if (!string.IsNullOrEmpty(settings.CompanyAddress))
                                    c.Item().Text(settings.CompanyAddress).FontSize(9);
                                if (!string.IsNullOrEmpty(settings.CompanyPhone))
                                    c.Item().Text($"ĐT: {settings.CompanyPhone}").FontSize(9);
                            });
                        });

                        col.Item().PaddingVertical(8).LineHorizontal(0.5f);

                        // -- Tiêu đề
                        col.Item().AlignCenter().Text("HÓA ĐƠN BÁN HÀNG").Bold().FontSize(18);
                        col.Item().AlignCenter().Text($"Số: {data.Invoice_code}").Bold().FontSize(12);
                        col.Item().AlignCenter().Text(
                            $"Ngày {data.Invoice_date:dd} tháng {data.Invoice_date:MM} năm {data.Invoice_date:yyyy}")
                            .FontSize(10).Italic();

                        col.Item().PaddingTop(10).Text($"Khách hàng: {data.Customer_name}").SemiBold();
                        if (!string.IsNullOrEmpty(data.Province_name))
                            col.Item().Text($"Tỉnh/Thành phố: {data.Province_name}");
                        if (data.Parent_invoice_code != null)
                            col.Item().Text($"Tách từ hóa đơn: {data.Parent_invoice_code}").FontSize(9).Italic();

                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(28);
                                cols.RelativeColumn(4);
                                cols.ConstantColumn(45);
                                cols.ConstantColumn(88);
                                cols.ConstantColumn(52);
                                cols.ConstantColumn(90);
                            });

                            table.Header(h =>
                            {
                                var style = TextStyle.Default.Bold().FontSize(9);
                                h.Cell().Background("#e8e8e8").Padding(3).Text("STT").Style(style).AlignCenter();
                                h.Cell().Background("#e8e8e8").Padding(3).Text("Tên sản phẩm").Style(style);
                                h.Cell().Background("#e8e8e8").Padding(3).Text("SL").Style(style).AlignCenter();
                                h.Cell().Background("#e8e8e8").Padding(3).Text("Đơn giá").Style(style).AlignRight();
                                h.Cell().Background("#e8e8e8").Padding(3).Text("Thuế (%)").Style(style).AlignCenter();
                                h.Cell().Background("#e8e8e8").Padding(3).Text("Thành tiền").Style(style).AlignRight();
                            });

                            var stt = 1;
                            foreach (var d in data.Details)
                            {
                                var bg = stt % 2 == 0 ? "#f9f9f9" : "#ffffff";
                                table.Cell().Background(bg).BorderBottom(0.3f).Padding(3).Text(stt.ToString()).AlignCenter();
                                table.Cell().Background(bg).BorderBottom(0.3f).Padding(3).Text(d.Product_name);
                                table.Cell().Background(bg).BorderBottom(0.3f).Padding(3).Text(d.Quantity.ToString()).AlignCenter();
                                table.Cell().Background(bg).BorderBottom(0.3f).Padding(3).Text(d.Unit_price.ToString("N0")).AlignRight();
                                table.Cell().Background(bg).BorderBottom(0.3f).Padding(3).Text(d.Tax_rate.ToString("N1")).AlignCenter();
                                table.Cell().Background(bg).BorderBottom(0.3f).Padding(3).Text(d.Total_price.ToString("N0")).AlignRight();
                                stt++;
                            }
                        });

                        // -- Tổng tiền
                        col.Item().PaddingTop(6).AlignRight().Text($"Tổng tiền hàng: {data.Total_amount:N0} VNĐ");
                        col.Item().AlignRight().Text($"Tiền thuế: {data.Tax_amount:N0} VNĐ");
                        col.Item().AlignRight().Text($"Tổng thanh toán: {data.Grand_total:N0} VNĐ").Bold().FontSize(12);

                        // -- Ký tên
                        col.Item().PaddingTop(36).Row(row =>
                        {
                            row.RelativeItem().AlignCenter().Column(c =>
                            {
                                c.Item().Text("Người mua hàng").Bold();
                                c.Item().Text("(Ký, ghi rõ họ tên)").Italic().FontSize(9);
                                c.Item().PaddingTop(48).Text("").FontSize(10);
                            });
                            row.RelativeItem().AlignCenter().Column(c =>
                            {
                                c.Item().Text("Người bán hàng").Bold();
                                c.Item().Text("(Ký, đóng dấu, ghi rõ họ tên)").Italic().FontSize(9);
                                c.Item().PaddingTop(48).Text("").FontSize(10);
                            });
                        });

                        col.Item().PaddingTop(10).AlignRight()
                            .Text($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm}").Italic().FontSize(8);
                    });
                });
            }).GeneratePdf();
        }

        // ── DOCX ─────────────────────────────────────────────────────────────
        public byte[] ToDocx(InvoiceDto data, PrintTemplateSettings settings)
        {
            using var mem = new MemoryStream();
            using (var doc = WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document, true))
            {
                var mainPart = doc.AddMainDocumentPart();
                var body = new Body();

                // Tiêu đề
                body.Append(DocxHelper.CenteredBoldParagraph(settings.CompanyName, 14));
                body.Append(DocxHelper.CenteredBoldParagraph("HÓA ĐƠN BÁN HÀNG", 18));
                body.Append(DocxHelper.CenteredParagraph($"Số: {data.Invoice_code}", 12, bold: true));
                body.Append(DocxHelper.CenteredParagraph(
                    $"Ngày {data.Invoice_date:dd} tháng {data.Invoice_date:MM} năm {data.Invoice_date:yyyy}", 11, italic: true));
                body.Append(DocxHelper.EmptyParagraph());

                // Thông tin khách hàng
                body.Append(DocxHelper.LabelValueParagraph("Khách hàng:", data.Customer_name));
                if (!string.IsNullOrEmpty(data.Province_name))
                    body.Append(DocxHelper.LabelValueParagraph("Tỉnh/Thành phố:", data.Province_name));
                body.Append(DocxHelper.EmptyParagraph());

                // Bảng chi tiết
                var table = DocxHelper.CreateTable(new[] { "STT", "Tên sản phẩm", "Số lượng", "Đơn giá", "Thuế (%)", "Thành tiền" });
                var stt = 1;
                foreach (var d in data.Details)
                {
                    DocxHelper.AddTableRow(table, new[]
                    {
                        stt.ToString(),
                        d.Product_name,
                        d.Quantity.ToString(),
                        d.Unit_price.ToString("N0"),
                        d.Tax_rate.ToString("N1"),
                        d.Total_price.ToString("N0")
                    });
                    stt++;
                }
                body.Append(table);
                body.Append(DocxHelper.EmptyParagraph());

                // Tổng tiền
                body.Append(DocxHelper.RightAlignParagraph($"Tổng tiền hàng: {data.Total_amount:N0} VNĐ"));
                body.Append(DocxHelper.RightAlignParagraph($"Tiền thuế: {data.Tax_amount:N0} VNĐ"));
                body.Append(DocxHelper.RightAlignParagraph($"Tổng thanh toán: {data.Grand_total:N0} VNĐ", bold: true));
                body.Append(DocxHelper.EmptyParagraph());
                body.Append(DocxHelper.RightAlignParagraph($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm}", italic: true, halfPtFontSize: 18));

                mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(body);
                mainPart.Document.Save();
            }
            return mem.ToArray();
        }

        // ── XML ──────────────────────────────────────────────────────────────
        public byte[] ToXml(InvoiceDto data, PrintTemplateSettings settings)
        {
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("HoaDon",
                    new XAttribute("MauBan", DisplayName),
                    new XElement("ThongTinCongTy",
                        new XElement("Ten", settings.CompanyName),
                        new XElement("DiaChi", settings.CompanyAddress),
                        new XElement("DienThoai", settings.CompanyPhone)
                    ),
                    new XElement("MaHoaDon", data.Invoice_code),
                    new XElement("NgayHoaDon", data.Invoice_date.ToString("yyyy-MM-dd")),
                    new XElement("KhachHang", data.Customer_name),
                    new XElement("TinhThanh", data.Province_name ?? ""),
                    new XElement("TrangThai", data.Status),
                    new XElement("TongTienHang", data.Total_amount),
                    new XElement("TienThue", data.Tax_amount),
                    new XElement("TongThanhToan", data.Grand_total),
                    new XElement("ChiTiet",
                        data.Details.Select((d, i) => new XElement("SanPham",
                            new XAttribute("stt", i + 1),
                            new XElement("Ten", d.Product_name),
                            new XElement("SoLuong", d.Quantity),
                            new XElement("DonGia", d.Unit_price),
                            new XElement("ThueXuat", d.Tax_rate),
                            new XElement("ThanhTien", d.Total_price)
                        ))
                    ),
                    new XElement("NgayIn", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"))
                )
            );
            return Encoding.UTF8.GetBytes(doc.ToString());
        }
    }
}
