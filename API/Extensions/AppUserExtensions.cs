using API.DTOs;
using API.Entities;
using API.Interfaces;

namespace API.Extensions;

public static class AppUserExtensions
{
  public static async Task<UserDTO> ToDTO(this AppUser user, ITokenService tokenService) // Static classes cannot use dependency injection so we have to pass the ITokenService as a parameter
  {
    return new UserDTO
    {
      Id = user.Id,
      DisplayName = user.DisplayName,
      Email = user.Email!,
      ImageUrl = user.ImageUrl,
      Token = await tokenService.CreateToken(user)
    };
  }
}
