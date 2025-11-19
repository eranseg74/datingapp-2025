

using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class LikesController(IUnitOfWork unitOfWork) : BaseAPIController
    {
        [HttpPost("{targetMemberId}")]
        public async Task<ActionResult> ToggleLike(string targetMemberId)
        {
            var sourceMemberId = User.GetMemberId();
            if (sourceMemberId == targetMemberId) return BadRequest("You cannot like tourself");

            var existingLike = await unitOfWork.LikesRepository.GetMemberLike(sourceMemberId, targetMemberId);
            if (existingLike == null)
            {
                var like = new MemberLike
                {
                    SourceMemberId = sourceMemberId,
                    TargetMemberId = targetMemberId
                };
                unitOfWork.LikesRepository.AddLike(like); // This will go to the EntityFramework's tracking and will be commited to the DB when executing the SaveAllChanges method
            }
            else
            {
                unitOfWork.LikesRepository.DeleteLike(existingLike); // This will go to the EntityFramework's tracking and will be commited to the DB when executing the SaveAllChanges method
            }
            if (await unitOfWork.Complete()) return Ok();
            return BadRequest("Failed to update like");
        }

        // Returns a list of all the members that the specified member likes. Will be used to display them in the Matches screen
        [HttpGet("list")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetCurrentMemberLikeIds()
        {
            return Ok(await unitOfWork.LikesRepository.GetCurrentMemberLikeIds(User.GetMemberId()));
        }

        // Returns all the members that the specified member like, liked by. or mutual like
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<Member>>> GetMemberLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.MemberId = User.GetMemberId();
            return Ok(await unitOfWork.LikesRepository.GetMemberLikes(likesParams));
        }
    }
}
