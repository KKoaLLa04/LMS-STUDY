import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { OcIconComponent } from '../../components/icon/icon.component';
import { Badge } from '../../models/badge.model';
import { AchievementService } from '../../services/achievement.service';
import { RankingService } from '../../services/ranking.service';
import { EnrollmentService } from '../../services/enrollment.service';
import { AuthService } from '../../../core/auth/auth.service';

interface InProgressCourse {
  title: string;
  progress: number;
  lessonsLeft: number;
}

interface StudentSummary {
  name: string;
  level: number;
  rankTitle: string;
  totalPoints: number;
  currentRank: number;
}

const DEFAULT_STUDENT: StudentSummary = {
  name: '',
  level: 1,
  rankTitle: '',
  totalPoints: 0,
  currentRank: 0,
};

// Backend chưa có khái niệm "Level"/"rankTitle" — đây là các bậc cảm quan (cosmetic)
// suy ra từ tổng điểm thật (GET /api/ranking), không phải dữ liệu backend trả về.
const RANK_TITLES: Array<{ minPoints: number; title: string }> = [
  { minPoints: 4000, title: 'Học sinh xuất sắc' },
  { minPoints: 2000, title: 'Học sinh chăm chỉ' },
  { minPoints: 500, title: 'Học sinh tích cực' },
  { minPoints: 0, title: 'Người mới bắt đầu' },
];

function levelFromPoints(points: number): number {
  return Math.floor(points / 500) + 1;
}

function rankTitleFromPoints(points: number): string {
  return RANK_TITLES.find((t) => points >= t.minPoints)?.title ?? RANK_TITLES[RANK_TITLES.length - 1].title;
}

const IN_PROGRESS_DISPLAY_COUNT = 4;

const HIGHLIGHT_UNLOCKED_COUNT = 3;
const HIGHLIGHT_LOCKED_COUNT = 3;

@Component({
  selector: 'app-student-profile',
  standalone: true,
  imports: [RouterLink, OcIconComponent],
  templateUrl: './student-profile.component.html',
  styleUrl: './student-profile.component.scss',
})
export class StudentProfileComponent implements OnInit, OnDestroy {
  private readonly achievementService = inject(AchievementService);
  private readonly rankingService = inject(RankingService);
  private readonly enrollmentService = inject(EnrollmentService);
  private readonly authService = inject(AuthService);

  readonly student = signal<StudentSummary>(DEFAULT_STUDENT);
  readonly inProgressCourses = signal<InProgressCourse[]>([]);

  private readonly badges = signal<Badge[]>([]);

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
    this.achievementService.getMyAchievements().subscribe((badges) => this.badges.set(badges));

    // "Đang học" = đã ghi danh, có ít nhất 1 bài học, chưa hoàn thành 100%.
    this.enrollmentService.getMyEnrollments().subscribe((enrollments) => {
      const inProgress = enrollments
        .filter((e) => e.totalLessons > 0 && e.progressPercent > 0 && e.progressPercent < 100)
        .sort((a, b) => new Date(b.enrolledAt).getTime() - new Date(a.enrolledAt).getTime())
        .slice(0, IN_PROGRESS_DISPLAY_COUNT)
        .map((e) => ({
          title: e.courseName,
          progress: e.progressPercent,
          lessonsLeft: e.totalLessons - e.completedLessons,
        }));
      this.inProgressCourses.set(inProgress);
    });

    this.authService.getCurrentUser().subscribe((user) => {
      if (user) this.student.update((s) => ({ ...s, name: user.fullName }));
    });

    // Hạng/điểm toàn thời gian, toàn hệ thống — cùng nguồn dữ liệu với trang bảng xếp hạng.
    this.rankingService.getLeaderboard('all').subscribe((res) => {
      const me = res.me;
      if (!me) return;
      this.student.update((s) => ({
        ...s,
        totalPoints: me.totalPoints,
        currentRank: me.rank,
        level: levelFromPoints(me.totalPoints),
        rankTitle: rankTitleFromPoints(me.totalPoints),
      }));
      this.animatePointsTo(me.totalPoints);
    });
  }

  private animatePointsTo(target: number): void {
    if (this.raf) cancelAnimationFrame(this.raf);
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
