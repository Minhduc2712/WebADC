using ErpOnlineOrder.Application.DTOs.CustomerManagementDTOs;
using ErpOnlineOrder.Application.DTOs.EmailDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Services
{
    public class CustomerManagementService : ICustomerManagementService
    {
        private readonly ICustomerManagementRepository _customerManagementRepository;
        private readonly IEmailQueue _emailQueue;

        public CustomerManagementService(ICustomerManagementRepository customerManagementRepository, IEmailQueue emailQueue)
        {
            _customerManagementRepository = customerManagementRepository;
            _emailQueue = emailQueue;
        }

        public async Task<Customer_management?> GetByIdAsync(int id)
        {
            return await _customerManagementRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Customer_management>> GetAllAsync()
        {
            return await _customerManagementRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Customer_management>> GetByStaffAsync(int staffId)
        {
            return await _customerManagementRepository.GetByStaffAsync(staffId);
        }

        public async Task<IEnumerable<Customer_management>> GetByCustomerAsync(int customerId)
        {
            return await _customerManagementRepository.GetByCustomerAsync(customerId);
        }

        public async Task<IEnumerable<Customer_management>> GetByProvinceAsync(int provinceId)
        {
            return await _customerManagementRepository.GetByProvinceAsync(provinceId);
        }

        public async Task<Customer_management> CreateCustomerManagementAsync(CreateCustomerManagementDto dto, int createdBy)
        {
            var alreadyAssigned = await _customerManagementRepository.ExistsAsync(dto.Staff_id, dto.Customer_id);
            if (alreadyAssigned)
                throw new InvalidOperationException("Cán bộ này đã được gán phụ trách khách hàng này rồi.");

            var deleted = await _customerManagementRepository.FindDeletedAsync(dto.Staff_id, dto.Customer_id);
            if (deleted != null)
            {
                deleted.Province_id = dto.Province_id;
                deleted.Ward_id = dto.Ward_id;
                deleted.Updated_by = createdBy;
                deleted.Updated_at = DateTime.UtcNow;
                deleted.Is_deleted = false;
                await _customerManagementRepository.UpdateAsync(deleted);
                return deleted;
            }

            var assignment = new Customer_management
            {
                Staff_id = dto.Staff_id,
                Customer_id = dto.Customer_id,
                Province_id = dto.Province_id,
                Ward_id = dto.Ward_id,
                Created_by = createdBy,
                Updated_by = createdBy,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Is_deleted = false
            };
            await _customerManagementRepository.AddAsync(assignment);
            return assignment;
        }

        public async Task<bool> UpdateCustomerManagementAsync(int id, UpdateCustomerManagementDto dto, int updatedBy)
        {
            var existing = await _customerManagementRepository.GetByIdAsync(id);
            if (existing == null) return false;

            var duplicateExists = await _customerManagementRepository.ExistsAsync(dto.Staff_id, dto.Customer_id, id);
            if (duplicateExists)
                throw new InvalidOperationException("Cán bộ này đã được gán phụ trách khách hàng này rồi.");

            existing.Staff_id = dto.Staff_id;
            existing.Customer_id = dto.Customer_id;
            existing.Province_id = dto.Province_id;
            existing.Ward_id = dto.Ward_id;
            existing.Updated_by = updatedBy;
            existing.Updated_at = DateTime.UtcNow;
            await _customerManagementRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteCustomerManagementAsync(int id)
        {
            await _customerManagementRepository.DeleteAsync(id);
            return true;
        }

        public async Task<Customer_management> AssignStaffToCustomerAsync(int staffId, int customerId, int provinceId, int? wardId, int createdBy)
        {
            // Kiểm tra đã gán chưa (active)
            var alreadyAssigned = await _customerManagementRepository.ExistsAsync(staffId, customerId);
            if (alreadyAssigned)
            {
                throw new InvalidOperationException("Cán bộ này đã được gán phụ trách khách hàng này rồi.");
            }

            // Kiểm tra có bản ghi đã soft-delete → khôi phục thay vì tạo mới (tránh vi phạm unique index)
            var deleted = await _customerManagementRepository.FindDeletedAsync(staffId, customerId);
            if (deleted != null)
            {
                deleted.Province_id = provinceId;
                deleted.Ward_id = wardId;
                deleted.Updated_by = createdBy;
                deleted.Updated_at = DateTime.UtcNow;
                deleted.Is_deleted = false;
                await _customerManagementRepository.UpdateAsync(deleted);
                return deleted;
            }

            var assignment = new Customer_management
            {
                Staff_id = staffId,
                Customer_id = customerId,
                Province_id = provinceId,
                Ward_id = wardId,
                Created_by = createdBy,
                Updated_by = createdBy,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Is_deleted = false
            };

            await _customerManagementRepository.AddAsync(assignment);
            await _emailQueue.EnqueueAsync(new EmailMessage
            {
                ActionType = EmailActionType.StaffAssignmentNotification,
                PrimaryId = assignment.Customer_id,
                SecondaryId = assignment.Staff_id
            });
            return assignment;
        }

        public async Task<bool> RemoveAssignmentAsync(int id)
        {
            await _customerManagementRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> IsAlreadyAssignedAsync(int staffId, int customerId)
        {
            return await _customerManagementRepository.ExistsAsync(staffId, customerId);
        }

        public async Task<Customer_management?> FindAssignmentByProvinceAndWardAsync(int provinceId, int? wardId)
        {
            return await _customerManagementRepository.FindAssignmentByProvinceAndWardAsync(provinceId, wardId);
        }

        public async Task<int> BulkReplaceStaffAsync(int departingStaffId, int newStaffId, int updatedBy)
        {
            var assignments = (await _customerManagementRepository.GetByStaffAsync(departingStaffId)).ToList();
            int count = 0;

            foreach (var assignment in assignments)
            {
                var alreadyAssigned = await _customerManagementRepository.ExistsAsync(newStaffId, assignment.Customer_id);
                if (alreadyAssigned) continue;

                await _customerManagementRepository.DeleteAsync(assignment.Id);

                var deletedRecord = await _customerManagementRepository.FindDeletedAsync(newStaffId, assignment.Customer_id);
                if (deletedRecord != null)
                {
                    deletedRecord.Province_id = assignment.Province_id;
                    deletedRecord.Ward_id = assignment.Ward_id;
                    deletedRecord.Updated_by = updatedBy;
                    deletedRecord.Updated_at = DateTime.UtcNow;
                    deletedRecord.Is_deleted = false;
                    await _customerManagementRepository.UpdateAsync(deletedRecord);
                }
                else
                {
                    await _customerManagementRepository.AddAsync(new Customer_management
                    {
                        Staff_id = newStaffId,
                        Customer_id = assignment.Customer_id,
                        Province_id = assignment.Province_id,
                        Ward_id = assignment.Ward_id,
                        Created_by = updatedBy,
                        Updated_by = updatedBy,
                        Created_at = DateTime.UtcNow,
                        Updated_at = DateTime.UtcNow,
                        Is_deleted = false
                    });
                }

                await _emailQueue.EnqueueAsync(new EmailMessage
                {
                    ActionType = EmailActionType.StaffReplacementNotification,
                    PrimaryId = assignment.Customer_id,
                    SecondaryId = departingStaffId,
                    TertiaryId = newStaffId
                });
                count++;
            }

            return count;
        }

        public async Task<Customer_management> ReplaceStaffAsync(int existingAssignmentId, int newStaffId, int updatedBy)
        {
            var existing = await _customerManagementRepository.GetByIdAsync(existingAssignmentId);
            if (existing == null)
                throw new InvalidOperationException("Không tìm thấy phân công cán bộ cần thay thế.");

            if (existing.Staff_id == newStaffId)
                throw new InvalidOperationException("Cán bộ mới phải khác cán bộ hiện tại.");

            var alreadyAssigned = await _customerManagementRepository.ExistsAsync(newStaffId, existing.Customer_id);
            if (alreadyAssigned)
                throw new InvalidOperationException("Cán bộ mới đã được gán phụ trách khách hàng này rồi.");

            int oldStaffId = existing.Staff_id;
            int customerId = existing.Customer_id;
            int provinceId = existing.Province_id;
            int? wardId = existing.Ward_id;

            // Xóa mềm gán cũ
            await _customerManagementRepository.DeleteAsync(existingAssignmentId);

            // Tạo hoặc khôi phục gán mới
            Customer_management newAssignment;
            var deleted = await _customerManagementRepository.FindDeletedAsync(newStaffId, customerId);
            if (deleted != null)
            {
                deleted.Province_id = provinceId;
                deleted.Ward_id = wardId;
                deleted.Updated_by = updatedBy;
                deleted.Updated_at = DateTime.UtcNow;
                deleted.Is_deleted = false;
                await _customerManagementRepository.UpdateAsync(deleted);
                newAssignment = deleted;
            }
            else
            {
                newAssignment = new Customer_management
                {
                    Staff_id = newStaffId,
                    Customer_id = customerId,
                    Province_id = provinceId,
                    Ward_id = wardId,
                    Created_by = updatedBy,
                    Updated_by = updatedBy,
                    Created_at = DateTime.UtcNow,
                    Updated_at = DateTime.UtcNow,
                    Is_deleted = false
                };
                await _customerManagementRepository.AddAsync(newAssignment);
            }

            // Gửi email thông báo thay thế
            await _emailQueue.EnqueueAsync(new EmailMessage
            {
                ActionType = EmailActionType.StaffReplacementNotification,
                PrimaryId = customerId,
                SecondaryId = oldStaffId,
                TertiaryId = newStaffId
            });

            return newAssignment;
        }
    }
}