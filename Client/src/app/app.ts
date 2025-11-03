import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-root',
  imports: [],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App implements OnInit {
  private http = inject(HttpClient);
  protected readonly title = signal('Dating app');
  protected members = signal<any>([]);
  // constructor(private http: HttpClient) {} // No need because of the injection above

  // Fetching the data will be here an not in the constructor which is too soon
  async ngOnInit() {
    // The get function returns an Observable which needs to be subscribed to
    // this.http.get('https://localhost:5001/api/members').subscribe({
    //   next: (response) => this.members.set(response),
    //   error: (error) => console.log(error),
    //   complete: () => console.log('Completed the http request'),

    // Option 2 - Use async await instead of Observables
    this.members.set(await this.getMembers());
  }

  async getMembers() {
    // Returning a promise
    try {
      return lastValueFrom(this.http.get('https://localhost:5001/api/members')); // Can be also firstValue from since there is only one argument which is the response
    } catch (error) {
      console.log(error);
      throw error;
    }
  }
}
