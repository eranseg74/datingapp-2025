import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { PaginatedResult } from '../../types/pagination';
import { Message } from '../../types/message';
import { AccountService } from './account-service';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  private baseUrl = environment.apiUrl;
  private hubUrl = environment.hubUrl;
  private http = inject(HttpClient);
  private accountService = inject(AccountService);
  private hubConnection?: HubConnection;
  // We need to store inside here ther meesage thread because we will use a signal inside the message service so when we will receive a message from the SignalR hub we can update the message thread here as a signal so the desired component can react to that change when we receive any message or update the messages here
  messageThread = signal<Message[]>([]);

  // Creating a hub connection
  createHubConnection(otherUserId: string) {
    // Creating a copy of the current user and storing it in the currentUser variable
    const currentUser = this.accountService.currentUser();
    if (!currentUser) return;
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'messages?userId=' + otherUserId, {
        // The query (userId) must match the query text defined in the MessageHub.cs file otherwise it will not work and we will get a hub exception that the other user was not found
        accessTokenFactory: () => currentUser.token,
      })
      .withAutomaticReconnect()
      .build();

    // Start the hub
    this.hubConnection.start().catch((error) => console.log(error));

    // Listen to the events from the SignalR
    this.hubConnection.on('ReceiveMessageThread', (messages: Message[]) => {
      this.messageThread.set(
        messages.map((message) => ({
          ...message,
          currentUserSender: message.senderId !== otherUserId,
        }))
      );
    });

    //
    this.hubConnection.on('NewMessage', (message: Message) => {
      message.currentUserSender = message.senderId === currentUser.id;
      this.messageThread.update((messages) => [...messages, message]);
    });
  }

  stopHubConnection() {
    // If the state is not connected there is nothing to stop. We are already disconnected
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      this.hubConnection?.stop().catch((error) => console.log(error));
    }
  }

  getMessages(container: string, pageNumber: number, pageSize: number) {
    let params = new HttpParams();

    params = params.append('pageNumber', pageNumber);
    params = params.append('pageSize', pageSize);
    params = params.append('container', container);

    return this.http.get<PaginatedResult<Message>>(this.baseUrl + 'messages', { params });
  }

  getMessageThread(memberId: string) {
    return this.http.get<Message[]>(this.baseUrl + 'messages/thread/' + memberId);
  }

  sendMessage(recipientId: string, content: string) {
    // Remember: because the name 'recipientId' and 'content' are the same as defined in the CreateMessageDTO and in the proerties that are passed to this method, we can write it in shortcut like this. Otherwise we would have to specify the source like in the DTO { recipientId: recIdDTO, content: cont }

    // return this.http.post<Message>(this.baseUrl + 'messages', { recipientId, content });
    // Using the hub connection to send messages between participants. The string in the first argument ('SendMessage') must match exactly the name of the function in the server (in the MessageHub.cs file)
    return this.hubConnection?.invoke('SendMessage', { recipientId, content });
  }

  deleteMessage(id: string) {
    return this.http.delete(this.baseUrl + 'messages/' + id);
  }
}
