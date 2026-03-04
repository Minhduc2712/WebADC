using ErpOnlineOrder.Application.DTOs.PublisherDTOs;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IPublisherService
    {
        Task<Publisher?> GetByIdAsync(int id);
        Task<IEnumerable<Publisher>> GetAllAsync();
        Task<Publisher> CreateAsync(CreatePublisherDto dto, int createdBy);
        Task<bool> UpdateAsync(UpdatePublisherDto dto, int updatedBy);
        Task<bool> DeleteAsync(int id);
    }
}
