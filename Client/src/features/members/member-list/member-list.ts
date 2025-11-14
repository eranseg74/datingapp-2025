import { Component, inject, OnInit, signal, ViewChild } from '@angular/core';
import { MemberService } from '../../../core/services/member-service';
import { Member, MemberParams } from '../../../types/member';
import { MemberCard } from '../member-card/member-card';
import { PaginatedResult } from '../../../types/pagination';
import { Paginator } from '../../../shared/paginator/paginator';
import { FilterModal } from '../filter-modal/filter-modal';

@Component({
  selector: 'app-member-list',
  imports: [MemberCard, Paginator, FilterModal], // The AsyncPipe will handle all the subscriptions to the observable, and once the component is removed, it will handle all the required unsubscriptions. Can be used not just for http request but on any observables
  templateUrl: './member-list.html',
  styleUrl: './member-list.css',
})
export class MemberList implements OnInit {
  // The filterModal is a component inside this component
  @ViewChild('filterModal') modal!: FilterModal;
  // Using the async pipe instead of subscribing to an observable
  private memberService = inject(MemberService);

  // Not using this approach in order to not use async pipe.
  // protected paginatedMembers$?: Observable<PaginatedResult<Member>>; // The $ is a sign for observable which is what the getMembers function below returns
  protected paginatedMembers = signal<PaginatedResult<Member> | null>(null);
  protected memberParams = new MemberParams();
  // The updatedParams will be used to update the UI. Not using the memberParams because it is attached to the template by two way binding (the ngModel) so if we change the filter but not submit it the displayed filters will be incorrect because we did not apply them
  private updatedParams = new MemberParams();

  constructor() {
    // Getting the filters from the local storage, in case they exist there
    const filters = localStorage.getItem('filters');
    if (filters) {
      this.memberParams = JSON.parse(filters);
      this.updatedParams = JSON.parse(filters);
    }
  }

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers() {
    this.memberService.getMembers(this.memberParams).subscribe({
      next: (result) => {
        this.paginatedMembers.set(result);
      },
    });
  }

  onPageChange(event: { pageNumber: number; pageSize: number }) {
    this.memberParams.pageNumber = event.pageNumber;
    this.memberParams.pageSize = event.pageSize;
    this.loadMembers();
  }

  openModal() {
    // Because of the ViewChild that gets the #filterModal which is of type FilterModal as a modal. In the FilterModal class we defined the open modal so we use it here
    this.modal.open();
  }

  onClose() {
    console.log('Modal closed');
  }

  onFilterChange(data: MemberParams) {
    this.memberParams = { ...data };
    this.updatedParams = { ...data };
    this.loadMembers();
  }

  resetFilters() {
    this.memberParams = new MemberParams();
    this.updatedParams = new MemberParams();
    this.loadMembers();
  }

  get displayMessage(): string {
    const defaultParams = new MemberParams();

    const filters: string[] = [];

    if (this.updatedParams.gender) {
      filters.push(this.updatedParams.gender + 's');
    } else {
      filters.push('Males, Females');
    }

    if (
      this.updatedParams.minAge !== defaultParams.minAge ||
      this.updatedParams.maxAge !== defaultParams.maxAge
    ) {
      filters.push(` ages ${this.updatedParams.minAge}-${this.updatedParams.maxAge}`);
    }
    filters.push(
      this.updatedParams.orderBy === 'lastActive' ? 'Recently active' : 'Newest members'
    );
    return filters.length > 0 ? `Selected: ${filters.join('  | ')}` : 'All members';
  }
}
