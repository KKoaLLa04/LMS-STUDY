import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CourseService } from '../../services/course.service';
import { CourseListItem } from '../../models/course.model';
import { CourseWizardModalComponent } from '../../components/course-wizard-modal/course-wizard-modal.component';
import { DeleteConfirmModalComponent } from '../../../../shared/components/delete-confirm-modal/delete-confirm-modal.component';
import { ToastService } from '../../../../shared/services/toast.service';

const SUBJECT_BG_PALETTE = ['#EEF2FF', '#FEF3C7', '#ECFDF5', '#FEE2E2', '#ECFEFF', '#F3E8FF'];
const DEFAULT_EMOJI = '📘';

@Component({
  selector: 'app-course-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, CourseWizardModalComponent, DeleteConfirmModalComponent],
  templateUrl: './course-list.component.html',
  styleUrl: './course-list.component.scss'
})
export class CourseListComponent implements OnInit {
  @ViewChild(CourseWizardModalComponent) wizard!: CourseWizardModalComponent;
  @ViewChild(DeleteConfirmModalComponent) deleteModal!: DeleteConfirmModalComponent;

  courses: CourseListItem[] = [];
  loading = false;
  deleting: number | null = null;
  pendingDeleteId: number | null = null;
  errorMessage = '';

  page = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;
  searchInput = '';
  keyword = '';

  constructor(
    private courseService: CourseService,
    private route: ActivatedRoute,
    private router: Router,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.loadCourses();

    if (this.route.snapshot.queryParamMap.get('create') === '1') {
      this.wizard.openCreate();
      this.router.navigate([], { queryParams: {} });
    }

    const editId = this.route.snapshot.queryParamMap.get('edit');
    if (editId) {
      this.wizard.openEdit(Number(editId));
      this.router.navigate([], { queryParams: {} });
    }
  }

  openCreateModal(): void {
    this.wizard.openCreate();
  }

  openEditModal(courseId: number): void {
    this.wizard.openEdit(courseId);
  }

  loadCourses(): void {
    this.loading = true;
    this.errorMessage = '';
    this.courseService
      .getCourses(this.page, this.pageSize, this.keyword || undefined)
      .subscribe({
        next: (res) => {
          this.courses = res.data?.items ?? [];
          this.totalCount = res.data?.totalCount ?? 0;
          this.totalPages = res.data?.totalPages ?? 0;
          this.loading = false;
        },
        error: () => {
          this.errorMessage = 'Không thể tải danh sách khóa học. Vui lòng thử lại.';
          this.loading = false;
        }
      });
  }

  onSearch(): void {
    this.keyword = this.searchInput.trim();
    this.page = 1;
    this.loadCourses();
  }

  onPageChange(newPage: number): void {
    if (newPage < 1 || newPage > this.totalPages) return;
    this.page = newPage;
    this.loadCourses();
  }

  openDeleteModal(course: CourseListItem): void {
    this.pendingDeleteId = course.id;
    this.deleteModal.open(course.title);
  }

  onConfirmDelete(): void {
    if (this.pendingDeleteId === null) return;
    const id = this.pendingDeleteId;
    this.deleting = id;
    this.courseService.deleteCourse(id).subscribe({
      next: () => {
        this.deleting = null;
        this.pendingDeleteId = null;
        this.deleteModal.close();
        this.toast.success('Đã xóa khóa học thành công.');
        if (this.courses.length === 1 && this.page > 1) this.page--;
        this.loadCourses();
      },
      error: () => {
        this.deleting = null;
        this.toast.error('Xóa khóa học thất bại. Vui lòng thử lại.');
      }
    });
  }

  get pageNumbers(): number[] {
    const delta = 2;
    const start = Math.max(1, this.page - delta);
    const end = Math.min(this.totalPages, this.page + delta);
    return Array.from({ length: end - start + 1 }, (_, i) => start + i);
  }

  get startIndex(): number {
    return (this.page - 1) * this.pageSize + 1;
  }

  get endIndex(): number {
    return Math.min(this.page * this.pageSize, this.totalCount);
  }

  formatPrice(price: number): string {
    return price === 0 ? 'Miễn phí' : price.toLocaleString('vi-VN') + ' đ';
  }

  getStatusClass(status: string): string {
    if (status === 'Published') return 'status-published';
    if (status === 'Upcoming') return 'status-upcoming';
    return 'status-draft';
  }

  getStatusLabel(status: string): string {
    if (status === 'Published') return 'Đang học';
    if (status === 'Upcoming') return 'Sắp tới';
    return 'Bản nháp';
  }

  getEmoji(course: CourseListItem): string {
    return course.emoji || DEFAULT_EMOJI;
  }

  getSubjectBg(id: number): string {
    return SUBJECT_BG_PALETTE[id % SUBJECT_BG_PALETTE.length];
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('vi-VN');
  }
}
