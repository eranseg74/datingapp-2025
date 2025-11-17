using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService(IConfiguration config, UserManager<AppUser> userManager) : ITokenService // The configuration file refers to the appsettings.json (or appsettings.Development.json)
{
  public async Task<string> CreateToken(AppUser user)
  {
    var tokenKey = config["TokenKey"] ?? throw new Exception("Cannot get token key");
    if (tokenKey.Length < 64)
    {
      throw new Exception("Your token key needs to be >= 64 characters");
    }
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)); // SymmetricSecurityKey means that the same key will be used to encrypt and decrypt. Certificates, for example, use AsymmetricSecurityKey which means a key for encryption and a different one for decryption (public key and private key)

    // Adding some information to the token in the form of claims
    var claims = new List<Claim>
    {
      new(ClaimTypes.Email, user.Email!),
      new(ClaimTypes.NameIdentifier, user.Id)
    };

    // Getting the user's roles so we will know what abilities to assign to the user
    var roles = await userManager.GetRolesAsync(user);
    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

    var tokenDescriptor = new SecurityTokenDescriptor
    { // Some properties that the token needs
      Subject = new ClaimsIdentity(claims),
      Expires = DateTime.UtcNow.AddMinutes(7),
      SigningCredentials = creds
    };

    var tokenHandler = new JwtSecurityTokenHandler(); // This is the class that will ceate the token based on the token descriptor
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
  }

  public string GenerateRefreshToken()
  {
    // Generating a new token
    // The randomBytes will contain a 64 bytes number
    var randomBytes = RandomNumberGenerator.GetBytes(64);
    return Convert.ToBase64String(randomBytes);
  }
}
