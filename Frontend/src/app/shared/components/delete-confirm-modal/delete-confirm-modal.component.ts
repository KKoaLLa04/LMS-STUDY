import { AfterViewInit, Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';

declare const bootstrap: any;

@Component({
  selector: 'app-delete-confirm-modal',
  standalone: true,
  imports: [],
  templateUrl: './delete-confirm-modal.component.html',
  styleUrl: './delete-confirm-modal.component.scss'
})
export class DeleteConfirmModalComponent implements AfterViewInit {
  @ViewChild('confirmModal') modalEl!: ElementRef<HTMLElement>;

  @Input() title = 'Xóa mục này?';
  @Input() confirmLabel = 'Xóa';
  @Input() bodyPrefix = 'Bạn sắp xóa';
  @Input() bodySuffix = '. Hành động này không thể hoàn tác.';
  @Input() deleting = false;

  @Output() confirmed = new EventEmitter<void>();

  itemName = '';

  private modal: any;

  ngAfterViewInit(): void {
    this.modal = new bootstrap.Modal(this.modalEl.nativeElement);
  }

  open(itemName: string): void {
    this.itemName = itemName;
    this.modal.show();
  }

  close(): void {
    this.modal.hide();
  }

  onConfirm(): void {
    this.confirmed.emit();
  }
}
