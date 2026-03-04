using ErpOnlineOrder.Application.Interfaces.Security;

namespace ErpOnlineOrder.Application.Services
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            // S? d?ng BCrypt.Net-Next ?? hash password
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool Verify(string password, string hash)
        {
            // S? d?ng BCrypt.Net-Next ?? verify password
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
