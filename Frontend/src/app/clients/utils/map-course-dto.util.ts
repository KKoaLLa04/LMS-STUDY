import { Chapter, Course, Lesson, LessonKind, LessonStatus, ThumbnailVariant } from '../models/course.model';
import { CourseDetailApi, CourseListItemApi, SectionDetailApi } from '../models/course-api.model';
import { formatTeacherInitials } from './format.util';
import { LessonProgressApi } from '../services/lesson-progress.service';

const FREE_PREVIEW_LESSON_COUNT = 3;

function thumbnailVariantFor(id: number): ThumbnailVariant {
  return id % 2 === 0 ? 'accent' : 'accent-2';
}

/** Maps the list endpoint's summary shape — no chapters/materials/reviews yet
 * (those need the detail call). rating/studentsCount come from the backend
 * (real Enrollment/CourseReview counts); purchased/progressPercent are derived
 * from the caller's own enrollment list since the courses endpoint is shared
 * with Admin and has no per-user context. */
export function mapCourseListItemToCourse(
  dto: CourseListItemApi,
  progressByCourseId: ReadonlyMap<number, number> = new Map()
): Course {
  return {
    id: dto.id,
    subject: dto.categoryName ?? 'Khác',
    title: dto.title,
    description: '',
    status: dto.status,
    teacher: dto.teacher ?? 'Chưa cập nhật',
    teacherInitials: formatTeacherInitials(dto.teacher),
    rating: dto.rating,
    ratingCount: dto.ratingCount,
    studentsCount: dto.studentsCount,
    lessonsCount: dto.lessonsCount,
    durationMinutes: dto.durationMinutes,
    price: dto.price,
    purchased: progressByCourseId.has(dto.id),
    progressPercent: progressByCourseId.get(dto.id) ?? 0,
    featured: dto.isFeatured,
    thumbnailVariant: thumbnailVariantFor(dto.id),
    thumbnail: dto.thumbnail,
    chapters: [],
    materials: [],
    reviews: [],
    ratingBreakdown: [],
  };
}

function mapSectionToChapter(
  section: SectionDetailApi,
  purchased: boolean,
  isPaidCourse: boolean,
  completedLessonIds: ReadonlySet<number>
): Chapter {
  return {
    id: section.id,
    title: section.title,
    lessons: section.lessons
      .slice()
      .sort((a, b) => a.position - b.position)
      .map((lesson, index): Lesson => {
        const lessonType = (lesson.lessonType as LessonKind) ?? 'Video';
        // Chưa gắn LessonProgress thật vào luồng xem khóa học ở đây — chỉ áp dụng quy tắc
        // "học thử 3 bài đầu" cho khóa chưa mua (progress theo từng bài là việc client gọi
        // LessonProgressController riêng khi học, không ảnh hưởng tới khóa/mở bài ở đây).
        // Riêng video của khóa MẤT PHÍ: luôn khóa tới khi ghi danh, không áp dụng học thử —
        // tránh xem hết nội dung video mà không cần mua khóa.
        const locked =
          !purchased && ((isPaidCourse && lessonType === 'Video') || index >= FREE_PREVIEW_LESSON_COUNT);
        const status: LessonStatus = completedLessonIds.has(lesson.id) ? 'done' : locked ? 'locked' : 'current';
        return {
          id: lesson.id,
          title: lesson.title,
          durationMinutes: lesson.durationMinutes,
          status,
          lessonType,
          videoUrl: lesson.videoUrl,
          content: lesson.content,
          documentUrl: lesson.documentUrl,
        };
      }),
  };
}

export function mapCourseDetailToCourse(
  dto: CourseDetailApi,
  enrolledCourseIds: ReadonlySet<number> = new Set(),
  lessonProgress: LessonProgressApi[] = []
): Course {
  const purchased = enrolledCourseIds.has(dto.id);
  const completedLessonIds = new Set(lessonProgress.filter((p) => p.isCompleted).map((p) => p.lessonId));

  const chapters = dto.sections
    .slice()
    .sort((a, b) => a.position - b.position)
    .map((section) => mapSectionToChapter(section, purchased, dto.price > 0, completedLessonIds));

  const totalLessons = chapters.reduce((sum, c) => sum + c.lessons.length, 0);
  const progressPercent = totalLessons === 0 ? 0 : Math.round((completedLessonIds.size / totalLessons) * 100);

  return {
    id: dto.id,
    subject: dto.categoryName ?? 'Khác',
    title: dto.title,
    description: dto.description ?? '',
    status: dto.status,
    teacher: dto.teacher ?? 'Chưa cập nhật',
    teacherInitials: formatTeacherInitials(dto.teacher),
    rating: dto.rating,
    ratingCount: dto.ratingCount,
    studentsCount: dto.studentsCount,
    lessonsCount: dto.lessonsCount,
    durationMinutes: dto.durationMinutes,
    price: dto.price,
    purchased,
    progressPercent,
    featured: dto.isFeatured,
    thumbnailVariant: thumbnailVariantFor(dto.id),
    thumbnail: dto.thumbnail,
    previewVideoUrl: dto.previewVideoUrl,
    chapters,
    materials: [],
    reviews: [],
    ratingBreakdown: [],
  };
}
