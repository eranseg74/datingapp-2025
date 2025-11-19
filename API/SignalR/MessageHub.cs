using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace API.SignalR;

[Authorize]
// IHubContext<PresenceHub> presenceHub gives us access to the functionality of the PresenceHub. We need this in order to check if a user is connected to the hub. If not but he is online, and he gets a message we will notify him with a toast that he received a message
public class MessageHub(IUnitOfWork unitOfWork,
                        IHubContext<PresenceHub> presenceHub) : Hub
{
  // Thid function enables the users in a chat to receive messages between them. Not send messages to each other
  public override async Task OnConnectedAsync()
  {
    var httpContext = Context.GetHttpContext(); // This is where the negotiation takes place. This is an HTTP request to set up the SignalR connection
    // Getting the Id of the other user and throwing an exception if is null
    var otherUser = httpContext?.Request?.Query["userId"].ToString() ?? throw new HubException("Other user not found");
    // Creating a group to ensure that only the two users that are conducting the chat can see each others messages
    var groupName = GetGroupName(GetUserId(), otherUser);
    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    await AddToGroup(groupName); // Adding the group to the database

    var messages = await unitOfWork.MessageRepository.GetMessageThread(GetUserId(), otherUser);

    await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
  }

  // A function to send message in a chat
  public async Task SendMessage(CreateMessageDTO createMessageDTO)
  {
    var sender = await unitOfWork.MemberRepository.GetMembeByIdAsync(GetUserId());
    var recipient = await unitOfWork.MemberRepository.GetMembeByIdAsync(createMessageDTO.RecipientId);

    if (recipient == null || sender == null || sender.Id == createMessageDTO.RecipientId)
    {
      throw new HubException("Cannot send message");
    }
    var message = new Message
    {
      SenderId = sender.Id,
      RecipientId = recipient.Id,
      Content = createMessageDTO.Content
    };

    var groupName = GetGroupName(sender.Id, recipient.Id);
    var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);
    var userInGroup = group != null && group.Connections.Any(x => x.UserId == message.RecipientId);

    if (userInGroup)
    {
      message.DateRead = DateTime.UtcNow;
    }

    unitOfWork.MessageRepository.AddMessage(message);

    if (await unitOfWork.Complete())
    {
      await Clients.Group(groupName).SendAsync("NewMessage", message.ToDTO());
      var connections = await PresenceTracker.GetConnectionsForUser(recipient.Id);
      if (connections != null && connections.Count > 0 && !userInGroup)
      {
        await presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", message.ToDTO());
      }
    }
  }

  // On disconnect the disconnected user will automatically be removed from the group. No need to implement this
  public override async Task OnDisconnectedAsync(Exception? exception)
  {
    await unitOfWork.MessageRepository.RemoveConnection(Context.ConnectionId);
    await base.OnDisconnectedAsync(exception);
  }

  private async Task<bool> AddToGroup(string groupName)
  {
    var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);
    var connection = new Connection(Context.ConnectionId, GetUserId());

    if (group == null)
    {
      group = new Group(groupName);
      unitOfWork.MessageRepository.AddGroup(group);
    }
    group.Connections.Add(connection);
    return await unitOfWork.Complete();
  }

  private static string GetGroupName(string? caller, string? other)
  {
    // We want to make sure that the group name will always be the same no matter the order of the users id's in this function. To do so we are creating a comparison function that will compare between the two ids and return a string that concatenates the ids in an alphabetically order. So no matter if we call this function with the other first and then caller or caller first and then the other, the result will always be the same
    var stringCompare = string.CompareOrdinal(caller, other) < 0;
    return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
  }

  // A utility method to fetch the user Id
  private string GetUserId()
  {
    return Context.User?.GetMemberId() ?? throw new HubException("Cannot get member id");
  }
}
