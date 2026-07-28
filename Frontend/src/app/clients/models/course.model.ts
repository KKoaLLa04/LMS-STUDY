export type LessonStatus = 'done' | 'current' | 'locked';

export interface Lesson {
  id: number;
  title: string;
  durationMinutes: number;
  status: LessonStatus;
  videoUrl?: string;
}

export interface Chapter {
  id: number;
  title: string;
  lessons: Lesson[];
}

export interface CourseMaterial {
  id: number;
  name: string;
  sizeLabel: string;
  url?: string;
}

export interface Review {
  id: number;
  studentName: string;
  initials: string;
  rating: number;
  createdAt: Date;
  comment: string;
}

export interface RatingBreakdownItem {
  stars: number;
  percent: number;
}

/** Alternates the card thumbnail gradient; purely cosmetic, index-derived. */
export type ThumbnailVariant = 'accent' | 'accent-2';

export interface Course {
  id: number;
  subject: string;
  title: string;
  description: string;
  teacher: string;
  teacherInitials: string;
  rating: number;
  ratingCount: number;
  studentsCount: number;
  lessonsCount: number;
  durationMinutes: number;
  price: number;
  purchased: boolean;
  /** 0-100, only meaningful when purchased is true. */
  progressPercent: number;
  featured: boolean;
  thumbnailVariant: ThumbnailVariant;
  previewVideoUrl?: string;
  chapters: Chapter[];
  materials: CourseMaterial[];
  reviews: Review[];
  ratingBreakdown: RatingBreakdownItem[];
}
