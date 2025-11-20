using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize] // Defining an attribute above the class will enforce the attribute on all endpoints. Note! If we define [Authorized] in the class level we can exclude a certain endpoint by defining it as [AllowAnonymous]. It is not possible to define [AllowAnonymous] on the class level and put [Authorized] on a certain endpoint
    public class MembersController(IUnitOfWork unitOfWork, IPhotoService photoService) : BaseAPIController
    {
        [HttpGet]
        // Note that the pagingParams is an object of type PagingParams so it will search the properties in the request's body. This is why we need to explicitly tell it to get the properties from the query url
        public async Task<ActionResult<IReadOnlyList<Member>>> GetMembers([FromQuery] MemberParams memberParams)
        {
            // Setting the Id in the member param to be the current user id taken from the user claims
            memberParams.CurrentMemberId = User.GetMemberId();
            // The GetMembersAsync returns a IReadOnlyList<Member> but the method needs to return an action result so we wrap it with an Ok action result
            return Ok(await unitOfWork.MemberRepository.GetMembersAsync(memberParams));
        }

        // [Authorize]
        [HttpGet("{id}")] // localhost:5000/api/members/bob-id
        public async Task<ActionResult<Member>> GetMember(string id)
        {
            var member = await unitOfWork.MemberRepository.GetMembeByIdAsync(id); //context.Users.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return Ok(member);
        }

        [HttpGet("{id}/photos")]
        public async Task<ActionResult<IReadOnlyList<Photo>>> GetMemberPhotos(string id)
        {
            var isCurrentUser = id == User.GetMemberId();
            return Ok(await unitOfWork.MemberRepository.GetPhotosForMemberAsync(id, isCurrentUser));
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
            var member = await unitOfWork.MemberRepository.GetMemberForUpdateAsync(memberId);
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
            unitOfWork.MemberRepository.Update(member); // Optional

            if (await unitOfWork.Complete())
            {
                return NoContent();
            }
            return BadRequest("Failed to update member");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<Photo>> AddPhoto([FromForm] IFormFile file)
        {
            var member = await unitOfWork.MemberRepository.GetMemberForUpdateAsync(User.GetMemberId());
            if (member == null)
            {
                return BadRequest("Cannot update user");
            }
            var result = await photoService.UploadPhotoAsync(file);
            if (result.Error != null) // The result.Error comes from the error mechanism of Cloudinary which also provides decriptive messages that we can use
            {
                return BadRequest(result.Error.Message);
            }
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
                MemberId = User.GetMemberId()
            };
            // if (member.ImageUrl == null)
            // {
            //     member.ImageUrl = photo.Url;
            //     member.User.ImageUrl = photo.Url;
            // }
            member.Photos.Add(photo);
            if (await unitOfWork.Complete())
            {
                return photo;
            }
            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var member = await unitOfWork.MemberRepository.GetMemberForUpdateAsync(User.GetMemberId());
            if (member == null) return BadRequest("Cannot get member from token");
            var photo = member.Photos.SingleOrDefault(x => x.Id == photoId);
            if (member.ImageUrl == photo?.Url || photo == null)
            {
                return BadRequest("Cannot set this as main image");
            }
            member.ImageUrl = photo.Url;
            member.User.ImageUrl = photo.Url;
            if (await unitOfWork.Complete()) return NoContent(); // Returns 204 - Ok + no further info is required
            return BadRequest("Problem setting main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var member = await unitOfWork.MemberRepository.GetMemberForUpdateAsync(User.GetMemberId());
            if (member == null) return BadRequest("Cannot get member from token");
            var photo = member.Photos.SingleOrDefault(x => x.Id == photoId);
            if (photo == null || member.ImageUrl == photo.Url)
            {
                return BadRequest("Cannot delete this photo");
            }
            if (photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null)
                {
                    return BadRequest(result.Error.Message);
                }
            }
            member.Photos.Remove(photo);
            if (await unitOfWork.Complete())
            {
                return Ok();
            }
            return BadRequest("Problem deleting the photo");
        }
    }
}
