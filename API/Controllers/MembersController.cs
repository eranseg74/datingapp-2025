using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    //[Authorize] // Defining an attribute above the class will enforce the attribute on all endpoints. Note! If we define [Authorized] in the class level we can exclude a certain endpoint by defining it as [AllowAnonymous]. It is not possible to define [AllowAnonymous] on the class level and put [Authorized] on a certain endpoint
    public class MembersController(AppDbContext context) : BaseAPIController
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AppUser>>> GetMembers()
        {
            var members = await context.Users.ToListAsync();
            return members;
        }

        [Authorize]
        [HttpGet("{id}")] // localhost:5000/api/members/bob-id
        public async Task<ActionResult<AppUser>> GetMember(string id)
        {
            var member = await context.Users.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return member;
        }
    }

}
