using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;
using System.Xml.Linq;
using QuestPdfDocument = QuestPDF.Fluent.Document;

namespace ErpOnlineOrder.Application.PrintTemplates.WarehouseExport
{

    public class ExportStandardTemplate : IPrintTemplate<ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs.WarehouseExportDto>
    {
        public string Name => "standard";
        public string DisplayName => "Mẫu chuẩn";
        public string DocumentType => "warehouse_export";

        // ── PDF ──────────────────────────────────────────────────────────────
        public byte[] ToPdf(WarehouseExportDto data, PrintTemplateSettings settings)
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
                        // Header công ty
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

                        col.Item().PaddingVertical(6).LineHorizontal(0.5f);

                        // Tiêu đề
                        col.Item().AlignCenter().Text("PHIẾU XUẤT KHO").Bold().FontSize(18);
                        col.Item().AlignCenter().Text($"Số: {data.Warehouse_export_code}").Bold().FontSize(12);
                        col.Item().AlignCenter().Text(
                            $"Ngày {data.Export_date:dd} tháng {data.Export_date:MM} năm {data.Export_date:yyyy}")
                            .Italic().FontSize(10);

                        // Thông tin
                        col.Item().PaddingTop(10).Text($"Khách hàng: {data.Customer_name}").SemiBold();
                        col.Item().Text($"Kho xuất: {data.Warehouse_name}");
                        if (!string.IsNullOrEmpty(data.Invoice_code))
                            col.Item().Text($"Hóa đơn liên quan: {data.Invoice_code}");
                        if (!string.IsNullOrEmpty(data.Order_code))
                            col.Item().Text($"Đơn hàng: {data.Order_code}");
                        if (data.Arrival_date.HasValue)
                            col.Item().Text($"Ngày vận chuyển đến: {data.Arrival_date.Value:dd/MM/yyyy}");

                        // Bảng hàng hóa
                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(28);
                                cols.ConstantColumn(65);
                                cols.RelativeColumn(4);
                                cols.ConstantColumn(45);
                                cols.ConstantColumn(88);
                                cols.ConstantColumn(90);
                            });

                            table.Header(h =>
                            {
                                var style = TextStyle.Default.Bold().FontSize(9);
                                h.Cell().Background("#e8e8e8").Padding(3).Text("STT").Style(style).AlignCenter();
                                h.Cell().Background("#e8e8e8").Padding(3).Text("Mã SP").Style(style);
                                h.Cell().Background("#e8e8e8").Padding(3).Text("Tên sản phẩm").Style(style);
                                h.Cell().Background("#e8e8e8").Padding(3).Text("SL").Style(style).AlignCenter();
                                h.Cell().Background("#e8e8e8").Padding(3).Text("Đơn giá").Style(style).AlignRight();
                                h.Cell().Background("#e8e8e8").Padding(3).Text("Thành tiền").Style(style).AlignRight();
                            });

                            var stt = 1;
                            foreach (var d in data.Details)
                            {
                                var bg = stt % 2 == 0 ? "#f9f9f9" : "#ffffff";
                                table.Cell().Background(bg).BorderBottom(0.3f).Padding(3).Text(stt.ToString()).AlignCenter();
                                table.Cell().Background(bg).BorderBottom(0.3f).Padding(3).Text(d.Product_code);
                                table.Cell().Background(bg).BorderBottom(0.3f).Padding(3).Text(d.Product_name);
                                table.Cell().Background(bg).BorderBottom(0.3f).Padding(3).Text(d.Quantity_shipped.ToString()).AlignCenter();
                                table.Cell().Background(bg).BorderBottom(0.3f).Padding(3).Text(d.Unit_price.ToString("N0")).AlignRight();
                                table.Cell().Background(bg).BorderBottom(0.3f).Padding(3).Text(d.Total_price.ToString("N0")).AlignRight();
                                stt++;
                            }
                        });

                        // Tổng
                        col.Item().PaddingTop(6).AlignRight()
                            .Text($"Tổng số lượng: {data.Total_quantity}  |  Tổng tiền: {data.Total_amount:N0} VNĐ").Bold();

                        // Ký tên
                        col.Item().PaddingTop(36).Row(row =>
                        {
                            row.RelativeItem().AlignCenter().Column(c =>
                            {
                                c.Item().Text("Thủ kho").Bold();
                                c.Item().Text("(Ký, ghi rõ họ tên)").Italic().FontSize(9);
                                c.Item().PaddingTop(48).Text("").FontSize(10);
                            });
                            row.RelativeItem().AlignCenter().Column(c =>
                            {
                                c.Item().Text("Người nhận hàng").Bold();
                                c.Item().Text("(Ký, ghi rõ họ tên)").Italic().FontSize(9);
                                c.Item().PaddingTop(48).Text("").FontSize(10);
                            });
                            row.RelativeItem().AlignCenter().Column(c =>
                            {
                                c.Item().Text("Phụ trách").Bold();
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
        public byte[] ToDocx(WarehouseExportDto data, PrintTemplateSettings settings)
        {
            using var mem = new MemoryStream();
            using (var doc = WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document, true))
            {
                var mainPart = doc.AddMainDocumentPart();
                var body = new Body();

                body.Append(DocxHelper.CenteredBoldParagraph(settings.CompanyName, 14));
                body.Append(DocxHelper.CenteredBoldParagraph("PHIẾU XUẤT KHO", 18));
                body.Append(DocxHelper.CenteredParagraph($"Số: {data.Warehouse_export_code}", 12, bold: true));
                body.Append(DocxHelper.CenteredParagraph(
                    $"Ngày {data.Export_date:dd} tháng {data.Export_date:MM} năm {data.Export_date:yyyy}", 11, italic: true));
                body.Append(DocxHelper.EmptyParagraph());

                body.Append(DocxHelper.LabelValueParagraph("Khách hàng:", data.Customer_name));
                body.Append(DocxHelper.LabelValueParagraph("Kho xuất:", data.Warehouse_name));
                if (!string.IsNullOrEmpty(data.Invoice_code))
                    body.Append(DocxHelper.LabelValueParagraph("Hóa đơn liên quan:", data.Invoice_code));
                if (!string.IsNullOrEmpty(data.Order_code))
                    body.Append(DocxHelper.LabelValueParagraph("Đơn hàng:", data.Order_code));
                body.Append(DocxHelper.EmptyParagraph());

                var table = DocxHelper.CreateTable(new[] { "STT", "Mã SP", "Tên sản phẩm", "Số lượng", "Đơn giá", "Thành tiền" });
                var stt = 1;
                foreach (var d in data.Details)
                {
                    DocxHelper.AddTableRow(table, new[]
                    {
                        stt.ToString(),
                        d.Product_code,
                        d.Product_name,
                        d.Quantity_shipped.ToString(),
                        d.Unit_price.ToString("N0"),
                        d.Total_price.ToString("N0")
                    });
                    stt++;
                }
                body.Append(table);
                body.Append(DocxHelper.EmptyParagraph());
                body.Append(DocxHelper.RightAlignParagraph(
                    $"Tổng số lượng: {data.Total_quantity}  |  Tổng tiền: {data.Total_amount:N0} VNĐ", bold: true));
                body.Append(DocxHelper.RightAlignParagraph($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm}", italic: true, halfPtFontSize: 18));

                mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(body);
                mainPart.Document.Save();
            }
            return mem.ToArray();
        }

        // ── XML ──────────────────────────────────────────────────────────────
        public byte[] ToXml(WarehouseExportDto data, PrintTemplateSettings settings)
        {
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("PhieuXuatKho",
                    new XElement("MaPhieu", data.Warehouse_export_code),
                    new XElement("NgayXuat", data.Export_date.ToString("yyyy-MM-dd")),
                    new XElement("KhachHang", data.Customer_name),
                    new XElement("KhoXuat", data.Warehouse_name),
                    new XElement("HoaDon", data.Invoice_code ?? ""),
                    new XElement("DonHang", data.Order_code ?? ""),
                    new XElement("TrangThai", data.Status),
                    new XElement("TongSoLuong", data.Total_quantity),
                    new XElement("TongTien", data.Total_amount),
                    new XElement("ChiTiet",
                        data.Details.Select((d, i) => new XElement("SanPham",
                            new XAttribute("stt", i + 1),
                            new XElement("Ma", d.Product_code),
                            new XElement("Ten", d.Product_name),
                            new XElement("SoLuong", d.Quantity_shipped),
                            new XElement("DonGia", d.Unit_price),
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
