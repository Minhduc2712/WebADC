namespace ErpOnlineOrder.Application.DTOs.AuthorDTOs
{
    public class AuthorDto
    {
        public int Id { get; set; }
        public string Author_code { get; set; } = null!;
        public string Author_name { get; set; } = null!;
        public string? Pen_name { get; set; }
        public string? Email_author { get; set; }
        public string? Phone_number { get; set; }
        public string? birth_date { get; set; }
        public string? death_date { get; set; }
        public string? Nationality { get; set; }
        public string? Biography { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
    }
}
