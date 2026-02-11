using ErpOnlineOrder.Application.DTOs.DistributorDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Services
{
    public class DistributorService : IDistributorService
    {
        private readonly IDistributorRepository _distributorRepository;

        public DistributorService(IDistributorRepository distributorRepository)
        {
            _distributorRepository = distributorRepository;
        }

        public async Task<Distributor?> GetByIdAsync(int id)
        {
            return await _distributorRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Distributor>> GetAllAsync()
        {
            return await _distributorRepository.GetAllAsync();
        }

        public async Task<Distributor> CreateDistributorAsync(CreateDistributorDto dto, int createdBy)
        {
            var existingByCode = await _distributorRepository.GetByCodeAsync(dto.Distributor_code);
            if (existingByCode != null)
                throw new InvalidOperationException($"Nhà phân phối với mã '{dto.Distributor_code}' đã tồn tại.");

            var existingByName = await _distributorRepository.GetByNameAsync(dto.Distributor_name);
            if (existingByName != null)
                throw new InvalidOperationException($"Nhà phân phối với tên '{dto.Distributor_name}' đã tồn tại.");

            var distributor = new Distributor
            {
                Distributor_code = dto.Distributor_code,
                Distributor_name = dto.Distributor_name,
                Distributor_address = dto.Distributor_address,
                Distributor_phone = dto.Distributor_phone,
                Distributor_email = dto.Distributor_email,
                Created_by = createdBy,
                Updated_by = createdBy,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Is_deleted = false
            };
            await _distributorRepository.AddAsync(distributor);
            return distributor;
        }

        public async Task<bool> UpdateDistributorAsync(UpdateDistributorDto dto, int updatedBy)
        {
            var existing = await _distributorRepository.GetByIdAsync(dto.Id);
            if (existing == null) return false;

            var existingByCode = await _distributorRepository.GetByCodeAsync(dto.Distributor_code);
            if (existingByCode != null && existingByCode.Id != dto.Id)
                throw new InvalidOperationException($"Nhà phân phối với mã '{dto.Distributor_code}' đã tồn tại.");

            var existingByName = await _distributorRepository.GetByNameAsync(dto.Distributor_name);
            if (existingByName != null && existingByName.Id != dto.Id)
                throw new InvalidOperationException($"Nhà phân phối với tên '{dto.Distributor_name}' đã tồn tại.");

            existing.Distributor_code = dto.Distributor_code;
            existing.Distributor_name = dto.Distributor_name;
            existing.Distributor_address = dto.Distributor_address;
            existing.Distributor_phone = dto.Distributor_phone;
            existing.Distributor_email = dto.Distributor_email;
            existing.Updated_by = updatedBy;
            existing.Updated_at = DateTime.UtcNow;
            await _distributorRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteDistributorAsync(int id)
        {
            await _distributorRepository.DeleteAsync(id);
            return true;
        }
    }
}
