using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Services
{
    public class PublisherService : IPublisherService
    {
        private readonly IPublisherRepository _publisherRepository;

        public PublisherService(IPublisherRepository publisherRepository)
        {
            _publisherRepository = publisherRepository;
        }

        public async Task<Publisher?> GetByIdAsync(int id)
        {
            return await _publisherRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Publisher>> GetAllAsync()
        {
            return await _publisherRepository.GetAllAsync();
        }

        public async Task<Publisher> CreateAsync(Publisher publisher)
        {
            // Kiểm tra trùng lặp Publisher_code
            var existingByCode = await _publisherRepository.GetByCodeAsync(publisher.Publisher_code);
            if (existingByCode != null)
            {
                throw new InvalidOperationException($"Nhà xuất bản với mã '{publisher.Publisher_code}' đã tồn tại.");
            }

            // Kiểm tra trùng lặp Publisher_name
            var existingByName = await _publisherRepository.GetByNameAsync(publisher.Publisher_name);
            if (existingByName != null)
            {
                throw new InvalidOperationException($"Nhà xuất bản với tên '{publisher.Publisher_name}' đã tồn tại.");
            }

            publisher.Created_at = DateTime.Now;
            publisher.Updated_at = DateTime.Now;
            return await _publisherRepository.AddAsync(publisher);
        }

        public async Task<bool> UpdateAsync(Publisher publisher)
        {
            var existing = await _publisherRepository.GetByIdAsync(publisher.Id);
            if (existing == null) return false;

            // Kiểm tra trùng lặp khi update (trừ bản ghi hiện tại)
            var existingByCode = await _publisherRepository.GetByCodeAsync(publisher.Publisher_code);
            if (existingByCode != null && existingByCode.Id != publisher.Id)
            {
                throw new InvalidOperationException($"Nhà xuất bản với mã '{publisher.Publisher_code}' đã tồn tại.");
            }

            var existingByName = await _publisherRepository.GetByNameAsync(publisher.Publisher_name);
            if (existingByName != null && existingByName.Id != publisher.Id)
            {
                throw new InvalidOperationException($"Nhà xuất bản với tên '{publisher.Publisher_name}' đã tồn tại.");
            }

            existing.Publisher_code = publisher.Publisher_code;
            existing.Publisher_name = publisher.Publisher_name;
            existing.Publisher_address = publisher.Publisher_address;
            existing.Publisher_phone = publisher.Publisher_phone;
            existing.Publisher_email = publisher.Publisher_email;
            existing.Updated_by = publisher.Updated_by;
            existing.Updated_at = DateTime.Now;
            await _publisherRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _publisherRepository.DeleteAsync(id);
            return true;
        }
    }
}
