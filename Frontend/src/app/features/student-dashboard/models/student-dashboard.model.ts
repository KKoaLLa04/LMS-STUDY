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
  | 'tag';

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

export type CourseStatus = 'active' | 'upcoming' | 'draft';

export interface Course {
  emoji: string;
  subjectBg: string;
  name: string;
  teacher: string;
  nextLesson: string;
  progress: number;
  progressColor: string;
  status: CourseStatus;
  statusLabel: string;
  statusBg: string;
  statusColor: string;
  actionLabel: string;
  actionBg: string;
  actionColor: string;
}

export interface Deadline {
  title: string;
  due: string;
  dotColor: string;
}

export interface Activity {
  icon: IconName;
  iconColor: string;
  iconBg: string;
  iconStrokeWidth: number;
  text: string;
  time: string;
}
