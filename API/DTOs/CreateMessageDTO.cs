namespace API.DTOs;

// A message DTO for creating a new message
public class CreateMessageDTO
{
  public required string RecipientId { get; set; }
  public required string Content { get; set; }
}
