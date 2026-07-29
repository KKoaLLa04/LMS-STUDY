import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AchievementService } from '../../services/achievement.service';
import { AchievementGroupService } from '../../services/achievement-group.service';
import { Achievement } from '../../models/achievement.model';
import { AchievementGroup } from '../../models/achievement-group.model';
import { AchievementIconComponent, AchievementIconKey } from '../../components/achievement-icon/achievement-icon.component';
import { ToastService } from '../../../../shared/services/toast.service';

declare const bootstrap: any;

type ModalMode = 'create' | 'edit';

const ICON_OPTIONS: AchievementIconKey[] = [
  'book-open', 'target', 'trophy', 'flame', 'message-circle', 'heart',
  'star', 'crown', 'users', 'graduation-cap', 'layers', 'check'
];

@Component({
  selector: 'app-achievement-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterLink, AchievementIconComponent],
  templateUrl: './achievement-list.component.html',
  styleUrls: ['./achievement-list.component.scss']
})
export class AchievementListComponent implements OnInit, AfterViewInit {
  @ViewChild('formModal') formModalEl!: ElementRef<HTMLElement>;
  @ViewChild('deleteModal') deleteModalEl!: ElementRef<HTMLElement>;
  @ViewChild('groupModal') groupModalEl!: ElementRef<HTMLElement>;
  @ViewChild('nameInput') nameInputEl?: ElementRef<HTMLInputElement>;
  @ViewChild('groupNameInput') groupNameInputEl?: ElementRef<HTMLInputElement>;

  achievements: Achievement[] = [];
  groups: AchievementGroup[] = [];
  loading = false;
  errorMessage = '';

  // Bộ lọc: nhóm đang chọn (hiển thị cùng dòng với nút "Thêm huy hiệu") + từ khóa tìm kiếm.
  selectedGroupId: number | 'all' = 'all';
  keyword = '';

  readonly iconOptions = ICON_OPTIONS;

  form: FormGroup;
  modalMode: ModalMode = 'create';
  editingItem: Achievement | null = null;
  submitting = false;

  groupForm: FormGroup;
  submittingGroup = false;

  deleteTarget: Achievement | null = null;
  deleteConfirmInput = '';
  deleting = false;

  private formModal: any;
  private deleteModal: any;
  private groupModal: any;

  constructor(
    private fb: FormBuilder,
    private achievementService: AchievementService,
    private achievementGroupService: AchievementGroupService,
    private toast: ToastService
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(255)]],
      description: ['', [Validators.required, Validators.maxLength(500)]],
      groupId: [null as number | null, [Validators.required]],
      iconKey: ['star', [Validators.required, Validators.maxLength(50)]],
      orderNumber: [0, [Validators.required, Validators.min(0)]],
      points: [50, [Validators.required, Validators.min(0)]]
    });

    this.groupForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(255)]]
    });
  }

  ngOnInit(): void {
    this.loadGroups();
    this.loadAchievements();
  }

  ngAfterViewInit(): void {
    this.formModal = new bootstrap.Modal(this.formModalEl.nativeElement);
    this.deleteModal = new bootstrap.Modal(this.deleteModalEl.nativeElement);
    this.groupModal = new bootstrap.Modal(this.groupModalEl.nativeElement);
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

  loadGroups(): void {
    this.achievementGroupService.getGroups().subscribe({
      next: (res) => (this.groups = res.data ?? []),
      error: () => this.toast.error('Không thể tải danh sách nhóm huy hiệu.')
    });
  }

  get filteredAchievements(): Achievement[] {
    const kw = this.keyword.trim().toLowerCase();
    return this.achievements.filter((a) => {
      const matchesGroup = this.selectedGroupId === 'all' || a.groupId === this.selectedGroupId;
      const matchesKeyword = !kw || a.name.toLowerCase().includes(kw) || a.description.toLowerCase().includes(kw);
      return matchesGroup && matchesKeyword;
    });
  }

  selectGroupFilter(groupId: number | 'all'): void {
    this.selectedGroupId = groupId;
  }

  get modalTitle(): string {
    return this.modalMode === 'create' ? 'Thêm huy hiệu' : 'Chỉnh sửa huy hiệu';
  }

  openCreateModal(): void {
    this.modalMode = 'create';
    this.editingItem = null;
    const defaultGroupId = this.selectedGroupId !== 'all' ? this.selectedGroupId : (this.groups[0]?.id ?? null);
    this.form.reset({
      name: '',
      description: '',
      groupId: defaultGroupId,
      iconKey: 'star',
      orderNumber: this.achievements.length,
      points: 50
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
      groupId: item.groupId,
      iconKey: item.iconKey,
      orderNumber: item.orderNumber,
      points: item.points
    });
    this.formModal.show();
    setTimeout(() => this.nameInputEl?.nativeElement.focus(), 200);
  }

  closeFormModal(): void {
    this.formModal.hide();
  }

  selectIcon(icon: AchievementIconKey): void {
    this.form.get('iconKey')?.setValue(icon);
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
      groupId: v.groupId,
      iconKey: v.iconKey,
      orderNumber: v.orderNumber,
      points: v.points
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

  // ── Nhóm huy hiệu (modal tạo mới, dùng luôn làm filter ở trên) ──

  openCreateGroupModal(): void {
    this.groupForm.reset({ name: '' });
    this.groupModal.show();
    setTimeout(() => this.groupNameInputEl?.nativeElement.focus(), 200);
  }

  closeGroupModal(): void {
    this.groupModal.hide();
  }

  submitGroup(): void {
    this.groupForm.markAllAsTouched();
    if (this.groupForm.invalid) {
      this.toast.error('Vui lòng nhập tên nhóm huy hiệu.');
      return;
    }

    this.submittingGroup = true;
    const name = (this.groupForm.value.name as string).trim();
    this.achievementGroupService.createGroup({ name }).subscribe({
      next: (res) => {
        this.submittingGroup = false;
        if (!res.success) {
          this.toast.error(res.message || 'Tạo nhóm huy hiệu thất bại');
          return;
        }
        this.toast.success(`Tạo nhóm huy hiệu "${name}" thành công!`);
        this.closeGroupModal();
        this.loadGroups();
      },
      error: (err) => {
        this.submittingGroup = false;
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
