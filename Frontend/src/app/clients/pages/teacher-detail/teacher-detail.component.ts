import { AfterViewInit, Component, ElementRef, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OcIconComponent } from '../../components/icon/icon.component';
import { CoursePricePipe } from '../../pipes/course-price.pipe';
import {
  SubjectMeta,
  TaughtCourse,
  TeacherRecord,
  TeacherReview,
  bioOf,
  buildTeacherRecord,
  coursesOf,
  degreeOf,
  fullNameOf,
  reviewsCountOf,
  reviewsOf,
  studentsCountOf,
  subjectMeta,
} from '../teachers/teachers.data';
import { PublicUserService } from '../../../shared/services/public-user.service';
import { FollowService } from '../../../shared/services/follow.service';
import { ToastService } from '../../../shared/services/toast.service';

interface StatCard {
  icon: 'users' | 'book-open' | 'star';
  target: number;
  decimals: number;
  suffix: string;
  label: string;
  bg: string;
  display: string;
}

function initialsOf(name: string): string {
  const parts = name.trim().split(/\s+/).filter(Boolean);
  const first = parts[0]?.[0] ?? '';
  const last = parts[parts.length - 1]?.[0] ?? '';
  return (first + last).toUpperCase();
}

@Component({
  selector: 'app-teacher-detail',
  standalone: true,
  imports: [RouterLink, CoursePricePipe, OcIconComponent],
  templateUrl: './teacher-detail.component.html',
  styleUrl: './teacher-detail.component.scss',
})
export class TeacherDetailComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('statsSection') statsSectionRef?: ElementRef<HTMLElement>;

  private readonly route = inject(ActivatedRoute);
  private readonly publicUserService = inject(PublicUserService);
  private readonly followService = inject(FollowService);
  private readonly toast = inject(ToastService);

  loading = true;
  teacher: TeacherRecord | undefined;

  fullName = '';
  subject: SubjectMeta | undefined;
  bio = '';
  degree = '';
  courses: TaughtCourse[] = [];
  reviews: TeacherReview[] = [];
  reviewsCount = 0;
  initials = '';
  stats: StatCard[] = [];

  isFollowing = false;
  followersCount = 0;
  followBusy = false;

  private viewReady = false;
  private dataReady = false;
  private observer?: IntersectionObserver;
  private counterRaf?: number;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.publicUserService.getById(id).subscribe({
      next: (res) => {
        if (!res.data) {
          this.loading = false;
          return;
        }

        const teacher = buildTeacherRecord(res.data);
        this.teacher = teacher;
        this.fullName = fullNameOf(teacher);
        this.subject = subjectMeta(teacher.subjectKey);
        this.bio = bioOf(teacher);
        this.degree = degreeOf(teacher);
        this.courses = coursesOf(teacher);
        this.reviews = reviewsOf(teacher);
        this.reviewsCount = reviewsCountOf(teacher);
        this.initials = initialsOf(teacher.name);
        this.stats = [
          { icon: 'users', target: studentsCountOf(teacher), decimals: 0, suffix: '+', label: 'Học viên', bg: 'var(--oc-accent-500)', display: '0' },
          { icon: 'book-open', target: teacher.coursesCount, decimals: 0, suffix: '', label: 'Khoá học', bg: 'var(--oc-accent-2-500)', display: '0' },
          { icon: 'star', target: teacher.rating, decimals: 1, suffix: '', label: 'Đánh giá trung bình', bg: 'var(--oc-accent-600)', display: '0' },
        ];

        this.loading = false;
        this.dataReady = true;
        this.maybeStartObserving();
        this.loadFollowStatus(id);
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  private loadFollowStatus(id: number): void {
    this.followService.getStatus(id).subscribe({
      next: (res) => {
        if (!res.data) return;
        this.isFollowing = res.data.isFollowing;
        this.followersCount = res.data.followersCount;
      },
    });
  }

  ngAfterViewInit(): void {
    this.viewReady = true;
    this.maybeStartObserving();
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
    if (this.counterRaf) cancelAnimationFrame(this.counterRaf);
  }

  scrollToCourses(): void {
    document.getElementById('teacher-courses')?.scrollIntoView({ behavior: 'smooth' });
  }

  toggleFollow(): void {
    if (!this.teacher || this.followBusy) return;

    const wasFollowing = this.isFollowing;
    this.followBusy = true;
    const request = wasFollowing
      ? this.followService.unfollow(this.teacher.id)
      : this.followService.follow(this.teacher.id);

    request.subscribe({
      next: () => {
        this.isFollowing = !wasFollowing;
        this.followersCount += wasFollowing ? -1 : 1;
        this.followBusy = false;
        this.toast.success(wasFollowing ? 'Đã bỏ theo dõi' : 'Đã theo dõi giáo viên');
      },
      error: () => {
        this.followBusy = false;
        this.toast.error('Có lỗi xảy ra, vui lòng thử lại');
      },
    });
  }

  private maybeStartObserving(): void {
    if (!this.viewReady || !this.dataReady || this.observer) return;
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

  private startCounter(): void {
    const duration = 1300;
    const start = performance.now();
    const step = (now: number) => {
      const t = Math.min(1, (now - start) / duration);
      const eased = 1 - Math.pow(1 - t, 3);
      this.stats = this.stats.map((s) => ({
        ...s,
        display: (s.target * eased).toLocaleString('vi-VN', { maximumFractionDigits: s.decimals, minimumFractionDigits: s.decimals }) + s.suffix,
      }));
      if (t < 1) this.counterRaf = requestAnimationFrame(step);
    };
    this.counterRaf = requestAnimationFrame(step);
  }
}
