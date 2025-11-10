using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
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

        [HttpPut]
        // Not specifying a type in the action result means that we are not actually returning anything
        public async Task<ActionResult> UpdateMember(MemberUpdateDTO memberUpdateDTO)
        {
            var memberId = User.GetMemberId();
            // No need to check because we generate the token so we knoe that there will be a token to extract the member id from
            // if (memberId == null)
            // {
            //     return BadRequest("Oops - no id found in token");
            // }
            var member = await memberRepository.GetMemberForUpdate(memberId);
            if (member == null)
            {
                return BadRequest("Could not get member");
            }
            member.DisplayName = memberUpdateDTO.DisplayName ?? member.DisplayName;
            member.Description = memberUpdateDTO.Description ?? member.Description;
            member.City = memberUpdateDTO.City ?? member.City;
            member.Country = memberUpdateDTO.Country ?? member.Country;
            member.User.DisplayName = memberUpdateDTO.DisplayName ?? member.User.DisplayName;

            // This method just turns the state of the member as modifies. This means that even if there are no changes the state will still be changed to modified. This is good because otherwise, when we will save the changes, the controller will return a BadRequest saying that no changes where made to the object which is true. Updating the state to modified in all cases helps us avoid the BadRequest even if the new values in the update request are exactly the same as the values in the current member object
            memberRepository.Update(member); // Optional

            if (await memberRepository.SaveAllAsync())
            {
                return NoContent();
            }
            return BadRequest("Failed to update member");
        }
    }
}
