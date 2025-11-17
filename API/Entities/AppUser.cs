using Microsoft.AspNetCore.Identity;

namespace API.Entities;

public class AppUser : IdentityUser
{
  // Each field will represent a column in the DB. Any field with the name Id will automatically be defined as the primary key
  // The Id, Email, PasswordHash, and PasswordSalt are removed because we get them from the AspNetCore.Identity package
  // public string Id { get; set; } = Guid.NewGuid().ToString();
  public required string DisplayName { get; set; }
  // public required string Email { get; set; }
  public string? ImageUrl { get; set; }
  // public required byte[] PasswordHash { get; set; } // Sqlite does not know what is byte array and will save it as blob
  // public required byte[] PasswordSalt { get; set; }
  public string? RefreshToken { get; set; }
  public DateTime? RefreshTokenExpiry { get; set; }

  // Navigation property
  public Member Member { get; set; } = null!;
}
// After any change in an Entity class we have to run migration in order to update the fields (columns) in our DB
// Migration is executed by the following command: dotnet ef migrations add <migration name>
// This will create a new migration with the following name: <migration date>_<migration name>.
// Once there is a migration folder all the migrations will be located there.
// In this project it is under the Data/Migrations folder
// Updating the DB with the new migration: dotnet ef database update