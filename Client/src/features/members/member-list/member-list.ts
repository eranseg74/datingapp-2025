import { Component, inject } from '@angular/core';
import { MemberService } from '../../../core/services/member-service';
import { Observable } from 'rxjs';
import { Member } from '../../../types/member';
import { AsyncPipe } from '@angular/common';
import { MemberCard } from '../member-card/member-card';

@Component({
  selector: 'app-member-list',
  imports: [AsyncPipe, MemberCard], // The AsyncPipe will handle all the subscriptions to the observable, and once the component is removed, it will handle all the required unsubscriptions. Can be used not just for http request but on any observables
  templateUrl: './member-list.html',
  styleUrl: './member-list.css',
})
export class MemberList {
  // Using the async pipe instead of subscribing to an observable
  private memberService = inject(MemberService);
  protected members$: Observable<Member[]>; // The $ is a sign for observable which is what the getMembers function below returns

  constructor() {
    this.members$ = this.memberService.getMembers();
  }
}
