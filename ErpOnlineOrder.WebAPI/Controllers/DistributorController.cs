using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Services;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Application.DTOs.DistributorDTOs;
namespace ErpOnlineOrder.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DistributorController : ApiController
    {
        private readonly IDistributorService _distributorService;
        private readonly IPermissionService _permissionService;

        public DistributorController(IDistributorService distributorService, IPermissionService permissionService)
        {
            _distributorService = distributorService;
            _permissionService = permissionService;
        }

        [HttpGet("for-select")]
        public async Task<IActionResult> GetForSelect()
        {
            var list = await _distributorService.GetForSelectAsync();
            return Ok(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetDistributors()
        {
            var distributors = await _distributorService.GetAllAsync();
            var list = distributors.Select(EntityMappers.ToDistributorDto).ToList();
            var userId = TryGetCurrentUserId();
            if (userId.HasValue && userId.Value > 0)
            {
                var permissions = (await _permissionService.GetUserPermissionsAsync(userId.Value)).ToHashSet();
                foreach (var dto in list)
                    RecordPermissionEnricher.Enrich(dto, permissions, PermissionCodes.DistributorUpdate, PermissionCodes.DistributorDelete);
            }
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDistributor(int id)
        {
            var distributor = await _distributorService.GetByIdAsync(id);
            if (distributor == null) return NotFound();
            var dto = EntityMappers.ToDistributorDto(distributor);
            var userId = TryGetCurrentUserId();
            if (userId.HasValue && userId.Value > 0)
                await RecordPermissionEnricher.EnrichAsync(dto, userId.Value, _permissionService, PermissionCodes.DistributorUpdate, PermissionCodes.DistributorDelete);
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDistributor([FromBody] CreateDistributorDto dto)
        {
            try
            {
                var created = await _distributorService.CreateDistributorAsync(dto, GetCurrentUserId());
                return Ok(EntityMappers.ToDistributorDto(created));
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
                var result = await _distributorService.UpdateDistributorAsync(dto, GetCurrentUserId());
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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