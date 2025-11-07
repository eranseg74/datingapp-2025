using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.DTOs;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
  // The purpose of this method is to check if the DB is empty. If not then nothing will happen because we have all the data and schemas. Otherwise, the method will run a migration to create all the schemas and insert all the users and members to the DB from the seed file
  public static async Task SeedUsers(AppDbContext context)
  {
    // Since this is a static method it will run once when the program starts. Here we check on initialization if there are users in the DB and if so we will not seed the data from the JSON file
    if (await context.Users.AnyAsync()) return;

    var memberData = await File.ReadAllTextAsync("Data/UserSeedData.json");
    // The data from the json file will come as a string so we need to deserialize it into a C# object
    var members = JsonSerializer.Deserialize<List<SeedUserDTO>>(memberData);

    // Note that the members is nullable so we need to check it. If there are no members then we have nothing to do so we return
    if (members == null)
    {
      System.Console.WriteLine("No members in seed data");
      return;
    }

    // Idf there are members we will run a for each loop to map all the members properties and define them as users and members. Since we don't have the password hash and salt we will need to do it inside the for each loop

    // This should be inside the loop. Otherwise, all the users will get the same PasswordSalt which is the hmac key
    //using var hmac = new HMACSHA512();

    foreach (var member in members)
    {
      using var hmac = new HMACSHA512();
      var user = new AppUser
      {
        Id = member.Id,
        Email = member.Email,
        DisplayName = member.DisplayName,
        ImageUrl = member.ImageUrl,
        PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd")),
        PasswordSalt = hmac.Key,
        Member = new Member
        {
          Id = member.Id,
          DisplayName = member.DisplayName,
          Description = member.Description,
          DateOfBirth = member.DateOfBirth,
          ImageUrl = member.ImageUrl,
          Gender = member.Gender,
          City = member.City,
          Country = member.Country,
          LastActive = member.LastActive,
          Created = member.Created,
        }
      };
      user.Member.Photos.Add(new Photo
      {
        Url = member.ImageUrl!,
        MemberId = member.Id,
      });
      context.Users.Add(user);
    }
    await context.SaveChangesAsync();
  }
}
