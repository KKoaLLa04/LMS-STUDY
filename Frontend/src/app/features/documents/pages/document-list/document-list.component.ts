import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DocumentService } from '../../services/document.service';
import { DocumentItem } from '../../models/document.model';
import { UploadService } from '../../../../shared/services/upload.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { RichTextEditorComponent } from '../../../../shared/components/rich-text-editor/rich-text-editor.component';

declare const bootstrap: any;

const MAX_DOCUMENT_SIZE = 50 * 1024 * 1024; // 50MB

@Component({
  selector: 'app-document-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, RichTextEditorComponent],
  templateUrl: './document-list.component.html',
  styleUrls: ['./document-list.component.scss']
})
export class DocumentListComponent implements OnInit, AfterViewInit {
  @ViewChild('editModal') editModalEl!: ElementRef<HTMLElement>;

  items: DocumentItem[] = [];
  loading = false;
  errorMessage = '';

  form: FormGroup;
  mode: 'add' | 'edit' = 'add';
  editingItem: DocumentItem | null = null;
  submitting = false;
  uploading = false;
  uploadedFileName = '';

  private editModal: any;

  constructor(
    private fb: FormBuilder,
    private documentService: DocumentService,
    private uploadService: UploadService,
    private toast: ToastService
  ) {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(255)]],
      content: [''],
      fileUrl: ['']
    });
  }

  ngOnInit(): void {
    this.loadItems();
  }

  ngAfterViewInit(): void {
    this.editModal = new bootstrap.Modal(this.editModalEl.nativeElement);
  }

  loadItems(): void {
    this.loading = true;
    this.errorMessage = '';
    this.documentService.getAll().subscribe({
      next: (res) => {
        this.items = res.data ?? [];
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Không thể tải danh sách tài liệu.';
        this.loading = false;
      }
    });
  }

  openCreateModal(): void {
    this.mode = 'add';
    this.editingItem = null;
    this.uploadedFileName = '';
    this.form.reset({ title: '', content: '', fileUrl: '' });
    this.editModal.show();
  }

  openEditModal(item: DocumentItem): void {
    this.mode = 'edit';
    this.editingItem = item;
    this.uploadedFileName = '';
    this.form.reset({
      title: item.title,
      content: item.content ?? '',
      fileUrl: item.fileUrl ?? ''
    });
    this.editModal.show();
  }

  closeEditModal(): void {
    this.editModal.hide();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;

    if (file.size > MAX_DOCUMENT_SIZE) {
      this.toast.error('File tài liệu không được vượt quá 50MB.');
      return;
    }

    this.uploading = true;
    this.uploadService.uploadDocument(file).subscribe({
      next: (res) => {
        this.uploading = false;
        if (!res.success || !res.data) {
          this.toast.error(res.message || 'Tải tài liệu lên thất bại');
          return;
        }
        this.form.get('fileUrl')?.setValue(res.data.url);
        this.uploadedFileName = file.name;
      },
      error: (err) => {
        this.uploading = false;
        this.toast.error(err?.error?.message || 'Tải tài liệu lên thất bại');
      }
    });
  }

  clearDocument(): void {
    this.form.get('fileUrl')?.setValue('');
    this.uploadedFileName = '';
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    const v = this.form.value;
    if (!v.content?.trim() && !v.fileUrl?.trim()) {
      this.toast.error('Cần nhập nội dung hoặc tải lên file tài liệu.');
      return;
    }

    this.submitting = true;
    const payload = {
      title: v.title,
      content: v.content || undefined,
      fileUrl: v.fileUrl || undefined
    };

    const req$ = this.mode === 'edit' && this.editingItem
      ? this.documentService.update(this.editingItem.id, payload)
      : this.documentService.create(payload);

    req$.subscribe({
      next: (res) => {
        this.submitting = false;
        if (!res.success) {
          this.toast.error(res.message || 'Lưu tài liệu thất bại');
          return;
        }
        this.toast.success(this.mode === 'edit' ? `Cập nhật tài liệu "${v.title}" thành công!` : `Tạo tài liệu "${v.title}" thành công!`);
        this.closeEditModal();
        this.loadItems();
      },
      error: (err) => {
        this.submitting = false;
        this.toast.error(err?.error?.message || 'Đã xảy ra lỗi. Vui lòng thử lại.');
      }
    });
  }

  deleteItem(item: DocumentItem): void {
    if (!confirm(`Xóa tài liệu "${item.title}"?`)) return;

    this.documentService.delete(item.id).subscribe({
      next: (res) => {
        if (!res.success) {
          this.toast.error(res.message || 'Xóa tài liệu thất bại');
          return;
        }
        this.toast.success('Đã xóa tài liệu.');
        this.loadItems();
      },
      error: (err) => {
        this.toast.error(err?.error?.message || 'Đã xảy ra lỗi. Vui lòng thử lại.');
      }
    });
  }
}
