using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Application.DTOs.DistributorDTOs;
namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DistributorController : ControllerBase
    {
        private readonly IDistributorService _distributorService;

        public DistributorController(IDistributorService distributorService)
        {
            _distributorService = distributorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDistributors()
        {
            var distributors = await _distributorService.GetAllAsync();
            return Ok(distributors.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDistributor(int id)
        {
            var distributor = await _distributorService.GetByIdAsync(id);
            if (distributor == null) return NotFound();
            return Ok(MapToDto(distributor));
        }

        [HttpPost]
        public async Task<IActionResult> CreateDistributor([FromBody] CreateDistributorDto dto)
        {
            try
            {
                var entity = new Distributor
                {
                    Distributor_code = dto.Distributor_code,
                    Distributor_name = dto.Distributor_name,
                    Distributor_address = dto.Distributor_address,
                    Distributor_phone = dto.Distributor_phone,
                    Distributor_email = dto.Distributor_email,
                    Created_by = 0,
                    Updated_by = 0
                };
                var created = await _distributorService.CreateDistributorAsync(entity);
                return Ok(MapToDto(created));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDistributor(int id, [FromBody] UpdateDistributorDto dto)
        {
            if (id != dto.Id) return BadRequest();

            try
            {
                var entity = new Distributor
                {
                    Id = dto.Id,
                    Distributor_code = dto.Distributor_code,
                    Distributor_name = dto.Distributor_name,
                    Distributor_address = dto.Distributor_address,
                    Distributor_phone = dto.Distributor_phone,
                    Distributor_email = dto.Distributor_email,
                    Updated_by = 0
                };
                var result = await _distributorService.UpdateDistributorAsync(entity);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private static DistributorDto MapToDto(Distributor d)
        {
            return new DistributorDto
            {
                Id = d.Id,
                Distributor_code = d.Distributor_code,
                Distributor_name = d.Distributor_name,
                Distributor_address = d.Distributor_address ?? "",
                Distributor_phone = d.Distributor_phone ?? "",
                Distributor_email = d.Distributor_email ?? "",
                Created_at = d.Created_at,
                Updated_at = d.Updated_at
            };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDistributor(int id)
        {
            // Require permission: DELETE_DISTRIBUTOR
            try
            {
                var result = await _distributorService.DeleteDistributorAsync(id);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}