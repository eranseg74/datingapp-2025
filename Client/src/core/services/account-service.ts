import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { LoginCreds, RegisterCreds, User } from '../../types/user';
import { tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { LikesService } from './likes-service';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private http = inject(HttpClient);
  // Injections can only occur one way. If the LikeService is injected here it will not be possible to inject the account service in the like service
  private likeService = inject(LikesService);
  currentUser = signal<User | null>(null); // Using union (|) otherwise we will get an error that User cannot be null
  private baseUrl = environment.apiUrl;

  register(creds: RegisterCreds) {
    return this.http.post<User>(this.baseUrl + 'account/register', creds).pipe(
      tap((user) => {
        // tap allows us to perform actions based on the data received from the http response without changing the received data itself
        if (user) {
          this.setCurrentUser(user);
        }
      })
    );
  }

  login(creds: LoginCreds) {
    return this.http.post<User>(this.baseUrl + 'account/login', creds).pipe(
      tap((user) => {
        // tap allows us to perform actions based on the data received from the http response without changing the received data itself
        if (user) {
          this.setCurrentUser(user);
        }
      })
    );
  }

  setCurrentUser(user: User) {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.set(user);
    this.likeService.getLikeIds(); // That will populate the signal in any of the components that uses the like feature
  }

  logout() {
    localStorage.removeItem('user');
    localStorage.removeItem('filters');
    this.likeService.clearLikeIds();
    this.currentUser.set(null);
  }
}
