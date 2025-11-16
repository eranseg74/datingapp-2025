using System.Linq.Expressions;
using API.DTOs;
using API.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace API.Extensions;

public static class MessageExtensions
{
  public static MessageDTO ToDTO(this Message message)
  {
    return new MessageDTO
    {
      Id = message.Id,
      SenderId = message.SenderId,
      SenderDisplayName = message.Sender.DisplayName,
      SenderImageUrl = message.Sender.ImageUrl,
      RecipientId = message.RecipientId,
      RecipientDisplayName = message.Recipient.DisplayName,
      RecipientImageUrl = message.Recipient.ImageUrl,
      Content = message.Content,
      DateRead = message.DateRead,
      MessageSent = message.MessageSent

    };
  }

  // The Select in the MessageRepository expects an expression so we cannot provide it with just the DTO because that will cause an error (500 - Object reference not set to an instance of an object). The way to overcome it is to implement the following expression that returns a function with the MessageDTO. This expression will be passed to the Select function. The function here takes an object of type Message as an input and returns an object of type MessageDTO
  public static Expression<Func<Message, MessageDTO>> ToDTOProjection()
  {
    return message => new MessageDTO
    {
      Id = message.Id,
      SenderId = message.SenderId,
      SenderDisplayName = message.Sender.DisplayName,
      SenderImageUrl = message.Sender.ImageUrl,
      RecipientId = message.RecipientId,
      RecipientDisplayName = message.Recipient.DisplayName,
      RecipientImageUrl = message.Recipient.ImageUrl,
      Content = message.Content,
      DateRead = message.DateRead,
      MessageSent = message.MessageSent
    };
  }
}
