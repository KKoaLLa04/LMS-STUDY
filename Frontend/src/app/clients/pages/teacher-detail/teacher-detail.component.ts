import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OcIconComponent } from '../../components/icon/icon.component';
import { CoursePricePipe } from '../../pipes/course-price.pipe';
import {
  TeacherRecord,
  bioOf,
  coursesOf,
  degreeOf,
  findTeacher,
  fullNameOf,
  reviewsCountOf,
  reviewsOf,
  studentsCountOf,
  subjectMeta,
} from '../teachers/teachers.data';

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
export class TeacherDetailComponent implements AfterViewInit, OnDestroy {
  @ViewChild('statsSection') statsSectionRef?: ElementRef<HTMLElement>;

  private readonly route = inject(ActivatedRoute);

  readonly teacher: TeacherRecord | undefined = findTeacher(Number(this.route.snapshot.paramMap.get('id')));

  readonly fullName = this.teacher ? fullNameOf(this.teacher) : '';
  readonly subject = this.teacher ? subjectMeta(this.teacher.subjectKey) : undefined;
  readonly bio = this.teacher ? bioOf(this.teacher) : '';
  readonly degree = this.teacher ? degreeOf(this.teacher) : '';
  readonly courses = this.teacher ? coursesOf(this.teacher) : [];
  readonly reviews = this.teacher ? reviewsOf(this.teacher) : [];
  readonly reviewsCount = this.teacher ? reviewsCountOf(this.teacher) : 0;
  readonly initials = this.teacher ? initialsOf(this.teacher.name) : '';

  stats: StatCard[] = this.teacher
    ? [
        { icon: 'users', target: studentsCountOf(this.teacher), decimals: 0, suffix: '+', label: 'Học viên', bg: 'var(--oc-accent-500)', display: '0' },
        { icon: 'book-open', target: this.teacher.coursesCount, decimals: 0, suffix: '', label: 'Khoá học', bg: 'var(--oc-accent-2-500)', display: '0' },
        { icon: 'star', target: this.teacher.rating, decimals: 1, suffix: '', label: 'Đánh giá trung bình', bg: 'var(--oc-accent-600)', display: '0' },
      ]
    : [];

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

  scrollToCourses(): void {
    document.getElementById('teacher-courses')?.scrollIntoView({ behavior: 'smooth' });
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
