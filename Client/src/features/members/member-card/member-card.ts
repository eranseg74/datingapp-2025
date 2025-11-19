import { Component, computed, inject, input } from '@angular/core';
import { Member } from '../../../types/member';
import { RouterLink } from '@angular/router';
import { AgePipe } from '../../../core/pipes/age-pipe';
import { LikesService } from '../../../core/services/likes-service';
import { computeMsgId } from '@angular/compiler';
import { PresenceService } from '../../../core/services/presence-service';

@Component({
  selector: 'app-member-card',
  imports: [RouterLink, AgePipe],
  templateUrl: './member-card.html',
  styleUrl: './member-card.css',
})
export class MemberCard {
  private likesService = inject(LikesService);
  private presenceService = inject(PresenceService);
  member = input.required<Member>();
  // Using a computed property to see if the current user has liked the member in this member card
  protected hasLiked = computed(() => this.likesService.likeIds().includes(this.member().id));
  // The isOnline will hold a boolean (wrapped in signal) that will specify if a member is online. Note that each card holds a different member so eventually we will get on each card an indication who is online
  protected isOnline = computed(() =>
    this.presenceService.onlineUsers().includes(this.member().id)
  );

  toggleLike(event: Event) {
    event.stopPropagation(); // This will prevent us from propagating to the member details component so when we click on the like icon we will not also be redirected to the details screen along with marking the member like icon
    this.likesService.toggleLike(this.member().id);
  }
}
