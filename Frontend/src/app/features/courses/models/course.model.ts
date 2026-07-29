export type CourseStatus = 'Draft' | 'Published' | 'Upcoming';

export interface CreateCourseRequest {
  title: string;
  description?: string;
  thumbnail?: string;
  teacher?: string;
  emoji?: string;
  price: number;
  status: string;
  khoiHocId?: number;
  categoryId?: number;
  isFeatured?: boolean;
  previewVideoUrl?: string;
}

export interface CreateSectionRequest {
  courseId: number;
  title: string;
  position: number;
}

export interface CreateLessonRequest {
  sectionId: number;
  title: string;
  content?: string;
  videoUrl?: string;
  documentUrl?: string;
  lessonType: string;
  position: number;
  durationMinutes: number;
  // Gắn với Tài liệu/Quiz chung có sẵn (kho dùng chung) thay vì tạo nội dung riêng cho bài học này.
  documentId?: number;
  quizId?: number;
}

export interface CourseListItem {
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
}

export interface LessonResponse {
  id: number;
  sectionId: number;
  title: string;
  content?: string;
  videoUrl?: string;
  documentUrl?: string;
  lessonType: string;
  position: number;
  durationMinutes: number;
  documentId?: number;
  quizId?: number;
}

// Backend/DTOs/LessonAdminListItemDto.cs — dùng cho trang quản trị "Tài liệu"/"Quiz" độc lập.
export interface LessonAdminListItem {
  id: number;
  title: string;
  lessonType: string;
  content?: string;
  videoUrl?: string;
  documentUrl?: string;
  position: number;
  durationMinutes: number;
  sectionId: number;
  sectionTitle: string;
  courseId: number;
  courseTitle: string;
  questionCount: number;
}

// Backend/DTOs/QuizQuestionDto.cs — QuizQuestionAdminDto/QuizOptionAdminDto (có IsCorrect).
export interface QuizOptionAdmin {
  id?: number;
  text: string;
  isCorrect: boolean;
  orderNumber: number;
}

export interface QuizQuestionAdmin {
  id?: number;
  text: string;
  orderNumber: number;
  allowMultipleAnswers: boolean;
  options: QuizOptionAdmin[];
}

export interface SectionDetail {
  id: number;
  courseId: number;
  title: string;
  position: number;
  lessons: LessonResponse[];
}

export interface CourseDetail {
  id: number;
  title: string;
  description?: string;
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
  previewVideoUrl?: string;
  sections: SectionDetail[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  httpStatusCode?: number;
}
