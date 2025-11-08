import { HttpClient /* HttpHeaders */ } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member, Photo } from '../../types/member';
// import { AccountService } from './account-service';

@Injectable({
  providedIn: 'root',
})
export class MemberService {
  private http = inject(HttpClient);
  // private accountService = inject(AccountService);
  private baseUrl = environment.apiUrl;

  getMembers() {
    // return an Observable of the HttpResponse, with a response body in the requested type
    // No need for the call with the options because of the jwtInterceptor.
    // return this.http.get<Member[]>(this.baseUrl + 'members', this.getHttpOptions());
    return this.http.get<Member[]>(this.baseUrl + 'members');
  }

  getMember(id: string) {
    // No need for the call with the options because of the jwtInterceptor.
    // return this.http.get<Member>(this.baseUrl + 'members/' + id, this.getHttpOptions());
    return this.http.get<Member>(this.baseUrl + 'members/' + id);
  }

  // No need for the call with the options because of the jwtInterceptor.
  // private getHttpOptions() {
  //   return {
  //     headers: new HttpHeaders({
  //       // The currentUser has parenthesis after the name because it is a signal
  //       Authorization: 'Bearer ' + this.accountService.currentUser()?.token, // The space after the 'Bearer' is important because there must be a space between the Bearer word and the token itself in the header
  //     }),
  //   };
  // }
  getMemberPhotos(id: string) {
    return this.http.get<Photo[]>(this.baseUrl + 'members/' + id + '/photos');
  }
}
