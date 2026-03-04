using System.Threading.Tasks;
using ErpOnlineOrder.Application.DTOs.AuthDTOs;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IUserRegistrationService
    {
        Task<bool> RegisterAsync(RegisterCustomerDto dto);
    }
}
