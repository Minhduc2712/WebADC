namespace ErpOnlineOrder.Domain.Models
{
    public class User_role
    {
        public int Id { get; set; }
        public int User_id { get; set; }
        public virtual User User { get; set; } = null!;
        public int Role_id { get; set; }
        public virtual Role Role { get; set; } = null!;
        public int Created_by { get; set; }
        public DateTime Created_at { get; set; }
        public int Updated_by { get; set; }
        public DateTime Updated_at { get; set; }
        public bool Is_deleted { get; set; }


    }
}
