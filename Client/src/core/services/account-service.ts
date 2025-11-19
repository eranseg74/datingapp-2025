import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { LoginCreds, RegisterCreds, User } from '../../types/user';
import { tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { LikesService } from './likes-service';
import { PresenceService } from './presence-service';
import { HubConnectionState } from '@microsoft/signalr';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private http = inject(HttpClient);
  // Injections can only occur one way. If the LikeService is injected here it will not be possible to inject the account service in the like service
  private likeService = inject(LikesService);
  // Injecting the SignalR's
  private presenceService = inject(PresenceService);
  currentUser = signal<User | null>(null); // Using union (|) otherwise we will get an error that User cannot be null
  private baseUrl = environment.apiUrl;

  register(creds: RegisterCreds) {
    return this.http
      .post<User>(this.baseUrl + 'account/register', creds, { withCredentials: true }) // Adding { withCredentials: true } will allow to get the refreshed token from the cookie
      .pipe(
        tap((user) => {
          // tap allows us to perform actions based on the data received from the http response without changing the received data itself
          if (user) {
            this.setCurrentUser(user);
            this.startTokenRefreshInterval();
          }
        })
      );
  }

  login(creds: LoginCreds) {
    return this.http
      .post<User>(this.baseUrl + 'account/login', creds, { withCredentials: true })
      .pipe(
        tap((user) => {
          // tap allows us to perform actions based on the data received from the http response without changing the received data itself
          if (user) {
            this.setCurrentUser(user);
            this.startTokenRefreshInterval();
          }
        })
      );
  }

  refreshToken() {
    return this.http.post<User>(
      this.baseUrl + 'account/refresh-token',
      {},
      { withCredentials: true }
    );
  }

  startTokenRefreshInterval() {
    setInterval(() => {
      this.http
        .post<User>(this.baseUrl + 'account/refresh-token', {}, { withCredentials: true })
        .subscribe({
          next: (user) => {
            this.setCurrentUser(user);
          },
          error: () => {
            this.logout();
          },
        });
    }, 5 * 60 * 1000);
  }

  setCurrentUser(user: User) {
    user.roles = this.getRolesFromToken(user);
    // localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.set(user);
    this.likeService.getLikeIds(); // That will populate the signal in any of the components that uses the like feature
    if (this.presenceService.hubConnection?.state !== HubConnectionState.Connected) {
      this.presenceService.createHubConnection(user);
    }
  }

  logout() {
    // localStorage.removeItem('user');
    localStorage.removeItem('filters');
    this.likeService.clearLikeIds();
    this.currentUser.set(null);
    this.presenceService.stopHubConnection();
  }

  private getRolesFromToken(user: User): string[] {
    // The token is divided into 3 section by a dot. The first part specifies the properties of the token such as expiration date and so on. The second part contains the payload which is all the other properties we loaded to the token. The third part is the encrypted part that can only be decrypted by the security key. Since the roles are in the payload we can extract them from there by taking the second argument (the payload) and decrypt it using the nstive function - atob, that is provided by JavaScript.
    const payload = user.token.split('.')[1];
    const decoded = atob(payload);
    const jsonPayload = JSON.parse(decoded); // Converting the string to a json format
    return Array.isArray(jsonPayload.role) ? jsonPayload.role : [jsonPayload.role]; // Validating that we alwyas return an array
  }
}
