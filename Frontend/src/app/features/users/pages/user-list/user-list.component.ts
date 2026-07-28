import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { UserService } from '../../services/user.service';
import { AppUser, AppUserGender, AppUserRole, AppUserStatus } from '../../models/user.model';
import { ToastService } from '../../../../shared/services/toast.service';
import { UploadService } from '../../../../shared/services/upload.service';

declare const bootstrap: any;

type ModalMode = 'create' | 'edit';

const MAX_AVATAR_SIZE = 10 * 1024 * 1024; // 10MB

interface AvatarUploadState {
  uploading: boolean;
}

const STATUS_LABELS: Record<AppUserStatus, string> = {
  Active: 'Đang hoạt động',
  Inactive: 'Ngừng hoạt động',
  Banned: 'Đã khóa'
};

const GENDER_LABELS: Record<AppUserGender, string> = {
  Male: 'Nam',
  Female: 'Nữ',
  Other: 'Khác'
};

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterLink],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.scss']
})
export class UserListComponent implements OnInit, AfterViewInit {
  @ViewChild('formModal') formModalEl!: ElementRef<HTMLElement>;
  @ViewChild('deleteModal') deleteModalEl!: ElementRef<HTMLElement>;
  @ViewChild('fullNameInput') fullNameInputEl?: ElementRef<HTMLInputElement>;

  // Cấu hình theo route data — cùng 1 component phục vụ cả "Danh sách giảng viên" và "Danh sách học sinh".
  role: AppUserRole = 'Student';
  pageTitle = 'Danh sách người dùng';
  entityLabel = 'người dùng';
  createLabel = 'Thêm người dùng';

  readonly statusOptions = Object.entries(STATUS_LABELS) as [AppUserStatus, string][];
  readonly genderOptions = Object.entries(GENDER_LABELS) as [AppUserGender, string][];

  users: AppUser[] = [];
  loading = false;
  errorMessage = '';

  form: FormGroup;
  modalMode: ModalMode = 'create';
  editingItem: AppUser | null = null;
  submitting = false;

  deleteTarget: AppUser | null = null;
  deleteConfirmInput = '';
  deleting = false;

  avatarPreviewUrl: string | null = null;
  avatarError = false;
  avatarUploadState: AvatarUploadState = { uploading: false };

  private formModal: any;
  private deleteModal: any;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private userService: UserService,
    private uploadService: UploadService,
    private toast: ToastService
  ) {
    this.form = this.fb.group({
      fullName: ['', [Validators.required, Validators.maxLength(255)]],
      username: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(255)]],
      phone: ['', [Validators.required, Validators.maxLength(20)]],
      password: ['', [Validators.minLength(6)]],
      status: ['Active' as AppUserStatus, [Validators.required]],
      gender: [''],
      dateOfBirth: [''],
      address: ['', [Validators.maxLength(500)]],
      avatarUrl: ['']
    });
  }

  ngOnInit(): void {
    const data = this.route.snapshot.data;
    this.role = data['role'] ?? 'Student';
    this.pageTitle = data['pageTitle'] ?? this.pageTitle;
    this.entityLabel = data['entityLabel'] ?? this.entityLabel;
    this.createLabel = data['createLabel'] ?? this.createLabel;

    this.loadUsers();
  }

  ngAfterViewInit(): void {
    this.formModal = new bootstrap.Modal(this.formModalEl.nativeElement);
    this.deleteModal = new bootstrap.Modal(this.deleteModalEl.nativeElement);
  }

  loadUsers(): void {
    this.loading = true;
    this.errorMessage = '';
    this.userService.getUsers(this.role).subscribe({
      next: (res) => {
        this.users = res.data ?? [];
        this.loading = false;
      },
      error: () => {
        this.errorMessage = `Không thể tải danh sách ${this.entityLabel}. Vui lòng thử lại.`;
        this.loading = false;
      }
    });
  }

  get modalTitle(): string {
    return this.modalMode === 'create' ? this.createLabel : `Chỉnh sửa ${this.entityLabel}`;
  }

  statusLabel(status: string): string {
    return STATUS_LABELS[status as AppUserStatus] ?? status;
  }

  genderLabel(gender?: string | null): string {
    if (!gender) return '—';
    return GENDER_LABELS[gender as AppUserGender] ?? gender;
  }

  statusBadgeClass(status: string): string {
    switch (status) {
      case 'Active':
        return 'usr-status-active';
      case 'Banned':
        return 'usr-status-banned';
      default:
        return 'usr-status-inactive';
    }
  }

  initialsOf(user: AppUser): string {
    return (user.fullName || user.username).trim().charAt(0).toUpperCase() || '?';
  }

  get formInitials(): string {
    const name = (this.form.get('fullName')?.value || this.form.get('username')?.value || '').trim();
    return name.charAt(0).toUpperCase() || '?';
  }

  openCreateModal(): void {
    this.modalMode = 'create';
    this.editingItem = null;
    this.form.reset({
      fullName: '',
      username: '',
      email: '',
      phone: '',
      password: '',
      status: 'Active',
      gender: '',
      dateOfBirth: '',
      address: '',
      avatarUrl: ''
    });
    this.avatarPreviewUrl = null;
    this.avatarError = false;
    this.avatarUploadState = { uploading: false };
    this.form.get('password')?.setValidators([Validators.required, Validators.minLength(6)]);
    this.form.get('password')?.updateValueAndValidity();
    this.formModal.show();
    setTimeout(() => this.fullNameInputEl?.nativeElement.focus(), 200);
  }

  openEditModal(item: AppUser): void {
    this.modalMode = 'edit';
    this.editingItem = item;
    this.form.reset({
      fullName: item.fullName,
      username: item.username,
      email: item.email,
      phone: item.phone,
      password: '',
      status: item.status,
      gender: item.gender ?? '',
      dateOfBirth: item.dateOfBirth ?? '',
      address: item.address ?? '',
      avatarUrl: item.avatarUrl ?? ''
    });
    this.avatarPreviewUrl = item.avatarUrl ?? null;
    this.avatarError = false;
    this.avatarUploadState = { uploading: false };
    this.form.get('password')?.setValidators([Validators.minLength(6)]);
    this.form.get('password')?.updateValueAndValidity();
    this.formModal.show();
    setTimeout(() => this.fullNameInputEl?.nativeElement.focus(), 200);
  }

  closeFormModal(): void {
    this.formModal.hide();
  }

  onAvatarFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      this.toast.error('Vui lòng chọn một file ảnh hợp lệ.');
      return;
    }
    if (file.size > MAX_AVATAR_SIZE) {
      this.toast.error('File ảnh không được vượt quá 10MB.');
      return;
    }

    this.avatarUploadState = { uploading: true };
    this.uploadService.uploadImage(file).subscribe({
      next: (res) => {
        this.avatarUploadState = { uploading: false };
        if (!res.success || !res.data) {
          this.toast.error(res.message || 'Tải ảnh lên thất bại');
          return;
        }
        this.form.get('avatarUrl')?.setValue(res.data.url);
        this.avatarPreviewUrl = res.data.url;
        this.avatarError = false;
      },
      error: (err) => {
        this.avatarUploadState = { uploading: false };
        this.toast.error(err?.error?.message || 'Tải ảnh lên thất bại');
      }
    });
  }

  removeAvatar(): void {
    this.form.get('avatarUrl')?.setValue('');
    this.avatarPreviewUrl = null;
    this.avatarError = false;
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
      fullName: v.fullName.trim(),
      username: v.username.trim(),
      email: v.email.trim(),
      phone: v.phone.trim(),
      status: v.status as AppUserStatus,
      gender: (v.gender || null) as AppUserGender | null,
      dateOfBirth: v.dateOfBirth || null,
      address: v.address?.trim() || null,
      avatarUrl: v.avatarUrl?.trim() || null,
      role: this.role
    };

    const request$ =
      this.modalMode === 'create'
        ? this.userService.createUser({ ...payload, password: v.password })
        : this.userService.updateUser(this.editingItem!.id, { ...payload, password: v.password || null });

    request$.subscribe({
      next: (res) => {
        this.submitting = false;
        if (!res.success) {
          this.toast.error(res.message || 'Thao tác thất bại');
          return;
        }
        this.toast.success(
          this.modalMode === 'create'
            ? `Tạo ${this.entityLabel} "${v.fullName}" thành công!`
            : `Cập nhật "${v.fullName}" thành công!`
        );
        this.closeFormModal();
        this.loadUsers();
      },
      error: (err) => {
        this.submitting = false;
        this.toast.error(err?.error?.message || 'Đã xảy ra lỗi. Vui lòng thử lại.');
      }
    });
  }

  openDeleteModal(item: AppUser): void {
    this.deleteTarget = item;
    this.deleteConfirmInput = '';
    this.deleteModal.show();
  }

  closeDeleteModal(): void {
    this.deleteModal.hide();
  }

  get isDeleteConfirmValid(): boolean {
    if (!this.deleteTarget) return false;
    return this.deleteConfirmInput.trim().toLowerCase() === this.deleteTarget.username.trim().toLowerCase();
  }

  confirmDelete(): void {
    if (!this.deleteTarget || !this.isDeleteConfirmValid) return;

    this.deleting = true;
    const { id, username } = this.deleteTarget;
    this.userService.deleteUser(id).subscribe({
      next: () => {
        this.deleting = false;
        this.toast.success(`Đã xóa "${username}" thành công.`);
        this.closeDeleteModal();
        this.loadUsers();
      },
      error: (err) => {
        this.deleting = false;
        this.toast.error(err?.error?.message || 'Xóa thất bại. Vui lòng thử lại.');
      }
    });
  }
}
