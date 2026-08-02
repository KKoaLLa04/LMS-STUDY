import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { PermissionGroupService } from '../../services/permission-group.service';
import { PermissionGroup, PermissionGroupMember } from '../../models/permission-group.model';
import { ToastService } from '../../../../shared/services/toast.service';
import { UserService } from '../../../users/services/user.service';
import { AppUser, TEACHER_PERMISSION_MODULES } from '../../../users/models/user.model';
import { PermissionModule, TeacherPermission } from '../../../../core/auth/models/auth.model';

declare const bootstrap: any;

type ModalMode = 'create' | 'edit';
type PermissionFlags = Pick<TeacherPermission, 'canView' | 'canCreate' | 'canUpdate' | 'canDelete'>;

@Component({
  selector: 'app-permission-group-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterLink],
  templateUrl: './permission-group-list.component.html',
  styleUrls: ['./permission-group-list.component.scss']
})
export class PermissionGroupListComponent implements OnInit, AfterViewInit {
  @ViewChild('formModal') formModalEl!: ElementRef<HTMLElement>;
  @ViewChild('deleteModal') deleteModalEl!: ElementRef<HTMLElement>;
  @ViewChild('nameInput') nameInputEl?: ElementRef<HTMLInputElement>;

  readonly permissionModules = TEACHER_PERMISSION_MODULES;

  groups: PermissionGroup[] = [];
  loading = false;
  errorMessage = '';

  teachers: AppUser[] = [];
  selectedMemberIds = new Set<number>();

  form: FormGroup;
  modalMode: ModalMode = 'create';
  editingItem: PermissionGroup | null = null;
  submitting = false;
  loadingDetail = false;
  permissionsMap: Record<PermissionModule, PermissionFlags> = this.buildEmptyPermissionsMap();

  deleteTarget: PermissionGroup | null = null;
  deleteConfirmInput = '';
  deleting = false;

  private formModal: any;
  private deleteModal: any;

  constructor(
    private fb: FormBuilder,
    private groupService: PermissionGroupService,
    private userService: UserService,
    private toast: ToastService
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(255)]],
      description: ['', [Validators.maxLength(1000)]]
    });
  }

  ngOnInit(): void {
    this.loadGroups();
    this.userService.getUsers('Teacher').subscribe((res) => (this.teachers = res.data ?? []));
  }

  ngAfterViewInit(): void {
    this.formModal = new bootstrap.Modal(this.formModalEl.nativeElement);
    this.deleteModal = new bootstrap.Modal(this.deleteModalEl.nativeElement);
  }

  loadGroups(): void {
    this.loading = true;
    this.errorMessage = '';
    this.groupService.getGroups().subscribe({
      next: (res) => {
        this.groups = res.data ?? [];
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Không thể tải danh sách nhóm quyền. Vui lòng thử lại.';
        this.loading = false;
      }
    });
  }

  get modalTitle(): string {
    return this.modalMode === 'create' ? 'Thêm nhóm quyền' : 'Chỉnh sửa nhóm quyền';
  }

  private buildEmptyPermissionsMap(): Record<PermissionModule, PermissionFlags> {
    const map = {} as Record<PermissionModule, PermissionFlags>;
    for (const { module } of TEACHER_PERMISSION_MODULES) {
      map[module] = { canView: false, canCreate: false, canUpdate: false, canDelete: false };
    }
    return map;
  }

  private applyPermissions(source?: TeacherPermission[]): void {
    const map = this.buildEmptyPermissionsMap();
    for (const p of source ?? []) {
      map[p.module] = { canView: p.canView, canCreate: p.canCreate, canUpdate: p.canUpdate, canDelete: p.canDelete };
    }
    this.permissionsMap = map;
  }

  isMemberSelected(id: number): boolean {
    return this.selectedMemberIds.has(id);
  }

  toggleMember(id: number): void {
    if (this.selectedMemberIds.has(id)) this.selectedMemberIds.delete(id);
    else this.selectedMemberIds.add(id);
  }

  openCreateModal(): void {
    this.modalMode = 'create';
    this.editingItem = null;
    this.form.reset({ name: '', description: '' });
    this.applyPermissions();
    this.selectedMemberIds = new Set<number>();
    this.formModal.show();
    setTimeout(() => this.nameInputEl?.nativeElement.focus(), 200);
  }

  openEditModal(item: PermissionGroup): void {
    this.modalMode = 'edit';
    this.editingItem = item;
    this.loadingDetail = true;
    this.form.reset({ name: item.name, description: item.description ?? '' });
    this.applyPermissions();
    this.selectedMemberIds = new Set<number>();
    this.formModal.show();

    this.groupService.getGroupById(item.id).subscribe({
      next: (res) => {
        this.loadingDetail = false;
        if (!res.data) return;
        this.applyPermissions(res.data.modulePermissions);
        this.selectedMemberIds = new Set<number>((res.data.members ?? []).map((m: PermissionGroupMember) => m.id));
      },
      error: () => {
        this.loadingDetail = false;
        this.toast.error('Không thể tải chi tiết nhóm quyền.');
      }
    });
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
      description: v.description?.trim() || null,
      modulePermissions: this.permissionModules.map(({ module }) => ({ module, ...this.permissionsMap[module] }))
    };

    const request$ =
      this.modalMode === 'create'
        ? this.groupService.createGroup(payload)
        : this.groupService.updateGroup(this.editingItem!.id, payload);

    request$.subscribe({
      next: (res) => {
        if (!res.success || !res.data) {
          this.submitting = false;
          this.toast.error(res.message || 'Thao tác thất bại');
          return;
        }

        this.groupService.setMembers(res.data.id, Array.from(this.selectedMemberIds)).subscribe({
          next: () => {
            this.submitting = false;
            this.toast.success(
              this.modalMode === 'create'
                ? `Tạo nhóm quyền "${v.name}" thành công!`
                : `Cập nhật nhóm quyền "${v.name}" thành công!`
            );
            this.closeFormModal();
            this.loadGroups();
          },
          error: (err) => {
            this.submitting = false;
            this.toast.error(err?.error?.message || 'Lưu thành viên nhóm thất bại.');
          }
        });
      },
      error: (err) => {
        this.submitting = false;
        this.toast.error(err?.error?.message || 'Đã xảy ra lỗi. Vui lòng thử lại.');
      }
    });
  }

  openDeleteModal(item: PermissionGroup): void {
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
    this.groupService.deleteGroup(id).subscribe({
      next: () => {
        this.deleting = false;
        this.toast.success(`Đã xóa nhóm quyền "${name}" thành công.`);
        this.closeDeleteModal();
        this.loadGroups();
      },
      error: (err) => {
        this.deleting = false;
        this.toast.error(err?.error?.message || 'Xóa nhóm quyền thất bại. Vui lòng thử lại.');
      }
    });
  }
}
