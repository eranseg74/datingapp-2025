import { Component, effect, ElementRef, inject, OnInit, signal, ViewChild } from '@angular/core';
import { MessageService } from '../../../core/services/message-service';
import { MemberService } from '../../../core/services/member-service';
import { Message } from '../../../types/message';
import { DatePipe } from '@angular/common';
import { TimeAgoPipe } from '../../../core/pipes/time-ago-pipe';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-member-messages',
  imports: [DatePipe, TimeAgoPipe, FormsModule],
  templateUrl: './member-messages.html',
  styleUrl: './member-messages.css',
})
export class MemberMessages implements OnInit {
  // Note that all elements that come from the template are not yet defined in the constructor stage so we will need to handle this situation because here we are saying that it will never be null (by adding the ! mark)
  @ViewChild('messageEndRef') messageEndRef!: ElementRef;
  private messageService = inject(MessageService);
  // Note that the member service contains a member property which is a signal. Because we are injecting this service also to the member-details which is the details of the displayed member and not necessarily the current user, the data needed for the appropriate recipient or sender will come from this service
  private memberService = inject(MemberService);
  protected messages = signal<Message[]>([]);
  protected messageContent = '';

  constructor() {
    effect(() => {
      const currentMessages = this.messages();
      if (currentMessages.length > 0) {
        this.scrollToBottom();
      }
    });
  }

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages() {
    const memberId = this.memberService.member()?.id;
    if (memberId) {
      this.messageService.getMessageThread(memberId).subscribe({
        next: (messages) =>
          this.messages.set(
            messages.map((message) => ({
              ...message,
              currentUserSender: message.senderId !== memberId,
            }))
          ),
      });
    }
  }

  sendMessage() {
    const recipientId = this.memberService.member()?.id;
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
  }

  scrollToBottom() {
    setTimeout(() => {
      if (this.messageEndRef) {
        this.messageEndRef.nativeElement.scrollIntoView({ behavior: 'smooth' });
      }
    });
  }
}
