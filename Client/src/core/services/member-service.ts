import { HttpClient /* HttpHeaders */, HttpParams } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { EditableMember, Member, MemberParams, Photo } from '../../types/member';
import { tap } from 'rxjs';
import { PaginatedResult } from '../../types/pagination';
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

  getMembers(memberParams: MemberParams) {
    let params = new HttpParams(); // The HttpParams (from angular/common/http) allows passing arguments as query string parameters to the API
    params = params.append('pageNumber', memberParams.pageNumber);
    params = params.append('pageSize', memberParams.pageSize);
    params = params.append('minAge', memberParams.minAge);
    params = params.append('maxAge', memberParams.maxAge);
    params = params.append('orderBy', memberParams.orderBy);
    // Checking if we have a gender. If so it will be added to the member parameters
    if (memberParams.gender) params = params.append('gender', memberParams.gender);
    // return an Observable of the HttpResponse, with a response body in the requested type
    // No need for the call with the options because of the jwtInterceptor.
    // return this.http.get<Member[]>(this.baseUrl + 'members', this.getHttpOptions());

    // Because the key and value names in the second parameter (the params) we can write it as defined. It is the same as writing -> { params: params }. If it was not the same we would have to specify both the key and the value explicitly.
    // Here we also persist the defined filters to the local storage so they will be available for the user after refresh. The filters will be removed from the local storage on logout
    return this.http.get<PaginatedResult<Member>>(this.baseUrl + 'members', { params }).pipe(
      tap(() => {
        localStorage.setItem('filters', JSON.stringify(memberParams));
      })
    );
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

  uploadPhoto(file: File) {
    // Uplading a file requires a form data and not JSON (remember postman)
    const formData = new FormData();
    formData.append('file', file); // The name has to match the key
    return this.http.post<Photo>(this.baseUrl + 'members/add-photo', formData);
  }

  setMainPhoto(photo: Photo) {
    // Because it is a put request we have to specify the put proerties as an object in the second parameter. If no properties are required - add an empty object
    return this.http.put(this.baseUrl + 'members/set-main-photo/' + photo.id, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + 'members/delete-photo/' + photoId);
  }
}
