using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MemberRepository(AppDbContext context) : IMemberRepository
{
  // Getting a member by id. This method will return null if the member is not found. The returned member will not include related entities such as photos or User
  public async Task<Member?> GetMembeByIdAsync(string id)
  {
    return await context.Members.FindAsync(id); // If the user is not found this will return null
  }

  // The GetMemberForUpdate will include the User entity as well which is needed when updating the member because we might need to update properties in the User entity as well.
  // We still keep the GetMember method also because the FindAsync function is most effective for getting data from the DB, so , unless we need the photos or User we will prefer to use the FindAsync function
  public async Task<Member?> GetMemberForUpdate(string id)
  {
    return await context.Members.Include(x => x.User).SingleOrDefaultAsync(x => x.Id == id);
  }

  public async Task<IReadOnlyList<Member>> GetMembersAsync()
  {
    // Entity Framework will not automatically return related entity with this methos. If we wanted the list of photos included for each member in the members list we would have to write it as follows:
    // return await context.Members.Include(x => x.Photos).ToListAsync();
    return await context.Members.ToListAsync();
  }

  public async Task<IReadOnlyList<Photo>> GetPhotosForMemberAsync(string memberId)
  {
    // Finding the user by the member id, then selecting all the photos of that user, then turning it to a list, so at the end, this will return a list of the user's photos
    return await context.Members.Where(x => x.Id == memberId).SelectMany(x => x.Photos).ToListAsync();
  }

  public async Task<bool> SaveAllAsync()
  {
    return await context.SaveChangesAsync() > 0; // The SaveChangesAsync returns a list of all the changes so we are returning if it is greater than 0 which means that at least one change occured
  }

  // Note that using this function will set the entity state to modified even if the updating properties are identical to the original values, so the SaveChangesAsync above will return true even if there are no changes
  public void Update(Member member)
  {
    context.Entry(member).State = EntityState.Modified; // Updating the tracking of the entity in order to say that something was modified
  }
}
