/** Shapes returned by the backend (Backend/DTOs/CourseDto.cs, SectionDto.cs, LessonDto.cs). */

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CourseListItemApi {
  id: number;
  title: string;
  thumbnail?: string;
  teacher?: string;
  emoji?: string;
  price: number;
  status: string;
  createdAt: string;
  khoiHocId?: number;
  khoiHocName?: string;
  categoryId?: number;
  categoryName?: string;
  isFeatured: boolean;
  lessonsCount: number;
  durationMinutes: number;
}

export interface LessonResponseApi {
  id: number;
  sectionId: number;
  title: string;
  content?: string;
  videoUrl?: string;
  lessonType: string;
  position: number;
  durationMinutes: number;
}

export interface SectionDetailApi {
  id: number;
  courseId: number;
  title: string;
  position: number;
  lessons: LessonResponseApi[];
}

export interface CourseDetailApi extends CourseListItemApi {
  description?: string;
  previewVideoUrl?: string;
  sections: SectionDetailApi[];
}

export interface CourseCategoryApi {
  id: number;
  name: string;
  code: string;
  orderNumber: number;
}
