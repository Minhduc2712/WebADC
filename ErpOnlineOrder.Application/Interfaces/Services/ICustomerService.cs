using ErpOnlineOrder.Application.DTOs.CustomerDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface ICustomerService
    {
        Task<bool> UpdateCustomerAsync(UpdateCustomerDto dto);
        Task<bool> UpdateOrganizationAsync(UpdateOrganizationByCustomerDto dto);
    }
}
