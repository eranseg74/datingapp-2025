import { inject, Injectable } from '@angular/core';
import { AccountService } from './account-service';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class InitService {
  private accountService = inject(AccountService);
  init() {
    const userString = localStorage.getItem('user');
    if (!userString) return of(null); // The of function wraps the parameter it gets in an observable
    const user = JSON.parse(userString);
    this.accountService.currentUser.set(user);

    return of(null);
  }
}
