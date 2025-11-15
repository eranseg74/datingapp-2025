using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LikesRepository(AppDbContext context) : ILikesRepository
{
  public void AddLike(MemberLike like)
  {
    context.Likes.Add(like);
  }

  public void DeleteLike(MemberLike like)
  {
    context.Likes.Remove(like);
  }

  public async Task<IReadOnlyList<string>> GetCurrentMemberLikeIds(string memberId)
  {
    return await context.Likes
      .Where(x => x.SourceMemberId == memberId)
      .Select(x => x.TargetMemberId)
      .ToListAsync();
  }

  public async Task<MemberLike?> GetMemberLike(string sourceMemberId, string targetMemberId)
  {
    return await context.Likes.FindAsync(sourceMemberId, targetMemberId);
  }

  public async Task<PaginatedResult<Member>> GetMemberLikes(LikesParams likesParams)
  {
    var query = context.Likes.AsQueryable();
    IQueryable<Member> result;

    switch (likesParams.Predicate)
    {
      case "liked":
        result = query
          .Where(like => like.SourceMemberId == likesParams.MemberId)
          .Select(like => like.TargetMember);
        break;
      // return await query.Where(x => x.SourceMemberId == likesParams.CurrentMemberId).Select(x => x.TargetMember).ToListAsync();
      case "likedBy":
        result = query
          .Where(like => like.TargetMemberId == likesParams.MemberId)
          .Select(like => like.SourceMember);
        break;
      // return await query.Where(x => x.TargetMemberId == memberId).Select(x => x.SourceMember).ToListAsync();

      default: // mutual
        var likeIds = await GetCurrentMemberLikeIds(likesParams.MemberId); // A list of IDs of all the members that the specified member likes
        // A list of IDs of all the members that like the specified member and are in the likeIds list (meaning that the specified member likes them).
        result = query.Where(x => x.TargetMemberId == likesParams.MemberId && likeIds.Contains(x.SourceMemberId)).Select(x => x.SourceMember);
        // return await query
        //   .Where(x => x.TargetMemberId == memberId && likeIds
        //   .Contains(x.SourceMemberId))
        //   .Select(x => x.SourceMember)
        //   .ToListAsync();
        break;

    }
    return await PaginationHelper.CreateAsync(result, likesParams.PageNumber, likesParams.PageSize);
  }

  public async Task<bool> SaveAllChanges()
  {
    return await context.SaveChangesAsync() > 0;
  }
}
