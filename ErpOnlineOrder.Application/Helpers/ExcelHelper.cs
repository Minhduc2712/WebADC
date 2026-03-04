namespace ErpOnlineOrder.Application.Helpers;

/// <summary>
/// Helper để làm sạch dữ liệu trước khi ghi vào Excel (OOXML).
/// Các ký tự điều khiển (0x00-0x1F trừ tab, newline, carriage return) không hợp lệ trong XML
/// và gây lỗi "file corrupt" khi mở Excel.
/// </summary>
public static class ExcelHelper
{
    /// <summary>
    /// Loại bỏ các ký tự không hợp lệ trong XML để tránh lỗi định dạng file Excel.
    /// </summary>
    public static string SanitizeForExcel(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        // Các ký tự hợp lệ trong XML 1.0: tab (0x09), newline (0x0A), carriage return (0x0D)
        // Các ký tự 0x00-0x08, 0x0B, 0x0C, 0x0E-0x1F là không hợp lệ
        var sb = new System.Text.StringBuilder(value.Length);
        foreach (var c in value)
        {
            if (IsValidXmlChar(c))
                sb.Append(c);
            else
                sb.Append(' '); // Thay bằng khoảng trắng
        }
        return sb.ToString();
    }

    /// <summary>
    /// Gán giá trị đã làm sạch vào ô Excel (hỗ trợ string, number, DateTime).
    /// </summary>
    public static void SetCellValue(ClosedXML.Excel.IXLCell cell, object? value)
    {
        if (value == null)
        {
            cell.Value = string.Empty;
            return;
        }

        switch (value)
        {
            case string s:
                cell.Value = SanitizeForExcel(s);
                break;
            case DateTime dt:
                cell.Value = dt;
                break;
            case decimal d:
                cell.Value = d;
                break;
            case double dbl:
                cell.Value = dbl;
                break;
            case float f:
                cell.Value = f;
                break;
            case int i:
                cell.Value = i;
                break;
            case long l:
                cell.Value = l;
                break;
            default:
                cell.Value = SanitizeForExcel(value.ToString());
                break;
        }
    }

    private static bool IsValidXmlChar(char c)
    {
        return c == 0x09 || c == 0x0A || c == 0x0D
            || (c >= 0x20 && c <= 0xD7FF)
            || (c >= 0xE000 && c <= 0xFFFD)
            || (c >= 0x10000 && c <= 0x10FFFF);
    }
}
