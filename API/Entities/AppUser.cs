namespace API.Entities;

public class AppUser
{
  // Each field will represent a column in the DB. Any field with the name Id will automatically be defined as the primary key
  public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string DisplayName { get; set; }
    public required string Email { get; set; }
}
