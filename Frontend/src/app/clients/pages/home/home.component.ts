import { AfterViewInit, Component, ElementRef, OnDestroy, OnInit, ViewChild, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { Course } from '../../models/course.model';
import { OcIconName } from '../../models/icon-name.type';
import { OcIconComponent } from '../../components/icon/icon.component';
import { CourseCardComponent } from '../../components/course-card/course-card.component';
import { ClientCourseService } from '../../services/client-course.service';
import { PlatformStatsService } from '../../services/platform-stats.service';
import { PublicUserService } from '../../../shared/services/public-user.service';
import { buildTeacherRecord, fullNameOf, PublicTeacher } from '../teachers/teachers.data';
import { formatTeacherInitials } from '../../utils/format.util';

interface HomeCategory {
  name: string;
  count: number;
  icon: OcIconName;
  bg: string;
}

interface HomeTeacher {
  name: string;
  subject: string;
  initials: string;
  rating: number;
}

interface HomeTestimonial {
  name: string;
  role: string;
  comment: string;
  bg: string;
}

interface HomeStat {
  icon: OcIconName;
  target: number;
  suffix: string;
  label: string;
  display: string;
}

// Chưa có bảng Testimonial/cảm nhận học viên ở backend — giữ mock cho khối này
// theo quyết định của nhóm (ưu tiên các phần có dữ liệu thật trước).
const MOCK_TESTIMONIALS: HomeTestimonial[] = [
  { name: 'Minh Anh', role: 'Học sinh lớp 11', bg: 'var(--oc-accent-500)', comment: 'Bài giảng rất dễ hiểu, em theo kịp ngay từ buổi đầu!' },
  { name: 'Gia Bảo', role: 'Học sinh lớp 9', bg: 'var(--oc-accent-2-500)', comment: 'Nhiều bài tập thực hành, học mà như chơi vậy đó.' },
  { name: 'Thảo Vy', role: 'Phụ huynh', bg: 'var(--oc-accent-600)', comment: 'Con tôi tiến bộ rõ rệt chỉ sau 2 tháng học.' },
];

const INITIAL_STATS: HomeStat[] = [
  { icon: 'users', target: 0, suffix: '+', label: 'Học viên', display: '0' },
  { icon: 'book-open', target: 0, suffix: '+', label: 'Khoá học', display: '0' },
  { icon: 'graduation-cap', target: 0, suffix: '+', label: 'Giáo viên', display: '0' },
  { icon: 'clock', target: 0, suffix: '+', label: 'Giờ học', display: '0' },
];

// Backend chỉ lưu tên danh mục (CourseCategory không có icon/màu) — icon/màu ở đây thuần
// là lựa chọn trình bày, tra theo tên danh mục thật; danh mục lạ dùng icon mặc định.
const CATEGORY_ICONS: Record<string, { icon: OcIconName; bg: string }> = {
  'Toán': { icon: 'math', bg: 'var(--oc-accent-500)' },
  'Vật Lý': { icon: 'atom', bg: 'var(--oc-accent-2-500)' },
  'Hóa Học': { icon: 'flask', bg: 'var(--oc-accent-600)' },
  'Ngữ Văn': { icon: 'book-open', bg: 'var(--oc-accent-2-600)' },
  'Sinh Học': { icon: 'leaf', bg: 'var(--oc-accent-400)' },
  'Tiếng Anh': { icon: 'globe', bg: 'var(--oc-accent-2-400)' },
  'Lịch Sử': { icon: 'clock', bg: 'var(--oc-accent-700)' },
  'Địa Lý': { icon: 'map-pin', bg: 'var(--oc-accent-2-700)' },
};
const DEFAULT_CATEGORY_ICON: { icon: OcIconName; bg: string } = { icon: 'book-open', bg: 'var(--oc-accent-500)' };

function buildCategories(courses: Course[]): HomeCategory[] {
  const counts = new Map<string, number>();
  for (const c of courses) counts.set(c.subject, (counts.get(c.subject) ?? 0) + 1);
  return Array.from(counts.entries())
    .sort((a, b) => b[1] - a[1])
    .map(([name, count]) => ({ name, count, ...(CATEGORY_ICONS[name] ?? DEFAULT_CATEGORY_ICON) }));
}

// Môn dạy/rating giờ lấy thật: Subject từ User (admin nhập), rating tính từ Course.rating thật
// của các khóa học giáo viên đang dạy (khớp theo tên) — cùng hàm dùng ở trang Giáo viên.
function toHomeTeacher(pu: PublicTeacher, courses: Course[]): HomeTeacher {
  const record = buildTeacherRecord(pu, courses);
  return {
    name: fullNameOf(record),
    subject: record.subject ? `Giáo viên ${record.subject}` : 'Giáo viên',
    initials: formatTeacherInitials(pu.fullName),
    rating: record.rating,
  };
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, OcIconComponent, CourseCardComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('statsSection') statsSectionRef?: ElementRef<HTMLElement>;

  private readonly courseService = inject(ClientCourseService);
  private readonly statsService = inject(PlatformStatsService);
  private readonly publicUserService = inject(PublicUserService);

  readonly categories = signal<HomeCategory[]>([]);
  readonly courses = signal<Course[]>([]);
  readonly teachers = signal<HomeTeacher[]>([]);
  readonly testimonials = MOCK_TESTIMONIALS;
  readonly stats = signal<HomeStat[]>(INITIAL_STATS);

  private observer?: IntersectionObserver;
  private counterRaf?: number;
  private sectionVisible = false;
  private statsTargetsLoaded = false;

  ngOnInit(): void {
    forkJoin({
      courses: this.courseService.getCourses(),
      teachers: this.publicUserService.getTeachers(),
    }).subscribe(({ courses, teachers }) => {
      // Admin nhận cả khóa Draft từ API (courses endpoint dùng chung với trang quản trị) —
      // Trang chủ là mặt tiền công khai nên phải lọc Published để khớp với
      // PlatformStatsService (backend luôn chỉ đếm Published cho ô "Khoá học").
      const publishedCourses = courses.filter((c) => c.status === 'Published');
      this.courses.set(publishedCourses.filter((c) => c.featured).slice(0, 6));
      this.categories.set(buildCategories(publishedCourses));
      this.teachers.set(
        (teachers.data ?? []).slice(0, 4).map((pu) => toHomeTeacher(pu, publishedCourses))
      );
    });

    this.statsService.getStats().subscribe((res) => {
      if (!res.data) return;
      const d = res.data;
      this.stats.set([
        { icon: 'users', target: d.studentsCount, suffix: '+', label: 'Học viên', display: '0' },
        { icon: 'book-open', target: d.coursesCount, suffix: '+', label: 'Khoá học', display: '0' },
        { icon: 'graduation-cap', target: d.teachersCount, suffix: '+', label: 'Giáo viên', display: '0' },
        { icon: 'clock', target: d.contentHours, suffix: '+', label: 'Giờ học', display: '0' },
      ]);
      this.statsTargetsLoaded = true;
      if (this.sectionVisible) this.startCounter();
    });
  }

  ngAfterViewInit(): void {
    const el = this.statsSectionRef?.nativeElement;
    if (!el) return;
    this.observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((e) => e.isIntersecting)) {
          this.sectionVisible = true;
          if (this.statsTargetsLoaded) this.startCounter();
          this.observer?.disconnect();
        }
      },
      { threshold: 0.3 }
    );
    this.observer.observe(el);
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
    if (this.counterRaf) cancelAnimationFrame(this.counterRaf);
  }

  private startCounter(): void {
    const duration = 1400;
    const start = performance.now();
    const step = (now: number) => {
      const t = Math.min(1, (now - start) / duration);
      const eased = 1 - Math.pow(1 - t, 3);
      this.stats.update((list) => list.map((s) => ({ ...s, display: formatStatCount(Math.round(s.target * eased)) + s.suffix })));
      if (t < 1) this.counterRaf = requestAnimationFrame(step);
    };
    this.counterRaf = requestAnimationFrame(step);
  }
}

function formatStatCount(n: number): string {
  return n >= 1000 ? Math.round(n / 1000) + 'k' : String(n);
}
