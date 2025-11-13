import { Component, inject, output, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { RegisterCreds } from '../../../types/user';
import { JsonPipe } from '@angular/common';
import { TextInput } from '../../../shared/text-input/text-input';
import { AccountService } from '../../../core/services/account-service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, TextInput],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private accountService = inject(AccountService);
  private router = inject(Router); // In order to redirect users after registration
  private formBuilder = inject(FormBuilder);
  // membersFromHome = input.required<User[]>(); // This is an input signal
  cancelRegister = output<boolean>(); // It is possible to use the Output decorator but the signal approach her eis better and will most likely be widely used in the future. Here we are specifying that the cancelRegister will have a boolean type
  protected creds = {} as RegisterCreds;
  protected credentialsForm: FormGroup;
  protected profileForm: FormGroup;
  protected currentStep = signal(1);
  // This will be used to display all the validation errors that come from the server to the client, to be displayed
  protected validationErrors = signal<string[]>([]);

  constructor() {
    //this.initializeForm(); // This will not work. Angular will require that we set the registerForm with an initial value. Only executing the implicit code in the constructor will work, or initializing the registerForm with an empty object like -> protected registerForm: FormGroup = new FormGroup({})
    // Instead of defining each field we will group them using the formBuilder:
    /* WITHOUT THE FORM BUILDER
    this.registerForm = new FormGroup({
      email: new FormControl('', [Validators.required, Validators.email]),
      displayName: new FormControl('', [Validators.required]),
      password: new FormControl('', [
        Validators.required,
        Validators.minLength(4),
        Validators.maxLength(8),
      ]),
      confirmPassword: new FormControl('', [Validators.required, this.matchValues('password')]),
    }); */
    // Using the FORM BUILDER:
    this.credentialsForm = this.formBuilder.group({
      email: new FormControl('', [Validators.required, Validators.email]),
      displayName: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['', [Validators.required, this.matchValues('password')]],
    });

    this.profileForm = this.formBuilder.group({
      gender: ['male', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
    });

    // The following code means that if the value in the password field changes, the confirm password will be recalculated and run all its validators. We need this behavior, otherwise, after a successful match, if we change the value in the password the mismatch will not appear
    this.credentialsForm.controls['password'].valueChanges.subscribe(() => {
      this.credentialsForm.controls['confirmPassword'].updateValueAndValidity();
    });
  }

  // ngOnInit(): void {
  //   this.initializeForm();
  // }

  // initializeForm() {
  //   this.registerForm = new FormGroup({
  //     email: new FormControl('', [Validators.required, Validators.email]),
  //     displayName: new FormControl('', [Validators.required]),
  //     password: new FormControl('', [
  //       Validators.required,
  //       Validators.minLength(4),
  //       Validators.maxLength(8),
  //     ]),
  //     confirmPassword: new FormControl('', [Validators.required, this.matchValues('password')]),
  //   });
  //   // The following code means that if the value in the password field changes, the confirm password will be recalculated and run all its validators. We need this behavior, otherwise, after a successful match, if we change the value in the password the mismatch will not appear
  //   this.registerForm.controls['password'].valueChanges.subscribe(() => {
  //     this.registerForm.controls['confirmPassword'].updateValueAndValidity();
  //   });
  // }

  // A function used for custom validation
  matchValues(matchTo: string): ValidatorFn {
    // All validators are functions such as the required or email validators. This is why the custom validator must also be of type ValidatorFn so we will be able to use it as a validator.
    return (control: AbstractControl): ValidationErrors | null => {
      // If valdition fails - return errors. Otherwise return null
      // All input fields in angular derive from the AbstractControl class
      // Because we want to match the field to other field we need the parent that holds both fields which is the FormGroup (control.parent)
      const parent = control.parent;
      if (!parent) return null;
      const matchValue = parent.get(matchTo)?.value;
      // If the passwords match we will return null. Otherwise, we will return the error that we check for when we want to display the error that failed
      return control.value === matchValue ? null : { passwordMismatch: true };
    };
  }

  nextStep() {
    if (this.credentialsForm.valid) {
      this.currentStep.update((prevStep) => prevStep + 1);
    }
  }

  prevStep() {
    this.currentStep.update((prevStep) => prevStep - 1);
  }

  getMaxDate() {
    const today = new Date();
    today.setFullYear(today.getFullYear() - 18);
    return today.toISOString().split('T')[0]; // The T splits between the date and the time. The split will give an array with 2 values, the first one will be the date (day, month, year) and the second one will be the time (hours, minutes, seconds). We are interested in the first element - the date
  }

  register() {
    if (this.profileForm.valid && this.credentialsForm.valid) {
      const formData = { ...this.credentialsForm.value, ...this.profileForm.value };
      this.accountService.register(formData).subscribe({
        next: () => {
          this.router.navigateByUrl('/members');
        },
        error: (error) => {
          console.log(error);
          this.validationErrors.set(error);
        },
      });
    }
  }

  cancel() {
    this.cancelRegister.emit(false); // This means that on cancel() execution the false value will be emitted to the output that is reachable in the parent element
  }
}
