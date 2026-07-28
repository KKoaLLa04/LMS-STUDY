export interface CourseCategory {
  id: number;
  name: string;
  code: string;
  orderNumber: number;
}

export interface CreateCourseCategoryRequest {
  name: string;
  code: string;
  orderNumber: number;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  httpStatusCode?: number;
}
