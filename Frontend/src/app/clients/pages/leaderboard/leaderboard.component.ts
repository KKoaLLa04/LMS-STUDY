import { Component, computed, effect, inject, signal } from '@angular/core';
import { trigger, transition, style, animate, query, stagger } from '@angular/animations';
import { OcIconComponent } from '../../components/icon/icon.component';
import { OcIconName } from '../../models/icon-name.type';
import { RankingService } from '../../services/ranking.service';
import { RankingEntryApi } from '../../models/ranking-api.model';
import { KhoiHocService } from '../../services/khoi-hoc.service';
import { FollowService } from '../../../shared/services/follow.service';
import { ToastService } from '../../../shared/services/toast.service';

type RankChange = 'up' | 'down' | 'same';

interface LeaderboardEntry {
  rank: number;
  userId: number;
  name: string;
  subtitle: string;
  points: number;
  change: RankChange;
  delta: number;
  isMe?: boolean;
  isFollowing: boolean;
  followBusy?: boolean;
}

interface GradeFilter {
  /** undefined = "Tất cả khối" (no filter). */
  khoiHocId?: number;
  label: string;
}

const ALL_GRADES: GradeFilter = { label: 'Tất cả khối' };

type LeaderboardPeriod = 'week' | 'month' | 'all';

interface RankMeta {
  ring: string;
  podiumGradient: string;
  height: number;
  avatarSize: number;
}

const RANK_META: Record<number, RankMeta> = {
  1: { ring: '#FFD700', podiumGradient: 'linear-gradient(160deg, #FFD700, #E6B800)', height: 120, avatarSize: 92 },
  2: { ring: '#C0C0C0', podiumGradient: 'linear-gradient(160deg, #C0C0C0, #9A9A9A)', height: 84, avatarSize: 72 },
  3: { ring: '#CD7F32', podiumGradient: 'linear-gradient(160deg, #CD7F32, #A8672A)', height: 64, avatarSize: 72 },
};

/** Cosmetic avatar gradient, cycled by rank — purely decorative since there
 * are no real student photos yet. */
const AVATAR_GRADIENTS = [
  'linear-gradient(135deg, var(--oc-accent-400), var(--oc-accent-700))',
  'linear-gradient(135deg, var(--oc-accent-2-400), var(--oc-accent-2-700))',
  'linear-gradient(135deg, var(--oc-accent-300), var(--oc-accent-600))',
  'linear-gradient(135deg, var(--oc-accent-2-300), var(--oc-accent-2-600))',
];

const PERIODS: Array<{ key: LeaderboardPeriod; label: string }> = [
  { key: 'week', label: 'Tuần này' },
  { key: 'month', label: 'Tháng này' },
  { key: 'all', label: 'Toàn thời gian' },
];

// Điểm số + hạng kỳ trước (previousRank) đều lấy thật từ GET /api/ranking — previousRank được
// backend tính lại trực tiếp từ lịch sử PointTransactions (không cần bảng snapshot riêng).
// change/delta suy ra từ so sánh rank hiện tại với previousRank ngay tại đây.
function mapRankingEntry(e: RankingEntryApi): LeaderboardEntry {
  const hasPrevious = e.previousRank != null;
  const delta = hasPrevious ? e.previousRank! - e.rank : 0;
  return {
    rank: e.rank,
    userId: e.userId,
    name: e.fullName,
    subtitle: e.gradeName ?? '',
    points: e.totalPoints,
    change: delta > 0 ? 'up' : delta < 0 ? 'down' : 'same',
    delta: Math.abs(delta),
    isMe: e.isMe,
    isFollowing: e.isFollowing,
  };
}

const listStagger = trigger('listStagger', [
  transition('* => *', [
    query(
      ':enter',
      [
        style({ opacity: 0, transform: 'translateY(10px)' }),
        stagger('40ms', [animate('220ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))]),
      ],
      { optional: true }
    ),
  ]),
]);

function initialsOf(name: string): string {
  const parts = name.trim().split(/\s+/).filter(Boolean);
  if (parts.length === 0) return '?';
  const first = parts[0][0] ?? '';
  const last = parts[parts.length - 1][0] ?? '';
  return (first + last).toUpperCase();
}

@Component({
  selector: 'app-leaderboard',
  standalone: true,
  imports: [OcIconComponent],
  templateUrl: './leaderboard.component.html',
  styleUrl: './leaderboard.component.scss',
  animations: [listStagger],
})
export class LeaderboardComponent {
  private readonly rankingService = inject(RankingService);
  private readonly khoiHocService = inject(KhoiHocService);
  private readonly followService = inject(FollowService);
  private readonly toast = inject(ToastService);

  readonly periods = PERIODS;

  // Danh sách khối thật (Backend/Models/KhoiHoc.cs), "Tất cả khối" luôn đứng đầu.
  readonly grades = signal<GradeFilter[]>([ALL_GRADES]);
  readonly activeGrade = signal<GradeFilter>(ALL_GRADES);
  readonly activePeriod = signal<LeaderboardPeriod>('week');

  readonly entries = signal<LeaderboardEntry[]>([]);
  readonly me = signal<LeaderboardEntry | undefined>(undefined);
  readonly loading = signal(false);

  /** Podium visual order is 2nd-1st-3rd (center-stage first place). */
  readonly podium = computed(() => {
    const list = this.entries();
    return [2, 1, 3].map((rank) => list.find((e) => e.rank === rank)).filter((e): e is LeaderboardEntry => !!e);
  });

  readonly restList = computed(() => this.entries().filter((e) => e.rank > 3));

  constructor() {
    this.khoiHocService.getKhoiHocs().subscribe((khoiHocs) => {
      this.grades.set([ALL_GRADES, ...khoiHocs.map((k) => ({ khoiHocId: k.id, label: k.name }))]);
    });

    // Tải lại bảng xếp hạng thật từ backend mỗi khi đổi mốc thời gian hoặc khối.
    effect(() => {
      const period = this.activePeriod();
      const khoiHocId = this.activeGrade().khoiHocId;
      this.loading.set(true);
      this.rankingService.getLeaderboard(period, undefined, khoiHocId).subscribe((res) => {
        this.entries.set(res.items.items.map(mapRankingEntry));
        this.me.set(res.me ? mapRankingEntry(res.me) : undefined);
        this.loading.set(false);
      });
    });
  }

  setGrade(grade: GradeFilter): void {
    this.activeGrade.set(grade);
  }

  setPeriod(period: LeaderboardPeriod): void {
    this.activePeriod.set(period);
  }

  periodIndex(period: LeaderboardPeriod): number {
    return this.periods.findIndex((p) => p.key === period);
  }

  rankMeta(rank: number): RankMeta {
    return RANK_META[rank] ?? { ring: '#E2E8F0', podiumGradient: '#E2E8F0', height: 60, avatarSize: 70 };
  }

  avatarGradient(rank: number): string {
    return AVATAR_GRADIENTS[rank % AVATAR_GRADIENTS.length];
  }

  initials(name: string): string {
    return initialsOf(name);
  }

  changeIcon(entry: LeaderboardEntry): OcIconName {
    if (entry.change === 'up') return 'arrow-up';
    if (entry.change === 'down') return 'arrow-down';
    return 'minus';
  }

  changeColor(entry: LeaderboardEntry): string {
    if (entry.change === 'up') return '#16A34A';
    if (entry.change === 'down') return '#DC2626';
    return '#94A3B8';
  }

  changeLabel(entry: LeaderboardEntry): string {
    if (entry.delta <= 0) return '';
    return entry.change === 'up' ? `+${entry.delta}` : `-${entry.delta}`;
  }

  formatPoints(points: number): string {
    return `${points.toLocaleString('vi-VN')} điểm`;
  }

  toggleFollow(entry: LeaderboardEntry, event: Event): void {
    event.stopPropagation();
    if (entry.followBusy) return;

    const wasFollowing = entry.isFollowing;
    this.patchEntry(entry.userId, { followBusy: true });
    const request = wasFollowing
      ? this.followService.unfollow(entry.userId)
      : this.followService.follow(entry.userId);

    request.subscribe({
      next: () => {
        this.patchEntry(entry.userId, { isFollowing: !wasFollowing, followBusy: false });
        this.toast.success(wasFollowing ? 'Đã bỏ theo dõi' : 'Đã theo dõi');
      },
      error: () => {
        this.patchEntry(entry.userId, { followBusy: false });
        this.toast.error('Có lỗi xảy ra, vui lòng thử lại');
      },
    });
  }

  private patchEntry(userId: number, patch: Partial<LeaderboardEntry>): void {
    this.entries.update((list) => list.map((e) => (e.userId === userId ? { ...e, ...patch } : e)));
    if (this.me()?.userId === userId) {
      this.me.update((m) => (m ? { ...m, ...patch } : m));
    }
  }
}
