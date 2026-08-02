export type IconName =
  | 'home'
  | 'book'
  | 'check-square'
  | 'award'
  | 'calendar'
  | 'settings'
  | 'droplet'
  | 'star'
  | 'check'
  | 'message-circle'
  | 'menu'
  | 'bell'
  | 'graduation-cap'
  | 'clock'
  | 'layers'
  | 'log-out'
  | 'tag'
  | 'users'
  | 'file-text'
  | 'help-circle'
  | 'shield';

/** Still consumed by MOCK_STUDENT (student-shell header avatar/streak pill) — unrelated to this page. */
export interface Student {
  name: string;
  grade: string;
  level: number;
  xpPercent: number;
  streakDays: number;
}

export interface Stat {
  icon: IconName;
  value: string;
  label: string;
  note: string;
  gradientFrom: string;
  gradientTo: string;
  noteColor: string;
  noteBg: string;
}

export type DashboardCourseStatus = 'Draft' | 'Published' | 'Upcoming';

export interface DashboardCourse {
  id: number;
  emoji: string;
  subjectBg: string;
  name: string;
  teacher: string;
  status: DashboardCourseStatus;
  statusLabel: string;
  statusBg: string;
  statusColor: string;
  createdLabel: string;
}

export interface LeaderboardEntry {
  rank: number;
  name: string;
  initials: string;
  points: number;
}

export interface Activity {
  icon: IconName;
  iconColor: string;
  iconBg: string;
  iconStrokeWidth: number;
  text: string;
  time: string;
}
