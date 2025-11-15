import { inject, Injectable } from '@angular/core';
import { AccountService } from './account-service';
import { Observable, of } from 'rxjs';
import { LikesService } from './likes-service';

@Injectable({
  providedIn: 'root',
})
export class InitService {
  private accountService = inject(AccountService);
  private likeService = inject(LikesService);
  init() {
    const userString = localStorage.getItem('user');
    if (!userString) return of(null); // The of function wraps the parameter it gets in an observable
    const user = JSON.parse(userString);
    this.accountService.currentUser.set(user);
    // Since we are not using here the setCurrentUser from the accountService which also executes the getLikeIds function, we have to execute it here. Otherwise we will not see the likes in the match screen. Only the ones we activly set as like
    this.likeService.getLikeIds();

    return of(null);
  }
}
