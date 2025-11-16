import { Component, inject, OnInit, signal } from '@angular/core';
import { MessageService } from '../../core/services/message-service';
import { PaginatedResult } from '../../types/pagination';
import { Message } from '../../types/message';
import { Paginator } from '../../shared/paginator/paginator';
import { TimeAgoPipe } from '../../core/pipes/time-ago-pipe';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-messages',
  imports: [Paginator, DatePipe, RouterLink],
  templateUrl: './messages.html',
  styleUrl: './messages.css',
})
export class Messages implements OnInit {
  private messageService = inject(MessageService);
  protected container = 'Inbox';
  // A utility container to overcome the delay when switching from inbox to outbox. The delay is because is only on the first time or after refresh because of the caching. When the cache is empty, an API call is executed and until the data comes from the server the image that is displayed is incorrect and changes after a second but still not a good UI experience. Since all the checks in the UI are done against the isInbox method which checks the fetchedContainer the data is synchronized and the delay of the photos is removed
  protected fetchedContainer = 'Inbox';
  protected pageNumber = 1;
  protected pageSize = 10;
  protected paginatedMessages = signal<PaginatedResult<Message> | null>(null);

  tabs = [
    { label: 'Inbox', value: 'Inbox' },
    { label: 'Outbox', value: 'Outbox' },
  ];

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages() {
    this.messageService.getMessages(this.container, this.pageNumber, this.pageSize).subscribe({
      next: (response) => {
        this.paginatedMessages.set(response);
        // Setting the fetchedContainer to the container value only after the data is received
        this.fetchedContainer = this.container;
      },
    });
  }

  deleteMessage(event: Event, id: string) {
    // Getting the event so we can stop propagation. We need it because currently whenever we click on a message we areredirected to its details. We want that whenever we click on the delete button we will not be redirected there
    event.stopPropagation();
    this.messageService.deleteMessage(id).subscribe({
      next: () => {
        // Recall that paginatedMessages is a signal of type PaginatedResult but after the assignment here the current type is PaginatedResult that holds a metadata and list of items
        const current = this.paginatedMessages();
        if (current?.items) {
          this.paginatedMessages.update((prev) => {
            // prev is the current data in the signal. The update method allows us to update the value of a signal based on its current value
            if (!prev) return null;
            const newItems = prev.items?.filter((x) => x.id !== id) || [];
            // Note that here we are not updating the metadata although it might be required because less messages might result to less total pages and for sure less totalcount which should be calculated and updated. In case of this implementation we need to set here a newMetadata, update the fields in it and return the new metadata and not the existing one as we are doing here.
            // Here is the more advanced approach:
            const newMetadata = prev.metadata
              ? {
                  ...prev.metadata,
                  totalCount: prev.metadata.totalCount - 1,
                  totalPages: Math.max(
                    1,
                    Math.ceil((prev.metadata.totalCount - 1) / prev.metadata.pageSize)
                  ),
                  currentPage: Math.min(
                    prev.metadata.currentPage,
                    Math.max(1, Math.ceil((prev.metadata.totalCount - 1) / prev.metadata.pageSize))
                  ),
                }
              : prev.metadata;
            return {
              items: newItems,
              metadata: newMetadata,
            };
          });
        }
      },
    });
  }

  get isInbox() {
    return this.fetchedContainer === 'Inbox';
  }

  setContainer(container: string) {
    this.container = container;
    // Returning to the first page when switching between containers
    this.pageNumber = 1;
    this.loadMessages();
  }

  // Because we are using pagination when displaying the messages we need the onPageChange (same implementation)
  onPageChange(event: { pageNumber: number; pageSize: number }) {
    this.pageNumber = event.pageNumber;
    this.pageSize = event.pageSize;
    this.loadMessages();
  }
}
