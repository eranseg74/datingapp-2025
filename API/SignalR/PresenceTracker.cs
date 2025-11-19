using System;
using System.Collections.Concurrent;

namespace API.SignalR;

// The purpose of the PresenceTracker is to track and retain them in memory the list of users that are connected to the SignalR hub so we need to add this as a service in the Program.cs
public class PresenceTracker
{
  // CuncurrentDictionary represents a thread-safe collection of key/value pairs that can be accessed by multiple threads concurrently. This is what we need in order to store who is connected to the application. The key will be the user Id and the value will be another ConcurrentDictionary because each user may have multiple connections (via different browswers, mobile phone, etc.). The key in the second ConcurrentDictionary will be the connection Id and the value will be a byte with no meaning just because we have to supply a value to the dictionary 
  private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> OnlineUsers = new();

  public Task UserConnected(string userId, string connectionId)
  {
    // The GetOrAdd will add the userId as key and a new empty ConcurrentDictionary. The GetOrAdd function will return the value which is the new empty ConcurrentDictionary to which we will add the connection Id and 0 (as byte). In the GetOrAdd, if there is a connection and there is another attempt to connect from a different source such as mobile, the created ConcurrentDictionary will be returned to which we will add the new connection
    var connections = OnlineUsers.GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>());
    connections.TryAdd(connectionId, 0);
    return Task.CompletedTask;
  }

  public Task UserDisconnected(string userId, string connectionId)
  {
    if (OnlineUsers.TryGetValue(userId, out var connections))
    {
      connections.TryRemove(connectionId, out _);
      if (connections.IsEmpty)
      {
        OnlineUsers.TryRemove(userId, out _);
      }
    }
    return Task.CompletedTask;
  }

  public Task<string[]> GetOnlineUsers()
  {
    // The FromResult creates a Task<TResult> that's completed successfully with the specified result and returns the successfully completed task. In this case the result will be a string array of the keys which are the users Id that are online so eventually a Task<string[]> is returned
    return Task.FromResult(OnlineUsers.Keys.OrderBy(k => k).ToArray());
  }

  // We want to get the connections for a user without having to create an object of PresenceTracker so we define the method as static
  public static Task<List<string>> GetConnectionsForUser(string userId)
  {
    if (OnlineUsers.TryGetValue(userId, out var connections))
    {
      return Task.FromResult(connections.Keys.ToList()); // List of the connections for the given userId
    }
    return Task.FromResult(new List<string>()); // If failed to get value return an empty list of strings
  }
}
