using API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    // Adding the LogUserActivity as an attribute. In this level - the attribute will be executed on every API call since they all derive from the BaseAPIController
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/[controller]")] // localhost:5000/api/<controller such as members, users...>
    [ApiController]
    public class BaseAPIController : ControllerBase
    {
    }
}
