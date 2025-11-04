using System;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDTO
{
  // It is important that the properties name will exacly match the key of the properties that were sent by the client.
  // The required will only handle null! If the request will contain these keys with empty string a new user will be created with no name, email or password. Also, if a field is missing we will get a generic error message that does not specify which field is missing but just that there is something wrong with the RegisterDTO so a new method is needed here
  [Required]
  public string DisplayName { get; set; } = "";
  [Required]
  [EmailAddress]
  public string Email { get; set; } = "";
  [Required]
  [MinLength(4)]
  public string Password { get; set; } = "";
}
