import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Course } from '../../models/course.model';
import { OcIconName } from '../../models/icon-name.type';
import { OcIconComponent } from '../../components/icon/icon.component';
import { CourseCardComponent } from '../../components/course-card/course-card.component';

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

/** Mock data for now — will be swapped for ClientCourseService calls once
 * the backend exposes featured-courses/teachers/stats endpoints. */
const MOCK_COURSES: Course[] = [
  {
    id: 101,
    subject: 'Toán',
    title: 'Toán học lớp 10 - Đại số & Hình học',
    description: 'Nắm chắc nền tảng đại số và hình học lớp 10 qua các ví dụ trực quan.',
    teacher: 'Thầy Nguyễn Văn An',
    teacherInitials: 'NA',
    rating: 4.8,
    ratingCount: 214,
    studentsCount: 3560,
    lessonsCount: 48,
    durationMinutes: 720,
    price: 299000,
    purchased: false,
    progressPercent: 0,
    featured: true,
    thumbnailVariant: 'accent',
    chapters: [],
    materials: [],
    reviews: [],
    ratingBreakdown: [],
  },
  {
    id: 102,
    subject: 'Hóa Học',
    title: 'Hóa Học Vô Cơ 12 - Ôn thi THPT',
    description: 'Hệ thống hóa kiến thức vô cơ trọng tâm, luyện đề sát cấu trúc thi THPT.',
    teacher: 'Thầy Lê Minh Khoa',
    teacherInitials: 'LK',
    rating: 4.9,
    ratingCount: 189,
    studentsCount: 2870,
    lessonsCount: 40,
    durationMinutes: 600,
    price: 349000,
    purchased: false,
    progressPercent: 0,
    featured: true,
    thumbnailVariant: 'accent-2',
    chapters: [],
    materials: [],
    reviews: [],
    ratingBreakdown: [],
  },
  {
    id: 103,
    subject: 'Tiếng Anh',
    title: 'Tiếng Anh Giao Tiếp THCS',
    description: 'Luyện phản xạ giao tiếp tiếng Anh tự nhiên qua các tình huống hằng ngày.',
    teacher: 'Cô Vũ Ngọc Lan',
    teacherInitials: 'VL',
    rating: 4.9,
    ratingCount: 302,
    studentsCount: 5120,
    lessonsCount: 56,
    durationMinutes: 840,
    price: 329000,
    purchased: false,
    progressPercent: 0,
    featured: true,
    thumbnailVariant: 'accent',
    chapters: [],
    materials: [],
    reviews: [],
    ratingBreakdown: [],
  },
  {
    id: 104,
    subject: 'Vật Lý',
    title: 'Vật Lý 11 - Điện học & Từ trường',
    description: 'Giải thích trực quan các hiện tượng điện, từ kèm bài tập minh họa.',
    teacher: 'Cô Trần Thị Bích',
    teacherInitials: 'TB',
    rating: 4.6,
    ratingCount: 128,
    studentsCount: 1940,
    lessonsCount: 36,
    durationMinutes: 540,
    price: 279000,
    purchased: false,
    progressPercent: 0,
    featured: true,
    thumbnailVariant: 'accent-2',
    chapters: [],
    materials: [],
    reviews: [],
    ratingBreakdown: [],
  },
  {
    id: 105,
    subject: 'Ngữ Văn',
    title: 'Ngữ Văn 9 - Nghị luận xã hội',
    description: 'Rèn kỹ năng lập luận, dẫn chứng để viết bài nghị luận xã hội thuyết phục.',
    teacher: 'Cô Phạm Thu Hà',
    teacherInitials: 'PH',
    rating: 4.7,
    ratingCount: 96,
    studentsCount: 1420,
    lessonsCount: 28,
    durationMinutes: 420,
    price: 249000,
    purchased: false,
    progressPercent: 0,
    featured: true,
    thumbnailVariant: 'accent',
    chapters: [],
    materials: [],
    reviews: [],
    ratingBreakdown: [],
  },
  {
    id: 106,
    subject: 'Sinh Học',
    title: 'Sinh Học 10 - Tế bào & Di truyền',
    description: 'Khám phá thế giới tế bào và các quy luật di truyền qua hình ảnh sinh động.',
    teacher: 'Thầy Đỗ Quang Huy',
    teacherInitials: 'ĐH',
    rating: 4.5,
    ratingCount: 74,
    studentsCount: 980,
    lessonsCount: 32,
    durationMinutes: 480,
    price: 259000,
    purchased: false,
    progressPercent: 0,
    featured: true,
    thumbnailVariant: 'accent-2',
    chapters: [],
    materials: [],
    reviews: [],
    ratingBreakdown: [],
  },
];

const MOCK_CATEGORIES: HomeCategory[] = [
  { name: 'Toán', count: 128, icon: 'math', bg: 'var(--oc-accent-500)' },
  { name: 'Vật Lý', count: 84, icon: 'atom', bg: 'var(--oc-accent-2-500)' },
  { name: 'Hóa Học', count: 76, icon: 'flask', bg: 'var(--oc-accent-600)' },
  { name: 'Ngữ Văn', count: 92, icon: 'book-open', bg: 'var(--oc-accent-2-600)' },
  { name: 'Sinh Học', count: 58, icon: 'leaf', bg: 'var(--oc-accent-400)' },
  { name: 'Tiếng Anh', count: 210, icon: 'globe', bg: 'var(--oc-accent-2-400)' },
  { name: 'Lịch Sử', count: 45, icon: 'clock', bg: 'var(--oc-accent-700)' },
  { name: 'Địa Lý', count: 39, icon: 'map-pin', bg: 'var(--oc-accent-2-700)' },
];

const MOCK_TEACHERS: HomeTeacher[] = [
  { name: 'Nguyễn Văn An', subject: 'Giáo viên Toán', initials: 'NA', rating: 4.8 },
  { name: 'Trần Thị Bích', subject: 'Giáo viên Vật Lý', initials: 'TB', rating: 4.6 },
  { name: 'Lê Minh Khoa', subject: 'Giáo viên Hóa Học', initials: 'LK', rating: 4.9 },
  { name: 'Vũ Ngọc Lan', subject: 'Giáo viên Tiếng Anh', initials: 'VL', rating: 4.9 },
];

const MOCK_TESTIMONIALS: HomeTestimonial[] = [
  { name: 'Minh Anh', role: 'Học sinh lớp 11', bg: 'var(--oc-accent-500)', comment: 'Bài giảng rất dễ hiểu, em theo kịp ngay từ buổi đầu!' },
  { name: 'Gia Bảo', role: 'Học sinh lớp 9', bg: 'var(--oc-accent-2-500)', comment: 'Nhiều bài tập thực hành, học mà như chơi vậy đó.' },
  { name: 'Thảo Vy', role: 'Phụ huynh', bg: 'var(--oc-accent-600)', comment: 'Con tôi tiến bộ rõ rệt chỉ sau 2 tháng học.' },
];

function formatStatCount(n: number): string {
  return n >= 1000 ? Math.round(n / 1000) + 'k' : String(n);
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, OcIconComponent, CourseCardComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent implements AfterViewInit, OnDestroy {
  @ViewChild('statsSection') statsSectionRef?: ElementRef<HTMLElement>;

  readonly categories = MOCK_CATEGORIES;
  readonly courses = MOCK_COURSES;
  readonly teachers = MOCK_TEACHERS;
  readonly testimonials = MOCK_TESTIMONIALS;

  stats: HomeStat[] = [
    { icon: 'users', target: 120000, suffix: '+', label: 'Học viên', display: '0' },
    { icon: 'book-open', target: 850, suffix: '+', label: 'Khoá học', display: '0' },
    { icon: 'graduation-cap', target: 210, suffix: '+', label: 'Giáo viên', display: '0' },
    { icon: 'clock', target: 45000, suffix: '+', label: 'Giờ học', display: '0' },
  ];

  private observer?: IntersectionObserver;
  private counterRaf?: number;

  ngAfterViewInit(): void {
    const el = this.statsSectionRef?.nativeElement;
    if (!el) return;
    this.observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((e) => e.isIntersecting)) {
          this.startCounter();
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
      this.stats = this.stats.map((s) => ({ ...s, display: formatStatCount(Math.round(s.target * eased)) + s.suffix }));
      if (t < 1) this.counterRaf = requestAnimationFrame(step);
    };
    this.counterRaf = requestAnimationFrame(step);
  }
}
