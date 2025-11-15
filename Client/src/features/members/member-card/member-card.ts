import { Component, computed, inject, input } from '@angular/core';
import { Member } from '../../../types/member';
import { RouterLink } from '@angular/router';
import { AgePipe } from '../../../core/pipes/age-pipe';
import { LikesService } from '../../../core/services/likes-service';
import { computeMsgId } from '@angular/compiler';

@Component({
  selector: 'app-member-card',
  imports: [RouterLink, AgePipe],
  templateUrl: './member-card.html',
  styleUrl: './member-card.css',
})
export class MemberCard {
  private likeService = inject(LikesService);
  member = input.required<Member>();
  // Using a computed property to see if the current user has liked the member in this member card
  protected hasLiked = computed(() => this.likeService.likeIds().includes(this.member().id));

  toggleLike(event: Event) {
    event.stopPropagation(); // This will prevent us from propagating to the member details component so when we click on the like icon we will not also be redirected to the details screen along with marking the member like icon
    this.likeService.toggleLike(this.member().id).subscribe({
      next: () => {
        // When clicking on the like icon, we check if the user likes the member. If so, we remove the like from the likeIds array and if not, we add it
        if (this.hasLiked()) {
          this.likeService.likeIds.update((ids) => ids.filter((x) => x !== this.member().id));
        } else {
          this.likeService.likeIds.update((ids) => [...ids, this.member().id]);
        }
      },
    });
  }
}
