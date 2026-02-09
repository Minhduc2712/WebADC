using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;

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
            IEnumerable<Distributor> distributors;
            distributors = await _distributorService.GetAllAsync();
            
            return Ok(distributors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDistributor(int id)
        {
            var distributor = await _distributorService.GetByIdAsync(id);
            if (distributor == null) return NotFound();
            return Ok(distributor);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDistributor([FromBody] Distributor model)
        {
            // Require permission: ADD_DISTRIBUTOR
            try
            {
                var created = await _distributorService.CreateDistributorAsync(model);
                return Ok(created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDistributor(int id, [FromBody] Distributor model)
        {
            // Require permission: EDIT_DISTRIBUTOR
            if (id != model.Id) return BadRequest();

            try
            {
                var result = await _distributorService.UpdateDistributorAsync(model);
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