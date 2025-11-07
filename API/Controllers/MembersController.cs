using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize] // Defining an attribute above the class will enforce the attribute on all endpoints. Note! If we define [Authorized] in the class level we can exclude a certain endpoint by defining it as [AllowAnonymous]. It is not possible to define [AllowAnonymous] on the class level and put [Authorized] on a certain endpoint
    public class MembersController(IMemberRepository memberRepository) : BaseAPIController
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Member>>> GetMembers()
        {
            // The GetMembersAsync returns a IReadOnlyList<Member> but the method needs to return an action result so we wrap it with an Ok action result
            return Ok(await memberRepository.GetMembersAsync());
        }

        // [Authorize]
        [HttpGet("{id}")] // localhost:5000/api/members/bob-id
        public async Task<ActionResult<Member>> GetMember(string id)
        {
            var member = await memberRepository.GetMembeByIdAsync(id); //context.Users.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return Ok(member);
        }

        [HttpGet("{id}/photos")]
        public async Task<ActionResult<IReadOnlyList<Photo>>> GetMemberPhotos(string id)
        {
            return Ok(await memberRepository.GetPhotosForMemberAsync(id));
        }
    }

}
