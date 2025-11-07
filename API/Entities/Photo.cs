using System.Text.Json.Serialization;

namespace API.Entities;

public class Photo
{
  public int Id { get; set; }
  public required string Url { get; set; }
  public string? PublicId { get; set; }

  // Navigation property
  [JsonIgnore]
  public Member Member { get; set; } = null!; // Do not declare it as required because the entity framework will have problem with that. The right way is to initialize it as null and assert to the compiler that ths Member property will never be null at runtime.
  public string MemberId { get; set; } = null!;
  // The MemberId was added after the first migration. Any changes in the entities will require running a migration again!!! Otherwise the changes will not apply in the DB and the program will fail because of mismatches
}
