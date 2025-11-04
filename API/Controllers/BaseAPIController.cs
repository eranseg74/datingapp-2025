using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")] // localhost:5000/api/<controller such as members, users...>
    [ApiController]
    public class BaseAPIController : ControllerBase
    {
    }
}
