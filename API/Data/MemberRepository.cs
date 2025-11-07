using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MemberRepository(AppDbContext context) : IMemberRepository
{
  public async Task<Member?> GetMembeByIdAsync(string id)
  {
    return await context.Members.FindAsync(id); // If the user is not found this will return null
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

  public void Update(Member member)
  {
    context.Entry(member).State = EntityState.Modified; // Updating the tracking of the entity in order to say that something was modified
  }
}
