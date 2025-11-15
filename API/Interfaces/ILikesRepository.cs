using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface ILikesRepository
{
  // Getting a single MemberLike between the source and target members
  Task<MemberLike?> GetMemberLike(string sourceMemberId, string targetMemberId);

  // A method that returns a list of members according to the predicate parameter - could be all the members that the specified member likes, all the members that like the specified member, all the members that like the specified member and are liked by him (mutual likes). Specified member - by the member Id.
  Task<PaginatedResult<Member>> GetMemberLikes(LikesParams likesParams);

  // Return a list of Ids of all the members that a certain member likes
  Task<IReadOnlyList<string>> GetCurrentMemberLikeIds(string memberId);

  // Delete a like
  void DeleteLike(MemberLike like);

  // Add a like
  void AddLike(MemberLike like);

  // This method will save all the updates to the database
  Task<bool> SaveAllChanges();
}
