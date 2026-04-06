using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ErpOnlineOrder.Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;
using System.Xml.Linq;
using QuestPdfDocument = QuestPDF.Fluent.Document;

namespace ErpOnlineOrder.Application.PrintTemplates.Order
{
    /// <summary>
    /// Mẫu xác nhận đơn hàng chuẩn: thông tin đơn, khách hàng, bảng sản phẩm, tổng tiền.
    /// </summary>
    public class OrderStandardTemplate : IPrintTemplate<OrderDTO>
    {
        public string Name => "standard";
        public string DisplayName => "Mẫu xác nhận đơn hàng";
        public string DocumentType => "order";

        // ── PDF ──────────────────────────────────────────────────────────────
        public byte[] ToPdf(OrderDTO data, PrintTemplateSettings settings)
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

                        col.Item().AlignCenter().Text("XÁC NHẬN ĐƠN HÀNG").Bold().FontSize(18);
                        col.Item().AlignCenter().Text($"Số: {data.Order_code}").Bold().FontSize(12);
                        col.Item().AlignCenter().Text(
                            $"Ngày {data.Order_date:dd} tháng {data.Order_date:MM} năm {data.Order_date:yyyy}")
                            .Italic().FontSize(10);

                        col.Item().PaddingTop(10).Text($"Khách hàng: {data.Customer_name}").SemiBold();
                        if (!string.IsNullOrEmpty(data.Shipping_address))
                            col.Item().Text($"Địa chỉ giao hàng: {data.Shipping_address}");
                        if (!string.IsNullOrEmpty(data.note))
                            col.Item().Text($"Ghi chú: {data.note}").Italic().FontSize(9);

                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(28);
                                cols.RelativeColumn(4);
                                cols.ConstantColumn(45);
                                cols.ConstantColumn(88);
                                cols.ConstantColumn(100);
                            });

                            table.Header(h =>
                            {
                                var style = TextStyle.Default.Bold().FontSize(9);
                                h.Cell().Background("#e8e8e8").Padding(3).Text("STT").Style(style).AlignCenter();
                                h.Cell().Background("#e8e8e8").Padding(3).Text("Tên sản phẩm").Style(style);
                                h.Cell().Background("#e8e8e8").Padding(3).Text("SL").Style(style).AlignCenter();
                                h.Cell().Background("#e8e8e8").Padding(3).Text("Đơn giá").Style(style).AlignRight();
                                h.Cell().Background("#e8e8e8").Padding(3).Text("Thành tiền").Style(style).AlignRight();
                            });

                            var stt = 1;
                            foreach (var d in data.Order_details)
                            {
                                var bg = stt % 2 == 0 ? "#f9f9f9" : "#ffffff";
                                table.Cell().Background(bg).BorderBottom(0.3f).Padding(3).Text(stt.ToString()).AlignCenter();
                                table.Cell().Background(bg).BorderBottom(0.3f).Padding(3).Text(d.Product_name);
                                table.Cell().Background(bg).BorderBottom(0.3f).Padding(3).Text(d.Quantity.ToString()).AlignCenter();
                                table.Cell().Background(bg).BorderBottom(0.3f).Padding(3).Text(d.Unit_price.ToString("N0")).AlignRight();
                                table.Cell().Background(bg).BorderBottom(0.3f).Padding(3).Text(d.Total_price.ToString("N0")).AlignRight();
                                stt++;
                            }
                        });

                        col.Item().PaddingTop(6).AlignRight()
                            .Text($"Tổng tiền: {data.Total_price:N0} VNĐ").Bold().FontSize(12);

                        col.Item().PaddingTop(36).Row(row =>
                        {
                            row.RelativeItem().AlignCenter().Column(c =>
                            {
                                c.Item().Text("Khách hàng").Bold();
                                c.Item().Text("(Ký, ghi rõ họ tên)").Italic().FontSize(9);
                                c.Item().PaddingTop(48).Text("").FontSize(10);
                            });
                            row.RelativeItem().AlignCenter().Column(c =>
                            {
                                c.Item().Text("Người xác nhận").Bold();
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
        public byte[] ToDocx(OrderDTO data, PrintTemplateSettings settings)
        {
            using var mem = new MemoryStream();
            using (var doc = WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document, true))
            {
                var mainPart = doc.AddMainDocumentPart();
                var body = new Body();

                body.Append(DocxHelper.CenteredBoldParagraph(settings.CompanyName, 14));
                body.Append(DocxHelper.CenteredBoldParagraph("XÁC NHẬN ĐƠN HÀNG", 18));
                body.Append(DocxHelper.CenteredParagraph($"Số: {data.Order_code}", 12, bold: true));
                body.Append(DocxHelper.CenteredParagraph(
                    $"Ngày {data.Order_date:dd} tháng {data.Order_date:MM} năm {data.Order_date:yyyy}", 11, italic: true));
                body.Append(DocxHelper.EmptyParagraph());

                body.Append(DocxHelper.LabelValueParagraph("Khách hàng:", data.Customer_name));
                if (!string.IsNullOrEmpty(data.Shipping_address))
                    body.Append(DocxHelper.LabelValueParagraph("Địa chỉ giao:", data.Shipping_address));
                body.Append(DocxHelper.EmptyParagraph());

                var table = DocxHelper.CreateTable(new[] { "STT", "Tên sản phẩm", "Số lượng", "Đơn giá", "Thành tiền" });
                var stt = 1;
                foreach (var d in data.Order_details)
                {
                    DocxHelper.AddTableRow(table, new[]
                    {
                        stt.ToString(),
                        d.Product_name,
                        d.Quantity.ToString(),
                        d.Unit_price.ToString("N0"),
                        d.Total_price.ToString("N0")
                    });
                    stt++;
                }
                body.Append(table);
                body.Append(DocxHelper.EmptyParagraph());
                body.Append(DocxHelper.RightAlignParagraph($"Tổng tiền: {data.Total_price:N0} VNĐ", bold: true));
                body.Append(DocxHelper.RightAlignParagraph($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm}", italic: true, halfPtFontSize: 18));

                mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(body);
                mainPart.Document.Save();
            }
            return mem.ToArray();
        }

        // ── XML ──────────────────────────────────────────────────────────────
        public byte[] ToXml(OrderDTO data, PrintTemplateSettings settings)
        {
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("DonHang",
                    new XElement("MaDonHang", data.Order_code),
                    new XElement("NgayDat", data.Order_date.ToString("yyyy-MM-dd")),
                    new XElement("KhachHang", data.Customer_name),
                    new XElement("DiaChiGiao", data.Shipping_address ?? ""),
                    new XElement("GhiChu", data.note ?? ""),
                    new XElement("TrangThai", data.Order_status),
                    new XElement("TongTien", data.Total_price),
                    new XElement("ChiTiet",
                        data.Order_details.Select((d, i) => new XElement("SanPham",
                            new XAttribute("stt", i + 1),
                            new XElement("Ten", d.Product_name),
                            new XElement("SoLuong", d.Quantity),
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
