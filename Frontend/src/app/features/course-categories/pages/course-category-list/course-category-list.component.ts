import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CourseCategoryService } from '../../services/course-category.service';
import { CourseCategory } from '../../models/course-category.model';
import { ToastService } from '../../../../shared/services/toast.service';

declare const bootstrap: any;

type ModalMode = 'create' | 'edit';

@Component({
  selector: 'app-course-category-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterLink],
  templateUrl: './course-category-list.component.html',
  styleUrls: ['./course-category-list.component.scss']
})
export class CourseCategoryListComponent implements OnInit, AfterViewInit {
  @ViewChild('formModal') formModalEl!: ElementRef<HTMLElement>;
  @ViewChild('deleteModal') deleteModalEl!: ElementRef<HTMLElement>;
  @ViewChild('nameInput') nameInputEl?: ElementRef<HTMLInputElement>;

  categories: CourseCategory[] = [];
  loading = false;
  errorMessage = '';

  form: FormGroup;
  modalMode: ModalMode = 'create';
  editingItem: CourseCategory | null = null;
  submitting = false;

  deleteTarget: CourseCategory | null = null;
  deleteConfirmInput = '';
  deleting = false;

  private formModal: any;
  private deleteModal: any;

  constructor(
    private fb: FormBuilder,
    private categoryService: CourseCategoryService,
    private toast: ToastService
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(255)]],
      code: ['', [Validators.required, Validators.maxLength(50)]],
      orderNumber: [0, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.loadCategories();
  }

  ngAfterViewInit(): void {
    this.formModal = new bootstrap.Modal(this.formModalEl.nativeElement);
    this.deleteModal = new bootstrap.Modal(this.deleteModalEl.nativeElement);
  }

  loadCategories(): void {
    this.loading = true;
    this.errorMessage = '';
    this.categoryService.getCategories().subscribe({
      next: (res) => {
        this.categories = res.data ?? [];
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Không thể tải danh sách danh mục khóa học. Vui lòng thử lại.';
        this.loading = false;
      }
    });
  }

  get modalTitle(): string {
    return this.modalMode === 'create' ? 'Thêm danh mục' : 'Chỉnh sửa danh mục';
  }

  openCreateModal(): void {
    this.modalMode = 'create';
    this.editingItem = null;
    this.form.reset({ name: '', code: '', orderNumber: this.categories.length });
    this.formModal.show();
    setTimeout(() => this.nameInputEl?.nativeElement.focus(), 200);
  }

  openEditModal(item: CourseCategory): void {
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
        ? this.categoryService.createCategory(payload)
        : this.categoryService.updateCategory(this.editingItem!.id, payload);

    request$.subscribe({
      next: (res) => {
        this.submitting = false;
        if (!res.success) {
          this.toast.error(res.message || 'Thao tác thất bại');
          return;
        }
        this.toast.success(
          this.modalMode === 'create'
            ? `Tạo danh mục "${v.name}" thành công!`
            : `Cập nhật danh mục "${v.name}" thành công!`
        );
        this.closeFormModal();
        this.loadCategories();
      },
      error: (err) => {
        this.submitting = false;
        this.toast.error(err?.error?.message || 'Đã xảy ra lỗi. Vui lòng thử lại.');
      }
    });
  }

  openDeleteModal(item: CourseCategory): void {
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
    this.categoryService.deleteCategory(id).subscribe({
      next: () => {
        this.deleting = false;
        this.toast.success(`Đã xóa danh mục "${name}" thành công.`);
        this.closeDeleteModal();
        this.loadCategories();
      },
      error: (err) => {
        this.deleting = false;
        this.toast.error(err?.error?.message || 'Xóa danh mục thất bại. Vui lòng thử lại.');
      }
    });
  }
}
