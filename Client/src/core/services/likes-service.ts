import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Member } from '../../types/member';
import { PaginatedResult, Pagination } from '../../types/pagination';

@Injectable({
  providedIn: 'root',
})
export class LikesService {
  private baseUrl = environment.apiUrl;
  private http = inject(HttpClient);
  likeIds = signal<string[]>([]);

  toggleLike(targetMemberId: string) {
    return this.http.post(`${this.baseUrl}likes/${targetMemberId}`, {}).subscribe({
      next: () => {
        // When clicking on the like icon, we check if the user likes the member. If so, we remove the like from the likeIds array and if not, we add it
        if (this.likeIds().includes(targetMemberId)) {
          this.likeIds.update((ids) => ids.filter((x) => x !== targetMemberId));
        } else {
          this.likeIds.update((ids) => [...ids, targetMemberId]);
        }
      },
    });
  }

  getLikes(predicate: string, pageNumber: number, pageSize: number) {
    let params = new HttpParams();
    params.append('pageNumber', pageNumber);
    params.append('pageSize', pageSize);
    params.append('predicate', predicate);
    return this.http.get<PaginatedResult<Member>>(`${this.baseUrl}likes`, { params });
  }

  // Subscribing to the respond so we will be able to update the likes on every login or on app initialization
  getLikeIds() {
    return this.http.get<string[]>(this.baseUrl + 'likes/list').subscribe({
      next: (ids) => this.likeIds.set(ids),
    });
  }

  // Can be used in logout
  clearLikeIds() {
    this.likeIds.set([]);
  }
}
