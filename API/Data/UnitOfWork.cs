using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

// The idea and the crutual aspect of the Unit of Work is that all the repositories share the same AppDbContext.
// The unit of work will be responsible for initializing the repositories we are using inside the controllers or in all other places in the code. So the UnitOfWork will coordinate all the repositories operations with aa single transaction so either all of the changes commited or non of them are
public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
  private IMemberRepository? _memberRepository;
  private IMessageRepository? _messageRepository;
  private ILikesRepository? _likesRepository;
  private IPhotoRepository? _photoRepository;

  // The ??= sign means that if the left side is null, the right side will be assigned to the variable
  public IMemberRepository MemberRepository => _memberRepository ??= new MemberRepository(context);
  public IMessageRepository MessageRepository => _messageRepository ??= new MessageRepository(context);
  public ILikesRepository LikesRepository => _likesRepository ??= new LikesRepository(context);
  public IPhotoRepository PhotoRepository => _photoRepository ??= new PhotoRepository(context);

  public async Task<bool> Complete()
  {
    try
    {
      return await context.SaveChangesAsync() > 0; // Checking if something was changed in the database
    }
    catch (DbUpdateException ex)
    {
      throw new Exception("An error occured while saving changes", ex);
    }
  }

  public bool HasChanges()
  {
    // Entity Framework also has a HasChanges tracker method so we use it
    return context.ChangeTracker.HasChanges();
  }
}
