using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IPhotoRepository
{
  Task<IReadOnlyList<PhotoForApprovalDTO>> GetUnapprovedPhotos();
  Task<Photo?> GetPhotoById(int id);
  void RemovePhoto(Photo photo);
}
