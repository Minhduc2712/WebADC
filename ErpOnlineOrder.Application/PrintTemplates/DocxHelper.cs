using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ErpOnlineOrder.Application.PrintTemplates
{

    internal static class DocxHelper
    {
        // ── Paragraph helpers ─────────────────────────────────────────────

        public static Paragraph CenteredBoldParagraph(string text, int halfPtFontSize = 24)
        {
            return new Paragraph(
                new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
                new Run(
                    new RunProperties(new Bold(), new FontSize { Val = halfPtFontSize.ToString() }),
                    new Text(text)
                )
            );
        }

        public static Paragraph CenteredParagraph(string text, int halfPtFontSize = 22, bool bold = false, bool italic = false)
        {
            var rp = new RunProperties(new FontSize { Val = halfPtFontSize.ToString() });
            if (bold) rp.AppendChild(new Bold());
            if (italic) rp.AppendChild(new Italic());
            return new Paragraph(
                new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
                new Run(rp, new Text(text))
            );
        }

        public static Paragraph LabelValueParagraph(string label, string value)
        {
            return new Paragraph(
                new Run(new RunProperties(new Bold()), new Text(label) { Space = SpaceProcessingModeValues.Preserve }),
                new Run(new Text(" " + value) { Space = SpaceProcessingModeValues.Preserve })
            );
        }

        public static Paragraph RightAlignParagraph(string text, bool bold = false, bool italic = false, int halfPtFontSize = 22)
        {
            var rp = new RunProperties(new FontSize { Val = halfPtFontSize.ToString() });
            if (bold) rp.AppendChild(new Bold());
            if (italic) rp.AppendChild(new Italic());
            return new Paragraph(
                new ParagraphProperties(new Justification { Val = JustificationValues.Right }),
                new Run(rp, new Text(text))
            );
        }

        public static Paragraph EmptyParagraph() => new Paragraph();

        public static Paragraph SimpleParagraph(string text, bool bold = false)
        {
            var rp = new RunProperties();
            if (bold) rp.AppendChild(new Bold());
            return new Paragraph(new Run(rp, new Text(text)));
        }

        // ── Table helpers ─────────────────────────────────────────────────

        public static Table CreateTable(string[] headers)
        {
            var table = new Table();

            // Border style
            var tblPr = new TableProperties(
                new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 4 },
                    new BottomBorder { Val = BorderValues.Single, Size = 4 },
                    new LeftBorder { Val = BorderValues.Single, Size = 4 },
                    new RightBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }
                ),
                new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct }
            );
            table.AppendChild(tblPr);

            // Header row
            var headerRow = new TableRow();
            foreach (var h in headers)
            {
                headerRow.AppendChild(new TableCell(
                    new TableCellProperties(new Shading { Fill = "CCCCCC", Val = ShadingPatternValues.Clear }),
                    new Paragraph(new Run(new RunProperties(new Bold()), new Text(h)))
                ));
            }
            table.AppendChild(headerRow);
            return table;
        }

        public static void AddTableRow(Table table, string[] values)
        {
            var row = new TableRow();
            foreach (var v in values)
                row.AppendChild(new TableCell(new Paragraph(new Run(new Text(v)))));
            table.AppendChild(row);
        }
    }
}
