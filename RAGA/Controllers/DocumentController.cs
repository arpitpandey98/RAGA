using Microsoft.AspNetCore.Mvc;

namespace RAGA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Works!");
        }
    }
}
