import { AfterViewInit, Component, OnDestroy, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { OcIconComponent } from '../../components/icon/icon.component';
import { BADGES } from '../achievements/achievements.data';

interface InProgressCourse {
  title: string;
  progress: number;
  lessonsLeft: number;
}

/** Mock data for now — this mirrors the current session's fictional student
 * (same identity used as "Bạn" on the leaderboard and achievements pages). */
const STUDENT = {
  name: 'Bảo Anh',
  level: 12,
  rankTitle: 'Học sinh chăm chỉ',
  totalPoints: 4280,
  currentRank: 14,
};

const COURSES: InProgressCourse[] = [
  { title: 'Toán học lớp 10 - Đại số & Hình học', progress: 64, lessonsLeft: Math.round(48 * 0.36) },
  { title: 'Hóa Học Vô Cơ 12 - Ôn thi THPT', progress: 88, lessonsLeft: Math.round(40 * 0.12) },
  { title: 'Sinh Học 10 - Tế bào & Di truyền', progress: 40, lessonsLeft: Math.round(32 * 0.6) },
];

/** Small highlight reel of badges — same source as the achievements page. */
const HIGHLIGHT_BADGE_IDS = ['b1', 'b4', 'b7', 'b3', 'b5', 'b2'];

@Component({
  selector: 'app-student-profile',
  standalone: true,
  imports: [RouterLink, OcIconComponent],
  templateUrl: './student-profile.component.html',
  styleUrl: './student-profile.component.scss',
})
export class StudentProfileComponent implements AfterViewInit, OnDestroy {
  readonly student = STUDENT;
  readonly inProgressCourses = COURSES;
  readonly highlightBadges = HIGHLIGHT_BADGE_IDS.map((id) => BADGES.find((b) => b.id === id)).filter(
    (b): b is (typeof BADGES)[number] => !!b
  );

  readonly pointsDisplay = signal('0');

  private raf?: number;

  ngAfterViewInit(): void {
    const target = this.student.totalPoints;
    const duration = 1200;
    const start = performance.now();
    const step = (now: number) => {
      const t = Math.min(1, (now - start) / duration);
      const eased = 1 - Math.pow(1 - t, 3);
      this.pointsDisplay.set(Math.round(target * eased).toLocaleString('vi-VN'));
      if (t < 1) this.raf = requestAnimationFrame(step);
    };
    this.raf = requestAnimationFrame(step);
  }

  ngOnDestroy(): void {
    if (this.raf) cancelAnimationFrame(this.raf);
  }

  medalGradient(unlocked: boolean): string {
    return unlocked
      ? 'linear-gradient(135deg, #ffd76a, #ff9d4d)'
      : 'linear-gradient(135deg, var(--oc-neutral-400), var(--oc-neutral-500))';
  }
}
