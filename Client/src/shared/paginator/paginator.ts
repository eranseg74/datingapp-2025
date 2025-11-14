import { Component, computed, input, model, output } from '@angular/core';

@Component({
  selector: 'app-paginator',
  imports: [],
  templateUrl: './paginator.html',
  styleUrl: './paginator.css',
})
export class Paginator {
  // The page nimber will come from the parent so input would be the obvious choice. But we also need the variable to be writable so we cannot use input so we use model!
  pageNumber = model(1);
  pageSize = model(10);
  totalCount = input(0); // This is an input signal with initial value of 0
  totalPages = input(0);
  pageSizeOptions = input([5, 10, 20, 50]); // The user will be able to override this

  // An output property because we need to send this to the members service so it will go and fetch the next members to display. The type of the output will contain a page number and size. Adding the parenthesis because we do not know their values yet
  pageChange = output<{ pageNumber: number; pageSize: number }>();

  lastItemIndex = computed(() => {
    return Math.min(this.pageNumber() * this.pageSize(), this.totalCount());
  });

  // The page size comes as an event target (see the template). Here we cast the pageSize to an HTMLSelectElement and extract the value, and then convert it to a number as desired. This number will go to the pageSize input signal (model)
  onPageChange(newPage?: number, pageSize?: EventTarget | null) {
    if (newPage) this.pageNumber.set(newPage);
    if (pageSize) {
      const size = Number((pageSize as HTMLSelectElement).value);
      this.pageSize.set(size);
    }

    // After setting the page size and number we emit the changes
    this.pageChange.emit({
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
    });
  }
}
