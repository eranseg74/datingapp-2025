import { inject, Injectable } from '@angular/core';
import { AccountService } from './account-service';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class InitService {
  private accountService = inject(AccountService);
  // private likeService = inject(LikesService);
  init() {
    // Not using the localStorage for the user any more
    /*
    const userString = localStorage.getItem('user');
    this.accountService.currentUser.set(user);
    if (!userString) return of(null); // The of function wraps the parameter it gets in an observable
    const user = JSON.parse(userString);
    */
    return this.accountService.refreshToken().pipe(
      tap((user) => {
        if (user) {
          // Need to get the roles like we are doing in the setCurrentUser function in the account-service. Instead we will use the setCurrnetUser function:
          // this.accountService.currentUser.set(user);
          // this.likeService.getLikeIds();
          this.accountService.setCurrentUser(user);
          this.accountService.startTokenRefreshInterval();
        }
      })
    );

    // Since we are not using here the setCurrentUser from the accountService which also executes the getLikeIds function, we have to execute it here. Otherwise we will not see the likes in the match screen. Only the ones we activly set as like
    // this.likeService.getLikeIds();

    // return of(null);
  }
}
