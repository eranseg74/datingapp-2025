import { Component, input, Self } from '@angular/core';
import { ControlValueAccessor, FormControl, NgControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-text-input',
  imports: [ReactiveFormsModule],
  templateUrl: './text-input.html',
  styleUrl: './text-input.css',
})
export class TextInput implements ControlValueAccessor {
  label = input<string>('');
  type = input<string>('text');
  maxDate = input<string>('');

  // Injecting NgControl to the class. In the constructor we are saying that the TextInput is of type ControlValueAccessor and we are assigning it to the ngControl value accessor
  // Self() is a dependency injection modifier - This means that the injected dependency is relevant only to this element and not to the other elements in the hierarchy. Important!!! - If we do not specify it as Self, angular will search in the tree hierarchy for other instances that use the NgControl and try to reuse this injection there. This way we garentee that the injected controller will be defined only to this TextInput and no other text inputs
  constructor(@Self() public ngControl: NgControl) {
    this.ngControl.valueAccessor = this;
  }

  // No need to implement these methods
  writeValue(obj: any): void {
    // throw new Error('Method not implemented.');
  }
  registerOnChange(fn: any): void {
    // throw new Error('Method not implemented.');
  }
  registerOnTouched(fn: any): void {
    // throw new Error('Method not implemented.');
  }
  // This method is optional
  // setDisabledState?(isDisabled: boolean): void {
  //   throw new Error('Method not implemented.');
  // }
  // A function to avoid the optional chaining in the html doc. Here we make sure that we provide a FormControl and not null
  get control(): FormControl {
    return this.ngControl.control as FormControl;
  }
}
