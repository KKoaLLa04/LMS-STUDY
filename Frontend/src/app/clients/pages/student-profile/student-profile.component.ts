import { AfterViewInit, Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { OcIconComponent } from '../../components/icon/icon.component';
import { Badge } from '../../models/badge.model';
import { AchievementService } from '../../services/achievement.service';
import { MOCK_BADGES } from '../achievements/achievements.data';

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

const HIGHLIGHT_UNLOCKED_COUNT = 3;
const HIGHLIGHT_LOCKED_COUNT = 3;

@Component({
  selector: 'app-student-profile',
  standalone: true,
  imports: [RouterLink, OcIconComponent],
  templateUrl: './student-profile.component.html',
  styleUrl: './student-profile.component.scss',
})
export class StudentProfileComponent implements OnInit, AfterViewInit, OnDestroy {
  private readonly achievementService = inject(AchievementService);

  readonly student = STUDENT;
  readonly inProgressCourses = COURSES;

  // Seeded with the mock list so the page paints immediately; ngOnInit()
  // reconciles it with the real API response right after.
  private readonly badges = signal<Badge[]>(MOCK_BADGES);

  /** Small highlight reel — a few unlocked badges plus a few in-progress ones,
   * same source of truth as the achievements page (not tied to specific ids,
   * so it stays correct regardless of which badges the backend returns). */
  readonly highlightBadges = computed(() => {
    const all = this.badges();
    const unlocked = all.filter((b) => b.unlocked).slice(0, HIGHLIGHT_UNLOCKED_COUNT);
    const locked = all.filter((b) => !b.unlocked).slice(0, HIGHLIGHT_LOCKED_COUNT);
    return [...unlocked, ...locked];
  });

  readonly pointsDisplay = signal('0');

  private raf?: number;

  ngOnInit(): void {
    this.achievementService.getAchievements().subscribe((badges) => this.badges.set(badges));
  }

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
