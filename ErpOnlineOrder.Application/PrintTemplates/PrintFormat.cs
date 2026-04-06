namespace ErpOnlineOrder.Application.PrintTemplates
{
    public static class PrintFormat
    {
        public const string Pdf = "pdf";
        public const string Docx = "docx";
        public const string Xml = "xml";

        public static string GetContentType(string format) => format.ToLowerInvariant() switch
        {
            Pdf => "application/pdf",
            Docx => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            Xml => "application/xml",
            _ => "application/octet-stream"
        };

        public static string GetExtension(string format) => format.ToLowerInvariant() switch
        {
            Pdf => ".pdf",
            Docx => ".docx",
            Xml => ".xml",
            _ => ".bin"
        };

        public static bool IsValid(string format) =>
            format is Pdf or Docx or Xml;
    }
}
