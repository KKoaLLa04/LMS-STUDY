import { Course } from '../models/course.model';
import { findNextLesson } from './chapters.util';

/** True when the student bought the course but hasn't finished it — the
 * "Đang học dở" (in-progress) shelf on the list page shows exactly these. */
export function isCourseInProgress(course: Course): boolean {
  return course.purchased && course.progressPercent > 0 && course.progressPercent < 100;
}

export function courseCtaLabel(course: Course): string {
  if (course.progressPercent >= 100) return 'Xem lại khóa học';
  const nextLesson = findNextLesson(course.chapters);
  return nextLesson ? `Tiếp tục: ${nextLesson.title}` : 'Bắt đầu học';
}
