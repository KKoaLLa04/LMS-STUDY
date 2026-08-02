import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { CourseService } from '../../services/course.service';
import { EnrollmentService } from '../../services/enrollment.service';
import { CourseDetail } from '../../models/course.model';
import { CourseEnrollment } from '../../models/enrollment.model';
import { UserService } from '../../../users/services/user.service';
import { AppUser } from '../../../users/models/user.model';
import { ToastService } from '../../../../shared/services/toast.service';

const DEFAULT_EMOJI = '📘';

/** Màn hình riêng để Admin/Teacher gán (ghi danh) học sinh vào một khóa học, tách khỏi trang
 * "Xem trước khóa học". Bố cục dạng transfer list 50/50: trái = đã ghi danh, phải = chưa ghi danh,
 * chọn (checkbox) rồi bấm mũi tên để chuyển qua lại — mỗi lần chuyển gọi thẳng API enroll/unenroll. */
@Component({
  selector: 'app-course-students',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './course-students.component.html',
  styleUrl: './course-students.component.scss'
})
export class CourseStudentsComponent implements OnInit {
  course: CourseDetail | null = null;
  loading = false;
  errorMessage = '';

  readonly enrolledStudents = signal<CourseEnrollment[]>([]);
  readonly studentsLoading = signal(false);
  readonly allStudents = signal<AppUser[]>([]);
  readonly allStudentsLoading = signal(false);

  readonly leftSearch = signal('');
  readonly rightSearch = signal('');
  readonly selectedLeftIds = signal<Set<number>>(new Set());
  readonly selectedRightIds = signal<Set<number>>(new Set());
  readonly moving = signal(false);

  readonly enrolledUserIds = computed(() => new Set(this.enrolledStudents().map((e) => e.userId)));

  readonly notEnrolledStudents = computed(() => {
    const enrolledIds = this.enrolledUserIds();
    return this.allStudents().filter((s) => !enrolledIds.has(s.id));
  });

  readonly leftFiltered = computed(() => {
    const query = this.leftSearch().trim().toLowerCase();
    const items = this.enrolledStudents();
    if (!query) return items;
    return items.filter((e) => e.studentName.toLowerCase().includes(query));
  });

  readonly rightFiltered = computed(() => {
    const query = this.rightSearch().trim().toLowerCase();
    const items = this.notEnrolledStudents();
    if (!query) return items;
    return items.filter(
      (s) =>
        s.fullName.toLowerCase().includes(query) ||
        s.email.toLowerCase().includes(query) ||
        s.username.toLowerCase().includes(query)
    );
  });

  readonly allLeftSelected = computed(() => {
    const items = this.leftFiltered();
    return items.length > 0 && items.every((e) => this.selectedLeftIds().has(e.userId));
  });

  readonly allRightSelected = computed(() => {
    const items = this.rightFiltered();
    return items.length > 0 && items.every((s) => this.selectedRightIds().has(s.id));
  });

  constructor(
    private route: ActivatedRoute,
    private courseService: CourseService,
    private enrollmentService: EnrollmentService,
    private userService: UserService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.errorMessage = 'Khóa học không hợp lệ.';
      return;
    }
    this.loadCourse(id);
    this.loadEnrolledStudents(id);

    this.allStudentsLoading.set(true);
    this.userService.getUsers('Student').subscribe({
      next: (res) => {
        this.allStudents.set(res.data ?? []);
        this.allStudentsLoading.set(false);
      },
      error: () => this.allStudentsLoading.set(false)
    });
  }

  loadCourse(id: number): void {
    this.loading = true;
    this.errorMessage = '';
    this.courseService.getCourseById(id).subscribe({
      next: (res) => {
        this.loading = false;
        if (!res.success || !res.data) {
          this.errorMessage = res.message || 'Không tìm thấy khóa học.';
          return;
        }
        this.course = res.data;
      },
      error: () => {
        this.loading = false;
        this.errorMessage = 'Không thể tải dữ liệu khóa học. Vui lòng thử lại.';
      }
    });
  }

  private loadEnrolledStudents(courseId: number): void {
    this.studentsLoading.set(true);
    this.enrollmentService.getByCourse(courseId).subscribe({
      next: (res) => {
        this.enrolledStudents.set(res.data ?? []);
        this.studentsLoading.set(false);
      },
      error: () => this.studentsLoading.set(false)
    });
  }

  isLeftSelected(userId: number): boolean {
    return this.selectedLeftIds().has(userId);
  }

  isRightSelected(userId: number): boolean {
    return this.selectedRightIds().has(userId);
  }

  toggleLeftSelect(userId: number): void {
    this.selectedLeftIds.update((current) => {
      const next = new Set(current);
      if (next.has(userId)) next.delete(userId);
      else next.add(userId);
      return next;
    });
  }

  toggleRightSelect(userId: number): void {
    this.selectedRightIds.update((current) => {
      const next = new Set(current);
      if (next.has(userId)) next.delete(userId);
      else next.add(userId);
      return next;
    });
  }

  toggleSelectAllLeft(): void {
    const items = this.leftFiltered();
    this.selectedLeftIds.set(this.allLeftSelected() ? new Set() : new Set(items.map((e) => e.userId)));
  }

  toggleSelectAllRight(): void {
    const items = this.rightFiltered();
    this.selectedRightIds.set(this.allRightSelected() ? new Set() : new Set(items.map((s) => s.id)));
  }

  /** Chuyển học sinh đã chọn ở danh sách bên phải (chưa ghi danh) sang trái — ghi danh vào khóa học. */
  moveToEnrolled(): void {
    const ids = Array.from(this.selectedRightIds());
    if (!this.course || ids.length === 0 || this.moving()) return;

    const courseId = this.course.id;
    this.moving.set(true);
    forkJoin(ids.map((userId) => this.enrollmentService.adminEnroll(userId, courseId))).subscribe({
      next: () => {
        this.moving.set(false);
        this.selectedRightIds.set(new Set());
        this.toast.success(`Đã gán ${ids.length} học sinh vào khóa học`);
        this.loadEnrolledStudents(courseId);
      },
      error: () => {
        this.moving.set(false);
        this.toast.error('Có lỗi xảy ra khi gán học sinh, vui lòng thử lại');
        this.loadEnrolledStudents(courseId);
      }
    });
  }

  /** Chuyển học sinh đã chọn ở danh sách bên trái (đã ghi danh) sang phải — hủy ghi danh khỏi khóa học. */
  moveToNotEnrolled(): void {
    const ids = Array.from(this.selectedLeftIds());
    if (!this.course || ids.length === 0 || this.moving()) return;

    const courseId = this.course.id;
    this.moving.set(true);
    forkJoin(ids.map((userId) => this.enrollmentService.adminUnenroll(userId, courseId))).subscribe({
      next: () => {
        this.moving.set(false);
        this.selectedLeftIds.set(new Set());
        this.toast.success(`Đã hủy ghi danh ${ids.length} học sinh`);
        this.loadEnrolledStudents(courseId);
      },
      error: () => {
        this.moving.set(false);
        this.toast.error('Có lỗi xảy ra khi hủy ghi danh, vui lòng thử lại');
        this.loadEnrolledStudents(courseId);
      }
    });
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('vi-VN');
  }

  formatPrice(price: number): string {
    return price === 0 ? 'Miễn phí' : price.toLocaleString('vi-VN') + ' đ';
  }

  get emoji(): string {
    return this.course?.emoji || DEFAULT_EMOJI;
  }
}
