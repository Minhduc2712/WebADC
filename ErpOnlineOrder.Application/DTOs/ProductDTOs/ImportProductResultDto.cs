namespace ErpOnlineOrder.Application.DTOs.ProductDTOs
{
    public class ImportProductResultDto
    {
        public int TotalRows { get; set; }
        public int Succeeded { get; set; }
        public int Skipped { get; set; }
        public List<ImportProductRowError> Errors { get; set; } = new();
    }

    public class ImportProductRowError
    {
        public int Row { get; set; }
        public string ProductCode { get; set; } = "";
        public string Message { get; set; } = "";
    }
}
