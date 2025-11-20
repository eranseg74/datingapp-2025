namespace API.Interfaces;

public interface IUnitOfWork
{
  IMemberRepository MemberRepository { get; }
  IMessageRepository MessageRepository { get; }
  ILikesRepository LikesRepository { get; }
  IPhotoRepository PhotoRepository { get; }
  Task<bool> Complete(); // This method will be used to save all the updates done by the repositories
  bool HasChanges(); // This method will indicate if there are changes that require saving
}
