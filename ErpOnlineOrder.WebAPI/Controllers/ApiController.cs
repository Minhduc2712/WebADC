using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ErpOnlineOrder.WebAPI.Controllers
{
    public abstract class ApiController : ControllerBase
    {
        protected int GetCurrentUserId()
        {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)??User.FindFirst("UserId");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
            throw new UnauthorizedAccessException("User not authenticated");
        }
        return userId;
        }
    }
}