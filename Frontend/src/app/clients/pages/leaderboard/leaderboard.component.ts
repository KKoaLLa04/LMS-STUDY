import { Component, computed, signal } from '@angular/core';
import { trigger, transition, style, animate, query, stagger } from '@angular/animations';
import { OcIconComponent } from '../../components/icon/icon.component';
import { OcIconName } from '../../models/icon-name.type';

type RankChange = 'up' | 'down' | 'same';

interface LeaderboardEntry {
  rank: number;
  name: string;
  grade: string;
  subtitle: string;
  points: number;
  change: RankChange;
  delta: number;
  isMe?: boolean;
}

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

const GRADES = ['Tất cả khối', 'Khối 1', 'Khối 2', 'Khối 3', 'Khối 4', 'Khối 5'];

const PERIODS: Array<{ key: LeaderboardPeriod; label: string }> = [
  { key: 'week', label: 'Tuần này' },
  { key: 'month', label: 'Tháng này' },
  { key: 'all', label: 'Toàn thời gian' },
];

/** Mock data for now — will be swapped for a real ranking endpoint once the
 * backend tracks XP/points per student. */
const ENTRIES: LeaderboardEntry[] = [
  { rank: 1, name: 'Minh Anh', grade: 'Khối 4', subtitle: 'Khối 4 • Tiếng Anh Vui', points: 3120, change: 'up', delta: 1 },
  { rank: 2, name: 'Gia Bảo', grade: 'Khối 3', subtitle: 'Khối 3 • Toán Sáng Tạo', points: 2840, change: 'same', delta: 0 },
  { rank: 3, name: 'Thảo Vy', grade: 'Khối 2', subtitle: 'Khối 2 • Ngữ Văn 9', points: 2610, change: 'up', delta: 2 },
  { rank: 4, name: 'Đức Huy', grade: 'Khối 3', subtitle: 'Khối 3 • Toán học lớp 3', points: 2400, change: 'up', delta: 2 },
  { rank: 5, name: 'Ngọc Hân', grade: 'Khối 5', subtitle: 'Khối 5 • Tiếng Anh giao tiếp', points: 2280, change: 'down', delta: 1 },
  { rank: 6, name: 'Quang Minh', grade: 'Khối 4', subtitle: 'Khối 4 • Hóa Học Vui', points: 2150, change: 'same', delta: 0 },
  { rank: 7, name: 'Bảo Trân', grade: 'Khối 2', subtitle: 'Khối 2 • Ngữ Văn 9', points: 2010, change: 'up', delta: 3 },
  { rank: 8, name: 'Anh Tuấn', grade: 'Khối 1', subtitle: 'Khối 1 • Vật Lý Khám Phá', points: 1920, change: 'down', delta: 2 },
  { rank: 9, name: 'Khánh Linh', grade: 'Khối 3', subtitle: 'Khối 3 • Sinh Học 10', points: 1840, change: 'up', delta: 1 },
  { rank: 10, name: 'Hữu Phát', grade: 'Khối 5', subtitle: 'Khối 5 • Toán học lớp 5', points: 1790, change: 'same', delta: 0 },
  { rank: 11, name: 'Thu Trang', grade: 'Khối 2', subtitle: 'Khối 2 • Tiếng Anh giao tiếp', points: 1700, change: 'up', delta: 4 },
  { rank: 12, name: 'Việt Hoàng', grade: 'Khối 4', subtitle: 'Khối 4 • Lịch Sử Việt Nam', points: 1650, change: 'down', delta: 1 },
  { rank: 13, name: 'Kim Ngân', grade: 'Khối 1', subtitle: 'Khối 1 • Hóa Học Vui', points: 1580, change: 'same', delta: 0 },
  { rank: 14, name: 'Bảo Anh', grade: 'Khối 3', subtitle: 'Khối 3 • Toán học lớp 3', points: 1520, change: 'up', delta: 2, isMe: true },
  { rank: 15, name: 'Nhật Nam', grade: 'Khối 2', subtitle: 'Khối 2 • Ngữ Văn 9', points: 1470, change: 'down', delta: 1 },
];

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
  readonly grades = GRADES;
  readonly periods = PERIODS;

  readonly activeGrade = signal(GRADES[0]);
  readonly activePeriod = signal<LeaderboardPeriod>('week');

  /** Podium visual order is 2nd-1st-3rd (center-stage first place). */
  readonly podium = computed(() =>
    [2, 1, 3].map((rank) => ENTRIES.find((e) => e.rank === rank)).filter((e): e is LeaderboardEntry => !!e)
  );

  readonly restList = computed(() => ENTRIES.filter((e) => e.rank > 3));

  readonly me = ENTRIES.find((e) => e.isMe);

  setGrade(grade: string): void {
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
}
