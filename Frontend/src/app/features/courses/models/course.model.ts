export interface CreateCourseRequest {
  title: string;
  description?: string;
  thumbnail?: string;
  price: number;
  status: string;
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
  lessonType: string;
  position: number;
}

export interface CourseListItem {
  id: number;
  title: string;
  thumbnail?: string;
  price: number;
  status: string;
  createdAt: string;
}

export interface LessonResponse {
  id: number;
  sectionId: number;
  title: string;
  content?: string;
  videoUrl?: string;
  lessonType: string;
  position: number;
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
  price: number;
  status: string;
  createdAt: string;
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
  httpStatusCode: number;
  message: string;
  data?: T;
}
