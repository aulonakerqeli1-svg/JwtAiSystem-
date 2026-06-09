using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ResourceService2.Controllers;

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
            service = "ResourceService2"
        });

    [HttpGet("protected")]
    [Authorize]
    public IActionResult GetProtected()
        => Ok(new
        {
            message = "✅ JWT valid — akses i lejuar!",
            service = "ResourceService2",
            user = User.Identity?.Name,
            time = DateTime.UtcNow
        });

    [HttpGet("data")]
    [Authorize]
    public IActionResult GetData()
        => Ok(new
        {
            service = "ResourceService2",
            data = new[]
            {
                new { id = 1, value = "Produkt A i mbrojtur" },
                new { id = 2, value = "Produkt B i mbrojtur" },
                new { id = 3, value = "Produkt C i mbrojtur" }
            }
        });
}