using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService(IConfiguration config) : ITokenService // The configuration file refers to the appsettings.json (or appsettings.Development.json)
{
  public string CreateToken(AppUser user)
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
      new(ClaimTypes.Email, user.Email),
      new(ClaimTypes.NameIdentifier, user.Id)
    };
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

    var tokenDescriptor = new SecurityTokenDescriptor
    { // Some properties that the token needs
      Subject = new ClaimsIdentity(claims),
      Expires = DateTime.UtcNow.AddDays(7),
      SigningCredentials = creds
    };

    var tokenHandler = new JwtSecurityTokenHandler(); // This is the class that will ceate the token based on the token descriptor
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
  }
}
