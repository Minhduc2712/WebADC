using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;
using System.Xml.Linq;
using QuestPdfDocument = QuestPDF.Fluent.Document;

namespace ErpOnlineOrder.Application.PrintTemplates.Invoice
{

    public class InvoiceSimplifiedTemplate : IPrintTemplate<InvoiceDto>
    {
        public string Name => "simplified";
        public string DisplayName => "Mẫu rút gọn";
        public string DocumentType => "invoice";

        // ── PDF ──────────────────────────────────────────────────────────────
        public byte[] ToPdf(InvoiceDto data, PrintTemplateSettings settings)
        {
            return QuestPdfDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(1.2f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Content().Column(col =>
                    {
                        col.Item().AlignCenter().Text(settings.CompanyName).Bold().FontSize(13);
                        col.Item().PaddingBottom(4).LineHorizontal(0.5f);

                        col.Item().AlignCenter().Text("HÓA ĐƠN").Bold().FontSize(16);
                        col.Item().AlignCenter().Text($"{data.Invoice_code}  |  {data.Invoice_date:dd/MM/yyyy}").FontSize(10);
                        col.Item().PaddingTop(6).Text($"Khách hàng: {data.Customer_name}").SemiBold();

                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(4);
                                cols.ConstantColumn(40);
                                cols.ConstantColumn(85);
                                cols.ConstantColumn(85);
                            });

                            table.Header(h =>
                            {
                                var style = TextStyle.Default.Bold().FontSize(9);
                                h.Cell().Background("#dddddd").Padding(3).Text("Sản phẩm").Style(style);
                                h.Cell().Background("#dddddd").Padding(3).Text("SL").Style(style).AlignCenter();
                                h.Cell().Background("#dddddd").Padding(3).Text("Đơn giá").Style(style).AlignRight();
                                h.Cell().Background("#dddddd").Padding(3).Text("Thành tiền").Style(style).AlignRight();
                            });

                            foreach (var d in data.Details)
                            {
                                table.Cell().BorderBottom(0.3f).Padding(3).Text(d.Product_name);
                                table.Cell().BorderBottom(0.3f).Padding(3).Text(d.Quantity.ToString()).AlignCenter();
                                table.Cell().BorderBottom(0.3f).Padding(3).Text(d.Unit_price.ToString("N0")).AlignRight();
                                table.Cell().BorderBottom(0.3f).Padding(3).Text(d.Total_price.ToString("N0")).AlignRight();
                            }
                        });

                        col.Item().PaddingTop(6).AlignRight()
                            .Text($"Tổng thanh toán: {data.Grand_total:N0} VNĐ").Bold().FontSize(12);

                        col.Item().PaddingTop(8).LineHorizontal(0.5f);
                        col.Item().AlignRight()
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

                body.Append(DocxHelper.CenteredBoldParagraph(settings.CompanyName, 14));
                body.Append(DocxHelper.CenteredBoldParagraph("HÓA ĐƠN", 18));
                body.Append(DocxHelper.CenteredParagraph($"{data.Invoice_code}  |  {data.Invoice_date:dd/MM/yyyy}", 11));
                body.Append(DocxHelper.EmptyParagraph());
                body.Append(DocxHelper.LabelValueParagraph("Khách hàng:", data.Customer_name));
                body.Append(DocxHelper.EmptyParagraph());

                var table = DocxHelper.CreateTable(new[] { "Sản phẩm", "Số lượng", "Đơn giá", "Thành tiền" });
                foreach (var d in data.Details)
                {
                    DocxHelper.AddTableRow(table, new[]
                    {
                        d.Product_name,
                        d.Quantity.ToString(),
                        d.Unit_price.ToString("N0"),
                        d.Total_price.ToString("N0")
                    });
                }
                body.Append(table);
                body.Append(DocxHelper.EmptyParagraph());
                body.Append(DocxHelper.RightAlignParagraph($"Tổng thanh toán: {data.Grand_total:N0} VNĐ", bold: true));
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
                    new XElement("MaHoaDon", data.Invoice_code),
                    new XElement("NgayHoaDon", data.Invoice_date.ToString("yyyy-MM-dd")),
                    new XElement("KhachHang", data.Customer_name),
                    new XElement("TongThanhToan", data.Grand_total),
                    new XElement("ChiTiet",
                        data.Details.Select((d, i) => new XElement("SanPham",
                            new XAttribute("stt", i + 1),
                            new XElement("Ten", d.Product_name),
                            new XElement("SoLuong", d.Quantity),
                            new XElement("DonGia", d.Unit_price),
                            new XElement("ThanhTien", d.Total_price)
                        ))
                    )
                )
            );
            return Encoding.UTF8.GetBytes(doc.ToString());
        }
    }
}
