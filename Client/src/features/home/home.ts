import { Component, inject, Input, signal } from '@angular/core';
import { Register } from '../account/register/register';
import { User } from '../../types/user';
import { AccountService } from '../../core/services/account-service';

@Component({
  selector: 'app-home',
  imports: [Register],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  // @Input({ required: true }) membersFromApp: User[] = []; // Setting the required to true means that the template that conatins the app-home element must provide a list of users. Otherwise we would have to check here that we got something or else the array will always be empty
  protected registerMode = signal(false);
  protected accountService = inject(AccountService);

  showRegister(value: boolean) {
    this.registerMode.set(value);
  }
}
