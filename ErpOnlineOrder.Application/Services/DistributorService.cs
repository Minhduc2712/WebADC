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

        public async Task<Distributor> CreateDistributorAsync(Distributor distributor)
        {
            // Kiểm tra trùng lặp Distributor_code
            var existingByCode = await _distributorRepository.GetByCodeAsync(distributor.Distributor_code);
            if (existingByCode != null)
            {
                throw new InvalidOperationException($"Nhà phân phối với mã '{distributor.Distributor_code}' đã tồn tại.");
            }

            // Kiểm tra trùng lặp Distributor_name
            var existingByName = await _distributorRepository.GetByNameAsync(distributor.Distributor_name);
            if (existingByName != null)
            {
                throw new InvalidOperationException($"Nhà phân phối với tên '{distributor.Distributor_name}' đã tồn tại.");
            }

            distributor.Created_at = DateTime.UtcNow;
            distributor.Updated_at = DateTime.UtcNow;
            distributor.Is_deleted = false;
            await _distributorRepository.AddAsync(distributor);
            return distributor;
        }

        public async Task<bool> UpdateDistributorAsync(Distributor distributor)
        {
            var existing = await _distributorRepository.GetByIdAsync(distributor.Id);
            if (existing == null) return false;

            // Kiểm tra trùng lặp khi update (trừ bản ghi hiện tại)
            var existingByCode = await _distributorRepository.GetByCodeAsync(distributor.Distributor_code);
            if (existingByCode != null && existingByCode.Id != distributor.Id)
            {
                throw new InvalidOperationException($"Nhà phân phối với mã '{distributor.Distributor_code}' đã tồn tại.");
            }

            var existingByName = await _distributorRepository.GetByNameAsync(distributor.Distributor_name);
            if (existingByName != null && existingByName.Id != distributor.Id)
            {
                throw new InvalidOperationException($"Nhà phân phối với tên '{distributor.Distributor_name}' đã tồn tại.");
            }

            existing.Distributor_code = distributor.Distributor_code;
            existing.Distributor_name = distributor.Distributor_name;
            existing.Distributor_address = distributor.Distributor_address;
            existing.Distributor_phone = distributor.Distributor_phone;
            existing.Distributor_email = distributor.Distributor_email;
            existing.Updated_by = distributor.Updated_by;
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
