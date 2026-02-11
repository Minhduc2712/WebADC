using ErpOnlineOrder.Application.DTOs.PublisherDTOs;
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

        public async Task<Publisher> CreateAsync(CreatePublisherDto dto, int createdBy)
        {
            var existingByCode = await _publisherRepository.GetByCodeAsync(dto.Publisher_code);
            if (existingByCode != null)
                throw new InvalidOperationException($"Nhà xuất bản với mã '{dto.Publisher_code}' đã tồn tại.");

            var existingByName = await _publisherRepository.GetByNameAsync(dto.Publisher_name);
            if (existingByName != null)
                throw new InvalidOperationException($"Nhà xuất bản với tên '{dto.Publisher_name}' đã tồn tại.");

            var publisher = new Publisher
            {
                Publisher_code = dto.Publisher_code,
                Publisher_name = dto.Publisher_name,
                Publisher_address = dto.Publisher_address,
                Publisher_phone = dto.Publisher_phone,
                Publisher_email = dto.Publisher_email,
                Created_by = createdBy,
                Updated_by = createdBy,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow
            };
            await _publisherRepository.AddAsync(publisher);
            return publisher;
        }

        public async Task<bool> UpdateAsync(UpdatePublisherDto dto, int updatedBy)
        {
            var existing = await _publisherRepository.GetByIdAsync(dto.Id);
            if (existing == null) return false;

            var existingByCode = await _publisherRepository.GetByCodeAsync(dto.Publisher_code);
            if (existingByCode != null && existingByCode.Id != dto.Id)
                throw new InvalidOperationException($"Nhà xuất bản với mã '{dto.Publisher_code}' đã tồn tại.");

            var existingByName = await _publisherRepository.GetByNameAsync(dto.Publisher_name);
            if (existingByName != null && existingByName.Id != dto.Id)
                throw new InvalidOperationException($"Nhà xuất bản với tên '{dto.Publisher_name}' đã tồn tại.");

            existing.Publisher_code = dto.Publisher_code;
            existing.Publisher_name = dto.Publisher_name;
            existing.Publisher_address = dto.Publisher_address;
            existing.Publisher_phone = dto.Publisher_phone;
            existing.Publisher_email = dto.Publisher_email;
            existing.Updated_by = updatedBy;
            existing.Updated_at = DateTime.UtcNow;
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
