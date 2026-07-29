import { Chapter, Lesson, LessonStatus } from '../models/course.model';

const LESSON_TITLES = [
  'Giới thiệu tổng quan',
  'Khái niệm cơ bản',
  'Ví dụ minh họa',
  'Bài tập vận dụng',
  'Phân tích chuyên sâu',
  'Mẹo giải nhanh',
  'Ôn tập & tổng kết',
  'Luyện đề thực chiến',
];

/**
 * Derives a chapter/lesson outline for a course from its lesson count and
 * purchase progress. Kept pure (no Angular deps) so list and detail views
 * always compute lesson status the same way instead of hand-authoring it
 * per mock course.
 */
export function buildChapters(lessonsCount: number, progressPercent: number, purchased: boolean): Chapter[] {
  const chapterCount = Math.max(3, Math.round(lessonsCount / 8));
  const perChapter = Math.ceil(lessonsCount / chapterCount);
  const doneCount = Math.round((lessonsCount * progressPercent) / 100);

  const chapters: Chapter[] = [];
  let lessonCounter = 0;

  for (let chapterIndex = 0; chapterIndex < chapterCount; chapterIndex++) {
    const lessons: Lesson[] = [];
    for (let i = 0; i < perChapter && lessonCounter < lessonsCount; i++) {
      lessonCounter++;
      const done = lessonCounter <= doneCount;
      const locked = !purchased && lessonCounter > 3;
      const status: LessonStatus = done ? 'done' : locked ? 'locked' : 'current';
      lessons.push({
        id: lessonCounter,
        title: `Bài ${lessonCounter}: ${LESSON_TITLES[(lessonCounter - 1) % LESSON_TITLES.length]}`,
        durationMinutes: 8 + (lessonCounter % 5) * 3,
        status,
        lessonType: 'Video',
      });
    }
    chapters.push({
      id: chapterIndex,
      title: `Chương ${chapterIndex + 1}`,
      lessons,
    });
  }

  return chapters;
}

export function findNextLesson(chapters: Chapter[]): Lesson | undefined {
  return chapters.flatMap((chapter) => chapter.lessons).find((lesson) => lesson.status === 'current');
}
