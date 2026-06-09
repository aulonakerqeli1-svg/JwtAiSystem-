using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ResourceService1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult GetPublic()
        => Ok(new
        {
            message = "Ky endpoint është publik",
            service = "ResourceService1"
        });

    [HttpGet("protected")]
    [Authorize]
    public IActionResult GetProtected()
        => Ok(new
        {
            message = "✅ JWT valid — akses i lejuar!",
            service = "ResourceService1",
            user = User.Identity?.Name,
            time = DateTime.UtcNow
        });

    [HttpGet("data")]
    [Authorize]
    public IActionResult GetData()
        => Ok(new
        {
            service = "ResourceService1",
            data = new[]
            {
                new { id = 1, value = "Record A konfidencial" },
                new { id = 2, value = "Record B konfidencial" },
                new { id = 3, value = "Record C konfidencial" }
            }
        });
}