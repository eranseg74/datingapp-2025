using System;
using System.Security.Claims;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

// Afetr creating the Hub we need to configure it in the Program.ts main file
[Authorize]
public class PresenceHub(PresenceTracker presenceTracker) : Hub
{
  public override async Task OnConnectedAsync()
  {
    await presenceTracker.UserConnected(this.GetUserId(), Context.ConnectionId);
    // The first argument ("UserOnline") is the name of the method that the other users will listen to when the user starts to be online
    await Clients.Others.SendAsync("UserOnline", GetUserId());
    // Getting the current users
    var currentUsers = await presenceTracker.GetOnlineUsers();
    // Updating all the clients about who is online
    await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
  }

  public override async Task OnDisconnectedAsync(Exception? exception)
  {
    await presenceTracker.UserDisconnected(GetUserId(), Context.ConnectionId);
    await Clients.Others.SendAsync("UserOffline", GetUserId());

    // Getting the current users
    var currentUsers = await presenceTracker.GetOnlineUsers();
    // Updating all the clients about who is online
    await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);

    await base.OnDisconnectedAsync(exception); // Passing the exxception in case of disconnection
  }

  private string GetUserId()
  {
    return Context.User?.GetMemberId() ?? throw new HubException("Cannot getmember Id");
  }
}
