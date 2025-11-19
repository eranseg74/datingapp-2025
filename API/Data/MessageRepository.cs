using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessageRepository(AppDbContext context) : IMessageRepository
{
  public void AddGroup(Group group)
  {
    context.Groups.Add(group);
  }

  public void AddMessage(Message message)
  {
    context.Messages.Add(message);
  }

  public void DeleteMessage(Message message)
  {
    context.Messages.Remove(message);
  }

  public async Task<Connection?> GetConnection(string connectionId)
  {
    return await context.Connections.FindAsync(connectionId);
  }

  // This will return the group in which the connectionId is inside of
  public async Task<Group?> GetGroupForConnection(string connectionId)
  {
    return await context.Groups
      .Include(x => x.Connections) // Returns all the ICollection<Connection> (there is one for each group)
      .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId)) // All the groups that contain the connectionId
      .FirstOrDefaultAsync(); // The first group from the result
  }

  public async Task<Message?> GetMessage(string messageId)
  {
    return await context.Messages.FindAsync(messageId);
  }

  public async Task<Group?> GetMessageGroup(string groupName)
  {
    return await context.Groups.Include(x => x.Connections).FirstOrDefaultAsync(X => X.Name == groupName);
  }

  public async Task<PaginatedResult<MessageDTO>> GetMessagesForMember(MessageParams messageParams)
  {
    // The OrderByDescending returns an IOrderedQueryable and we want IQueryable so we execute the AsQueryable function
    var query = context.Messages.OrderByDescending(x => x.MessageSent).AsQueryable();

    query = messageParams.Container switch
    {
      "Outbox" => query.Where(x => x.SenderId == messageParams.MemberId && x.SenderDeleted == false),
      _ => query.Where(x => x.RecipientId == messageParams.MemberId && x.RecipientDeleted == false) // Default
    };

    // Projecting to the MessageDTO. Projecting is done using the Select function
    var messageQuery = query.Select(MessageExtensions.ToDTOProjection());
    // The following is an implementation without the extension
    // var messageQuery = query.Select(
    //   message => new MessageDTO
    //   {
    //     Id = message.Id,
    //     SenderId = message.SenderId,
    //     SenderDisplayName = message.Sender.DisplayName,
    //     SenderImageUrl = message.Sender.ImageUrl,
    //     RecipientId = message.RecipientId,
    //     RecipientDisplayName = message.Recipient.DisplayName,
    //     RecipientImageUrl = message.Recipient.ImageUrl,
    //     Content = message.Content,
    //     DateRead = message.DateRead,
    //     MessageSent = message.MessageSent
    //   }
    // );
    return await PaginationHelper.CreateAsync(messageQuery, messageParams.PageNumber, messageParams.PageSize);
  }

  public async Task<IReadOnlyList<MessageDTO>> GetMessageThread(string currentMemberId, string recipientId)
  {
    // The recipientId is the other person in the conversation
    // Checking if the recipient of the message is the current user and the sender is the other person
    // Also checking the opposite, if the recipient is the other person and the sender is the current user
    // This way we get all messages between the two users
    // Additionally, we are ordering the messages by the date they were sent
    // Finally, we are projecting the messages to the MessageDTO
    // Before returning the messages, we are updating the DateRead property of any unread messages to the current date and time
    await context.Messages
      .Where(m => m.RecipientId == currentMemberId && m.SenderId == recipientId && m.DateRead == null)
      .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.DateRead, DateTime.UtcNow));


    var messages = await context.Messages
      .Where(m =>
        (m.RecipientId == currentMemberId && m.RecipientDeleted == false && m.SenderId == recipientId) ||
        (m.RecipientId == recipientId && m.SenderDeleted == false && m.SenderId == currentMemberId))
      .OrderBy(m => m.MessageSent)
      .Select(MessageExtensions.ToDTOProjection())
      .ToListAsync();
    return messages;
  }

  public async Task RemoveConnection(string connectionId)
  {
    // The ExecuteDeleteAsync will simply remove the result from the database
    await context.Connections.Where(x => x.ConnectionId == connectionId).ExecuteDeleteAsync();
  }

  public async Task<bool> SaveAllAsync()
  {
    return await context.SaveChangesAsync() > 0;
  }
}
