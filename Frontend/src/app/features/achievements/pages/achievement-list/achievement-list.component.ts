import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AchievementService } from '../../services/achievement.service';
import { Achievement, AchievementCategory } from '../../models/achievement.model';
import { ToastService } from '../../../../shared/services/toast.service';

declare const bootstrap: any;

type ModalMode = 'create' | 'edit';

const CATEGORY_LABELS: Record<AchievementCategory, string> = {
  HocTap: 'Học tập',
  ChuyenCan: 'Chuyên cần',
  TuongTac: 'Tương tác'
};

const ICON_OPTIONS = [
  'book-open', 'target', 'trophy', 'flame', 'message-circle', 'heart',
  'star', 'crown', 'users', 'graduation-cap', 'layers', 'check'
];

@Component({
  selector: 'app-achievement-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterLink],
  templateUrl: './achievement-list.component.html',
  styleUrls: ['./achievement-list.component.scss']
})
export class AchievementListComponent implements OnInit, AfterViewInit {
  @ViewChild('formModal') formModalEl!: ElementRef<HTMLElement>;
  @ViewChild('deleteModal') deleteModalEl!: ElementRef<HTMLElement>;
  @ViewChild('nameInput') nameInputEl?: ElementRef<HTMLInputElement>;

  achievements: Achievement[] = [];
  loading = false;
  errorMessage = '';

  readonly categoryOptions = Object.entries(CATEGORY_LABELS) as [AchievementCategory, string][];
  readonly iconOptions = ICON_OPTIONS;

  form: FormGroup;
  modalMode: ModalMode = 'create';
  editingItem: Achievement | null = null;
  submitting = false;

  deleteTarget: Achievement | null = null;
  deleteConfirmInput = '';
  deleting = false;

  private formModal: any;
  private deleteModal: any;

  constructor(
    private fb: FormBuilder,
    private achievementService: AchievementService,
    private toast: ToastService
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(255)]],
      description: ['', [Validators.required, Validators.maxLength(500)]],
      category: ['HocTap' as AchievementCategory, [Validators.required]],
      iconKey: ['star', [Validators.required, Validators.maxLength(50)]],
      orderNumber: [0, [Validators.required, Validators.min(0)]],
      isUnlocked: [false],
      progressPercent: [0, [Validators.required, Validators.min(0), Validators.max(100)]]
    });
  }

  ngOnInit(): void {
    this.loadAchievements();
  }

  ngAfterViewInit(): void {
    this.formModal = new bootstrap.Modal(this.formModalEl.nativeElement);
    this.deleteModal = new bootstrap.Modal(this.deleteModalEl.nativeElement);
  }

  loadAchievements(): void {
    this.loading = true;
    this.errorMessage = '';
    this.achievementService.getAchievements().subscribe({
      next: (res) => {
        this.achievements = (res.data ?? []).slice().sort((a, b) => a.orderNumber - b.orderNumber);
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Không thể tải danh sách huy hiệu. Vui lòng thử lại.';
        this.loading = false;
      }
    });
  }

  categoryLabel(category: string): string {
    return CATEGORY_LABELS[category as AchievementCategory] ?? category;
  }

  get modalTitle(): string {
    return this.modalMode === 'create' ? 'Thêm huy hiệu' : 'Chỉnh sửa huy hiệu';
  }

  openCreateModal(): void {
    this.modalMode = 'create';
    this.editingItem = null;
    this.form.reset({
      name: '',
      description: '',
      category: 'HocTap',
      iconKey: 'star',
      orderNumber: this.achievements.length,
      isUnlocked: false,
      progressPercent: 0
    });
    this.formModal.show();
    setTimeout(() => this.nameInputEl?.nativeElement.focus(), 200);
  }

  openEditModal(item: Achievement): void {
    this.modalMode = 'edit';
    this.editingItem = item;
    this.form.reset({
      name: item.name,
      description: item.description,
      category: item.category,
      iconKey: item.iconKey,
      orderNumber: item.orderNumber,
      isUnlocked: item.isUnlocked,
      progressPercent: item.progressPercent
    });
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
    const payload = {
      name: v.name.trim(),
      description: v.description.trim(),
      category: v.category,
      iconKey: v.iconKey,
      orderNumber: v.orderNumber,
      isUnlocked: v.isUnlocked,
      progressPercent: v.progressPercent
    };

    const request$ =
      this.modalMode === 'create'
        ? this.achievementService.createAchievement(payload)
        : this.achievementService.updateAchievement(this.editingItem!.id, payload);

    request$.subscribe({
      next: (res) => {
        this.submitting = false;
        if (!res.success) {
          this.toast.error(res.message || 'Thao tác thất bại');
          return;
        }
        this.toast.success(
          this.modalMode === 'create'
            ? `Tạo huy hiệu "${v.name}" thành công!`
            : `Cập nhật huy hiệu "${v.name}" thành công!`
        );
        this.closeFormModal();
        this.loadAchievements();
      },
      error: (err) => {
        this.submitting = false;
        this.toast.error(err?.error?.message || 'Đã xảy ra lỗi. Vui lòng thử lại.');
      }
    });
  }

  openDeleteModal(item: Achievement): void {
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
    this.achievementService.deleteAchievement(id).subscribe({
      next: () => {
        this.deleting = false;
        this.toast.success(`Đã xóa huy hiệu "${name}" thành công.`);
        this.closeDeleteModal();
        this.loadAchievements();
      },
      error: (err) => {
        this.deleting = false;
        this.toast.error(err?.error?.message || 'Xóa huy hiệu thất bại. Vui lòng thử lại.');
      }
    });
  }
}
