using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IMemberRepository
{
  void Update(Member member);
  // Task<bool> SaveAllAsync(); // // Is handled through the Complete() methos in the UnitOfWork
  // The GetMembersAsync will not return a list of all existing members but a class of PaginatedResult which contains a list of T items, in this case T = Member, and the Metadata class 
  Task<PaginatedResult<Member>> GetMembersAsync(MemberParams memberParams);
  Task<Member?> GetMembeByIdAsync(string id);
  Task<IReadOnlyList<Photo>> GetPhotosForMemberAsync(string memberId, bool isCurrentUser);
  Task<Member?> GetMemberForUpdateAsync(string id);
}
