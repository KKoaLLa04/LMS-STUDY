import { Chapter, Course, Lesson, ThumbnailVariant } from '../models/course.model';
import { CourseDetailApi, CourseListItemApi, SectionDetailApi } from '../models/course-api.model';
import { formatTeacherInitials } from './format.util';

const FREE_PREVIEW_LESSON_COUNT = 3;

function thumbnailVariantFor(id: number): ThumbnailVariant {
  return id % 2 === 0 ? 'accent' : 'accent-2';
}

/** Maps the list endpoint's summary shape — no chapters/materials/reviews yet
 * (those need the detail call), and rating/enrollment fields aren't tracked
 * by the backend until Phase 2/3, so they default to zero/empty here. */
export function mapCourseListItemToCourse(dto: CourseListItemApi): Course {
  return {
    id: dto.id,
    subject: dto.categoryName ?? 'Khác',
    title: dto.title,
    description: '',
    teacher: dto.teacher ?? 'Chưa cập nhật',
    teacherInitials: formatTeacherInitials(dto.teacher),
    rating: 0,
    ratingCount: 0,
    studentsCount: 0,
    lessonsCount: dto.lessonsCount,
    durationMinutes: dto.durationMinutes,
    price: dto.price,
    purchased: false,
    progressPercent: 0,
    featured: dto.isFeatured,
    thumbnailVariant: thumbnailVariantFor(dto.id),
    chapters: [],
    materials: [],
    reviews: [],
    ratingBreakdown: [],
  };
}

function mapSectionToChapter(section: SectionDetailApi, purchased: boolean): Chapter {
  return {
    id: section.id,
    title: section.title,
    lessons: section.lessons
      .slice()
      .sort((a, b) => a.position - b.position)
      .map(
        (lesson, index): Lesson => ({
          id: lesson.id,
          title: lesson.title,
          durationMinutes: lesson.durationMinutes,
          // Chưa có bảng LessonProgress (Phase 2) nên chưa biết bài nào học sinh đã
          // hoàn thành — chỉ áp dụng quy tắc "học thử 3 bài đầu" cho khóa chưa mua.
          status: !purchased && index >= FREE_PREVIEW_LESSON_COUNT ? 'locked' : 'current',
          videoUrl: lesson.videoUrl,
        })
      ),
  };
}

export function mapCourseDetailToCourse(dto: CourseDetailApi): Course {
  const purchased = false;

  return {
    id: dto.id,
    subject: dto.categoryName ?? 'Khác',
    title: dto.title,
    description: dto.description ?? '',
    teacher: dto.teacher ?? 'Chưa cập nhật',
    teacherInitials: formatTeacherInitials(dto.teacher),
    rating: 0,
    ratingCount: 0,
    studentsCount: 0,
    lessonsCount: dto.lessonsCount,
    durationMinutes: dto.durationMinutes,
    price: dto.price,
    purchased,
    progressPercent: 0,
    featured: dto.isFeatured,
    thumbnailVariant: thumbnailVariantFor(dto.id),
    previewVideoUrl: dto.previewVideoUrl,
    chapters: dto.sections
      .slice()
      .sort((a, b) => a.position - b.position)
      .map((section) => mapSectionToChapter(section, purchased)),
    materials: [],
    reviews: [],
    ratingBreakdown: [],
  };
}
