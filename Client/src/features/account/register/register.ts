import { Component, inject, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RegisterCreds, User } from '../../../types/user';
import { AccountService } from '../../../core/services/account-service';

@Component({
  selector: 'app-register',
  imports: [FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private accountService = inject(AccountService);
  // membersFromHome = input.required<User[]>(); // This is an input signal
  cancelRegister = output<boolean>(); // It is possible to use the Output decorator but the signal approach her eis better and will most likely be widely used in the future. Here we are specifying that the cancelRegister will have a boolean type
  protected creds = {} as RegisterCreds;

  register() {
    this.accountService.register(this.creds).subscribe({
      next: (response) => {
        console.log(response);
        this.cancel();
      },
      error: (error) => console.log(error),
    });
  }

  cancel() {
    this.cancelRegister.emit(false); // This means that on cancel() execution the false value will be emitted to the output that is reachable in the parent element
  }
}
