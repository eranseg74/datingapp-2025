import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { ToastService } from './toast-service';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { User } from '../../types/user';
import { Message } from '../../types/message';

@Injectable({
  providedIn: 'root',
})
export class PresenceService {
  private hubUrl = environment.hubUrl;
  private toast = inject(ToastService);
  hubConnection?: HubConnection;
  onlineUsers = signal<string[]>([]);

  createHubConnection(user: User) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'presence', {
        accessTokenFactory: () => user.token,
      })
      .withAutomaticReconnect()
      .build();

    // After creating the connection - Starting the connection. The start function returns a promise that resolves when the connection is successfully established. or rejects with an error
    this.hubConnection.start().catch((error) => console.log(error));

    this.hubConnection.on('UserOnline', (userId) => {
      this.onlineUsers.update((users) => [...users, userId]);
    });
    this.hubConnection.on('UserOffline', (userId) => {
      this.onlineUsers.update((users) => users.filter((x) => x !== userId));
    });
    this.hubConnection.on('GetOnlineUsers', (userIds) => {
      // This will send an updated list of online users whenever a user logs in or out to or from the application
      this.onlineUsers.set(userIds);
    });

    this.hubConnection.on('NewMessageReceived', (message: Message) => {
      this.toast.info(
        message.senderDisplayName + ' has sent you a new message',
        10000,
        message.senderImageUrl,
        `/members/${message.senderId}/messages`
      );
    });
  }

  stopHubConnection() {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      this.hubConnection.stop().catch((error) => console.log(error));
    }
  }
}
