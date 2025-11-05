import { Component, inject } from '@angular/core';
import { Nav } from '../layout/nav/nav';
import { Router, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [Nav, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected router = inject(Router); // This will give us a router functionallity such as the route's URL
  // private http = inject(HttpClient);
  // protected readonly title = signal('Dating app');
  // protected members = signal<User[]>([]);
  // constructor(private http: HttpClient) {} // No need because of the injection above

  // Fetching the data will be here an not in the constructor which is too soon
  // async ngOnInit() {
  //   // The get function returns an Observable which needs to be subscribed to
  //   // this.http.get('https://localhost:5001/api/members').subscribe({
  //   //   next: (response) => this.members.set(response),
  //   //   error: (error) => console.log(error),
  //   //   complete: () => console.log('Completed the http request'),

  //   // Option 2 - Use async await instead of Observables
  //   this.members.set(await this.getMembers());
  // }
}
