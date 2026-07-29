import { Component, Input, forwardRef, inject } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { CKEditorModule, ChangeEvent } from '@ckeditor/ckeditor5-angular';
import {
  ClassicEditor,
  Essentials,
  Paragraph,
  Heading,
  Bold,
  Italic,
  Underline,
  Link,
  List,
  BlockQuote,
  Table,
  TableToolbar,
  Image,
  ImageUpload,
  ImageToolbar,
  ImageStyle,
  ImageResize,
  Undo
} from 'ckeditor5';
import { UploadService } from '../../services/upload.service';
import { createImageUploadAdapterPlugin } from '../../ckeditor/ck-image-upload-adapter';

// Wrapper CKEditor 5 (free, self-host) dùng như một form control bình thường qua
// ControlValueAccessor — chỉ cần gắn formControlName như với input/textarea. Chèn ảnh dùng
// upload adapter tự viết gọi api/uploads/image sẵn có của app (thay cho CKFinder — sản phẩm
// thương mại, không có bản miễn phí).
@Component({
  selector: 'app-rich-text-editor',
  standalone: true,
  imports: [CKEditorModule],
  template: `
    <ckeditor
      [editor]="Editor"
      [config]="config"
      [data]="value"
      [disabled]="disabled"
      (change)="onEditorChange($event)"
      (blur)="onTouched()"
    ></ckeditor>
  `,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => RichTextEditorComponent),
      multi: true
    }
  ]
})
export class RichTextEditorComponent implements ControlValueAccessor {
  @Input() placeholder = '';

  private readonly uploadService = inject(UploadService);

  readonly Editor = ClassicEditor;
  readonly config = {
    licenseKey: 'GPL',
    plugins: [
      Essentials,
      Paragraph,
      Heading,
      Bold,
      Italic,
      Underline,
      Link,
      List,
      BlockQuote,
      Table,
      TableToolbar,
      Image,
      ImageUpload,
      ImageToolbar,
      ImageStyle,
      ImageResize,
      Undo
    ],
    toolbar: [
      'heading',
      '|',
      'bold',
      'italic',
      'underline',
      'link',
      '|',
      'bulletedList',
      'numberedList',
      'blockQuote',
      'insertTable',
      'uploadImage',
      '|',
      'undo',
      'redo'
    ],
    image: {
      toolbar: ['imageStyle:inline', 'imageStyle:block', 'imageStyle:side', '|', 'resizeImage']
    },
    table: {
      contentToolbar: ['tableColumn', 'tableRow', 'mergeTableCells']
    },
    extraPlugins: [createImageUploadAdapterPlugin(this.uploadService)]
  };

  value = '';
  disabled = false;

  private onChangeFn: (value: string) => void = () => {};
  onTouched: () => void = () => {};

  writeValue(value: string | null): void {
    this.value = value ?? '';
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChangeFn = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onEditorChange(event: ChangeEvent): void {
    const data = (event.editor as ClassicEditor).getData();
    this.value = data;
    this.onChangeFn(data);
  }
}
