using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

// It is the framework's responsibility to create the AppDbContext instance once an object is created from this class, and it is also the framework's responsibility to dispose of it once this account controller is out of scope
public class AccountController(AppDbContext context, ITokenService tokenService) : BaseAPIController
{
  [HttpPost("register")] // .../api/account/register
  // Note that all the parameters are strings this will make the API controller to look for a query string that matches these parameters and not the body of the request! If all the parameters match the controller will also bind the values to these parameters. If we wat to bind to the request's body we cannot use plain strings but rather create an object - a DTO (Data Transfer Object)
  // public async Task<ActionResult<AppUser>> Register(string email, string displayName, string password)
  public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
  {
    if (await EmailExists(registerDTO.Email))
    {
      return BadRequest("Email taken");
    }
    // Normally, when the account controller is out of scope the HMACSHA512 will be left without resource and wait for the garbage collector to dispose of it. If we don't want to wait for the garbage collection and dispose the class immediatelly after the controller is out of scope we can use the 'using' keyword for that. We can do this type of actions only on classes that implement the iDisposable interface. Here the HMACSHA512 class has a parent which also derives from a parent and so on. The 4th parent implements the iDisposable interface and therefore the HMACSHA512 supports the Dispose method
    using var hmac = new HMACSHA512();

    var user = new AppUser
    {
      DisplayName = registerDTO.DisplayName,
      Email = registerDTO.Email,
      PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)), // The Encoding.UTF8.GetBytes converts the password into bytes array
      PasswordSalt = hmac.Key
    };

    // Adding the user to the DB
    context.Users.Add(user); // context.Users refer to the Users table in the DB. This line tells the entity framework to track what is going on with this entity
    // Saving the inserted data
    await context.SaveChangesAsync(); // This line will save any of those track changes to the DB
    return user.ToDTO(tokenService);
  }

  private async Task<bool> EmailExists(string email)
  {
    return await context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
  }

  [HttpPost("login")]
  public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO) // Note that when sending an object the API controller will search at the body of the request. If we want to send a body but look at the query string we need to specify it using the [FromQuery] notation before the object. Will look like this: Login([FromQuery]LoginDTO loginDTO). The other way around is also possible. If we want to pass strings but get the data from the body we could use [FromBody]
  {
    // There are a lot more options depending on the developer's needs - FirstOrDefaultAsync, LastOrDefaultAsync - which we can also use. Need to read the descriptions of each option to select the best one
    var user = await context.Users.SingleOrDefaultAsync(x => x.Email == loginDTO.Email);
    if (user == null)
    {
      return Unauthorized("Invalid email address");

    }
    // Validating the password
    using var hmac = new HMACSHA512(user.PasswordSalt); // Passing the PasswordSalt will ensure that we will get the same hash that was generated when the user was created. We can see it in the register method where we set the PasswordSalt to the hmac.key, the key that was randomly generated when a new HMACSHA512 was instantiated with no parameters

    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));
    for (var i = 0; i < computedHash.Length; i++)
    {
      if (computedHash[i] != user.PasswordHash[i])
      {
        return Unauthorized("Invalid password");
      }
    }
    return user.ToDTO(tokenService);
  }
}
// dotnet ef database drop - This command will delete the DB. Because we are using sqlite, the db file will be deleted
// dotnet ef database update - In case of no DB, this will create the DB file with the appropriate fields (columns)