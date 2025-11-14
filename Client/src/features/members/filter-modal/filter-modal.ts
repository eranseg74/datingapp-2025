import { Component, ElementRef, model, output, ViewChild } from '@angular/core';
import { MemberParams } from '../../../types/member';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-filter-modal',
  imports: [FormsModule],
  templateUrl: './filter-modal.html',
  styleUrl: './filter-modal.css',
})
export class FilterModal {
  // Without the exclamation mark we would have to initialize it. Here we are promising that it will not be null ever
  @ViewChild('filterModal') modalRef!: ElementRef<HTMLDialogElement>;
  closeModal = output(); // No parameters because we will not emit anything. Just saying that the modal is closed
  submitData = output<MemberParams>();
  memberParams = model(new MemberParams());

  // Using the constructor to take initial values from the local storage
  constructor() {
    const filters = localStorage.getItem('filters');
    if (filters) {
      this.memberParams.set(JSON.parse(filters));
    }
  }

  open() {
    // Openning the modal dialog box
    this.modalRef.nativeElement.showModal();
  }

  close() {
    this.modalRef.nativeElement.close();
    this.closeModal.emit(); // Again, not emitting anything, just indicating that the modal is closed
  }

  submit() {
    this.submitData.emit(this.memberParams());
    this.close();
  }

  // Setting a limitation of the min age to 18. If the user will enter a min age lower than 18 the min age will be set to 18
  onMinAgeChange() {
    if (this.memberParams().minAge < 18) this.memberParams().minAge = 18;
  }

  // Setting a limitation of the max age not to be less than the min age. In case a user define a max age lower than the min age, the max age will be set to the min age
  onMaxAgeChange() {
    if (this.memberParams().maxAge < this.memberParams().minAge)
      this.memberParams().maxAge = this.memberParams().minAge;
  }
}
