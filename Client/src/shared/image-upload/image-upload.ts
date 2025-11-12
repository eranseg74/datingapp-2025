import { Component, input, output, signal } from '@angular/core';

@Component({
  selector: 'app-image-upload',
  imports: [],
  templateUrl: './image-upload.html',
  styleUrl: './image-upload.css',
})
export class ImageUpload {
  // Because of the zoneless feature when we drag an image to the dropzone, the image will not be saved so we have to define the image source as a signal. The following -> protected imageSrc?: string | ArrayBuffer | null = null; will not work
  protected imageSrc = signal<string | ArrayBuffer | null | undefined>(null);
  protected isDragging = false;
  private fileToUpload: File | null = null;
  // Using the output and input functions given from the angular.core. The output will come from the member-photo parent element and theloading will be given to the parent element to indicate that the photot is loading
  uploadFile = output<File>();
  loading = input<boolean>(false);

  onDragOver(event: DragEvent) {
    // We want angular to handle the dragging event and not the default behavior of the browser
    event.preventDefault();
    event.stopPropagation(); // This prevents further propagation of the current event in the capturing and bubbling phases
    this.isDragging = true;
  }
  onDragLeave(event: DragEvent) {
    // We want angular to handle the dragging event and not the default behavior of the browser
    event.preventDefault();
    event.stopPropagation(); // This prevents further propagation of the current event in the capturing and bubbling phases
    this.isDragging = false;
  }
  onDrop(event: DragEvent) {
    // We want angular to handle the dragging event and not the default behavior of the browser
    event.preventDefault();
    event.stopPropagation(); // This prevents further propagation of the current event in the capturing and bubbling phases
    this.isDragging = false;
    // On drop we check if we have the file. Basically we can drag a number of files but in this case we will drag only one so we get the first element in the array (index 0)
    if (event.dataTransfer?.files.length) {
      const file = event.dataTransfer.files[0];
      this.previwImage(file);
      this.fileToUpload = file;
    }
  }

  onCancel() {
    this.fileToUpload = null;
    this.imageSrc.set(null);
  }

  onUploadFile() {
    if (this.fileToUpload) {
      this.uploadFile.emit(this.fileToUpload);
    }
  }

  private previwImage(file: File) {
    const reader = new FileReader();
    reader.onload = (e) => this.imageSrc.set(e.target?.result); // The e.target?.result returns the content of the file. Here we set the image source to the file's content so we will be able to display it in the label element in the template
    reader.readAsDataURL(file); // ThereadAsDataURL is used to read the contents of the specified file's data as a base64 encoded string
  }
}
