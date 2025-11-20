import {
  Component,
  effect,
  ElementRef,
  inject,
  model,
  OnDestroy,
  OnInit,
  signal,
  ViewChild,
} from '@angular/core';
import { MessageService } from '../../../core/services/message-service';
import { MemberService } from '../../../core/services/member-service';
import { Message } from '../../../types/message';
import { DatePipe } from '@angular/common';
import { TimeAgoPipe } from '../../../core/pipes/time-ago-pipe';
import { FormsModule } from '@angular/forms';
import { PresenceService } from '../../../core/services/presence-service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-member-messages',
  imports: [DatePipe, TimeAgoPipe, FormsModule],
  templateUrl: './member-messages.html',
  styleUrl: './member-messages.css',
})
export class MemberMessages implements OnInit, OnDestroy {
  // Note that all elements that come from the template are not yet defined in the constructor stage so we will need to handle this situation because here we are saying that it will never be null (by adding the ! mark)
  @ViewChild('messageEndRef') messageEndRef!: ElementRef;
  protected messageService = inject(MessageService);
  // Note that the member service contains a member property which is a signal. Because we are injecting this service also to the member-details which is the details of the displayed member and not necessarily the current user, the data needed for the appropriate recipient or sender will come from this service
  private memberService = inject(MemberService);
  protected presenceService = inject(PresenceService);
  // We need the route because we need an access to the route parameters in order to get the other user Id from the query
  private route = inject(ActivatedRoute);
  // protected messages = signal<Message[]>([]); // The messages will come from the messages service that implements the SignalR
  protected messageContent = model('');

  constructor() {
    effect(() => {
      const currentMessages = this.messageService.messageThread();
      if (currentMessages.length > 0) {
        this.scrollToBottom();
      }
    });
  }

  ngOnInit(): void {
    // We need to create the hub connection inside here because when this component is loaded, that is when we want to connect to the SignalR hub.
    // Getting the other user id from the route (url)
    this.route.parent?.paramMap.subscribe({
      next: (params) => {
        const otherUserId = params.get('id');
        if (!otherUserId) throw new Error('Cannot connect to hub');
        this.messageService.createHubConnection(otherUserId);
      },
    });
  }

  // Not using it anymore because we aere getting the messages from the hub
  // loadMessages() {
  //   const memberId = this.memberService.member()?.id;
  //   if (memberId) {
  //     this.messageService.getMessageThread(memberId).subscribe({
  //       next: (messages) =>
  //         this.messages.set(
  //           messages.map((message) => ({
  //             ...message,
  //             currentUserSender: message.senderId !== memberId,
  //           }))
  //         ),
  //     });
  //   }
  // }

  sendMessage() {
    const recipientId = this.memberService.member()?.id;
    // Not using the http to send messages so this code is no longer relevant. Since we are using SignalR we are getting the data as a promise and not Observable
    /*
    if (recipientId) {
      this.messageService.sendMessage(recipientId, this.messageContent).subscribe({
        next: (message) => {
          this.messages.update((messages) => {
            message.currentUserSender = true;
            return [...messages, message];
          });
          this.messageContent = '';
        },
      });
    }
    */
    // Using SignalR to send the message. If we do not have recipientId or message content we will not send the message
    if (!recipientId || !this.messageContent()) return;
    // After sending the message reset the message content so next time we will not get it again, concatenated to the new message
    this.messageService.sendMessage(recipientId, this.messageContent())?.then(() => {
      this.messageContent.set('');
    });
  }

  scrollToBottom() {
    setTimeout(() => {
      if (this.messageEndRef) {
        this.messageEndRef.nativeElement.scrollIntoView({ behavior: 'smooth' });
      }
    });
  }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }
}
