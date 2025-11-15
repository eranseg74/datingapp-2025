using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Entities;

// When seeding members from a JSON file we need to make sure that all the properties spelling are identical between the Member class and the JSON file that contains all the members data. Otherwise the seeding will not work
public class Member
{
  public string Id { get; set; } = null!; // The exclamation mark means that we are asserting to the compiler that this property will not be null at runtime
  public DateOnly DateOfBirth { get; set; }
  public string? ImageUrl { get; set; }
  public required string DisplayName { get; set; }
  public DateTime Created { get; set; } = DateTime.UtcNow;
  public DateTime LastActive { get; set; } = DateTime.UtcNow;
  public required string Gender { get; set; }
  public string? Description { get; set; }
  public required string City { get; set; }
  public required string Country { get; set; }

  // Navigation property - meaning that we will be able to navigate from the Member to the User because of the established relation between them
  [JsonIgnore] // This will ignore these fields when we serialize the response into JSON by the Api Controller
  public List<Photo> Photos { get; set; } = [];

  // A list of all the members that liked the current member. Also setting the JsonIgnore attribute to indicate that this property should not be passed when we return a list of members
  [JsonIgnore]
  public List<MemberLike> LikedByMembers { get; set; } = [];

  // A list of all the members that the current member liked
  [JsonIgnore]
  public List<MemberLike> LikedMembers { get; set; } = [];

  [JsonIgnore]
  // The ForeignKey attribute defines the relation between the Member and the AppUser classes
  [ForeignKey(nameof(Id))]
  public AppUser User { get; set; } = null!;
}
