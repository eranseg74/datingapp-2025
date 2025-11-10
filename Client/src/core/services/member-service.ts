import { HttpClient /* HttpHeaders */ } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { EditableMember, Member, Photo } from '../../types/member';
import { tap } from 'rxjs';
// import { AccountService } from './account-service';

@Injectable({
  providedIn: 'root',
})
export class MemberService {
  private http = inject(HttpClient);
  // private accountService = inject(AccountService);
  private baseUrl = environment.apiUrl;
  editMode = signal(false);
  member = signal<Member | null>(null);

  getMembers() {
    // return an Observable of the HttpResponse, with a response body in the requested type
    // No need for the call with the options because of the jwtInterceptor.
    // return this.http.get<Member[]>(this.baseUrl + 'members', this.getHttpOptions());
    return this.http.get<Member[]>(this.baseUrl + 'members');
  }

  getMember(id: string) {
    // No need for the call with the options because of the jwtInterceptor.
    // return this.http.get<Member>(this.baseUrl + 'members/' + id, this.getHttpOptions());
    return this.http.get<Member>(this.baseUrl + 'members/' + id).pipe(
      tap((member) => {
        // In addition of returning the member we use the pipe to set the member (signal) so we can use it to update different parts of the member data in the UI so all of the member's properties in the profile and in the navigation bar can be updated immediately
        this.member.set(member);
      })
    );
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

  updateMember(member: EditableMember) {
    return this.http.put(this.baseUrl + 'members', member);
  }
}
