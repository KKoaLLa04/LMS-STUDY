import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { KhoiHocService } from '../../services/khoi-hoc.service';
import { KhoiHoc } from '../../models/khoi-hoc.model';
import { ToastService } from '../../../../shared/services/toast.service';
import { AuthService } from '../../../../core/auth/auth.service';

declare const bootstrap: any;

type ModalMode = 'create' | 'edit';

@Component({
  selector: 'app-khoi-hoc-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterLink],
  templateUrl: './khoi-hoc-list.component.html',
  styleUrls: ['./khoi-hoc-list.component.scss']
})
export class KhoiHocListComponent implements OnInit, AfterViewInit {
  @ViewChild('formModal') formModalEl!: ElementRef<HTMLElement>;
  @ViewChild('deleteModal') deleteModalEl!: ElementRef<HTMLElement>;
  @ViewChild('nameInput') nameInputEl?: ElementRef<HTMLInputElement>;

  khoiHocs: KhoiHoc[] = [];
  loading = false;
  errorMessage = '';

  form: FormGroup;
  modalMode: ModalMode = 'create';
  editingItem: KhoiHoc | null = null;
  submitting = false;

  deleteTarget: KhoiHoc | null = null;
  deleteConfirmInput = '';
  deleting = false;

  private formModal: any;
  private deleteModal: any;

  constructor(
    private fb: FormBuilder,
    private khoiHocService: KhoiHocService,
    private toast: ToastService,
    public auth: AuthService
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(255)]],
      code: ['', [Validators.required, Validators.maxLength(50)]],
      orderNumber: [0, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.loadKhoiHocs();
  }

  ngAfterViewInit(): void {
    this.formModal = new bootstrap.Modal(this.formModalEl.nativeElement);
    this.deleteModal = new bootstrap.Modal(this.deleteModalEl.nativeElement);
  }

  loadKhoiHocs(): void {
    this.loading = true;
    this.errorMessage = '';
    this.khoiHocService.getKhoiHocs().subscribe({
      next: (res) => {
        this.khoiHocs = res.data ?? [];
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Không thể tải danh sách khối học. Vui lòng thử lại.';
        this.loading = false;
      }
    });
  }

  get modalTitle(): string {
    return this.modalMode === 'create' ? 'Thêm khối học' : 'Chỉnh sửa khối học';
  }

  openCreateModal(): void {
    this.modalMode = 'create';
    this.editingItem = null;
    this.form.reset({ name: '', code: '', orderNumber: this.khoiHocs.length });
    this.formModal.show();
    setTimeout(() => this.nameInputEl?.nativeElement.focus(), 200);
  }

  openEditModal(item: KhoiHoc): void {
    this.modalMode = 'edit';
    this.editingItem = item;
    this.form.reset({ name: item.name, code: item.code, orderNumber: item.orderNumber });
    this.formModal.show();
    setTimeout(() => this.nameInputEl?.nativeElement.focus(), 200);
  }

  closeFormModal(): void {
    this.formModal.hide();
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.toast.error('Vui lòng điền đầy đủ các trường bắt buộc.');
      return;
    }

    this.submitting = true;
    const v = this.form.value;
    const payload = { name: v.name.trim(), code: v.code.trim(), orderNumber: v.orderNumber };

    const request$ =
      this.modalMode === 'create'
        ? this.khoiHocService.createKhoiHoc(payload)
        : this.khoiHocService.updateKhoiHoc(this.editingItem!.id, payload);

    request$.subscribe({
      next: (res) => {
        this.submitting = false;
        if (!res.success) {
          this.toast.error(res.message || 'Thao tác thất bại');
          return;
        }
        this.toast.success(
          this.modalMode === 'create'
            ? `Tạo khối học "${v.name}" thành công!`
            : `Cập nhật khối học "${v.name}" thành công!`
        );
        this.closeFormModal();
        this.loadKhoiHocs();
      },
      error: (err) => {
        this.submitting = false;
        this.toast.error(err?.error?.message || 'Đã xảy ra lỗi. Vui lòng thử lại.');
      }
    });
  }

  openDeleteModal(item: KhoiHoc): void {
    this.deleteTarget = item;
    this.deleteConfirmInput = '';
    this.deleteModal.show();
  }

  closeDeleteModal(): void {
    this.deleteModal.hide();
  }

  get isDeleteConfirmValid(): boolean {
    if (!this.deleteTarget) return false;
    return this.deleteConfirmInput.trim().toLowerCase() === this.deleteTarget.name.trim().toLowerCase();
  }

  confirmDelete(): void {
    if (!this.deleteTarget || !this.isDeleteConfirmValid) return;

    this.deleting = true;
    const { id, name } = this.deleteTarget;
    this.khoiHocService.deleteKhoiHoc(id).subscribe({
      next: () => {
        this.deleting = false;
        this.toast.success(`Đã xóa khối học "${name}" thành công.`);
        this.closeDeleteModal();
        this.loadKhoiHocs();
      },
      error: (err) => {
        this.deleting = false;
        this.toast.error(err?.error?.message || 'Xóa khối học thất bại. Vui lòng thử lại.');
      }
    });
  }
}
