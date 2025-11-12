using CloudinaryDotNet.Actions;

namespace API.Interfaces;

public interface IPhotoService
{
  Task<ImageUploadResult> UploadPhotoAsync(IFormFile file); // IFormFile is used to represent a file in dotNet  
  Task<DeletionResult> DeletePhotoAsync(string publicId); // After uploading a photo one of its parameters is the publicId which will be used for identification and deletion
}
