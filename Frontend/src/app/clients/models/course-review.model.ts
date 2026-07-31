/** Shapes returned by the backend (Backend/DTOs/CourseReviewDto.cs). */

export interface CourseReview {
  id: number;
  courseId: number;
  userId: number;
  studentName: string;
  rating: number;
  comment?: string;
  createdAt: string;
  updatedAt?: string;
  isMine: boolean;
}

export interface CourseRatingBreakdownItem {
  stars: number;
  count: number;
  percent: number;
}

export interface CourseRatingSummary {
  averageRating: number;
  ratingCount: number;
  breakdown: CourseRatingBreakdownItem[];
}

export interface CreateCourseReviewRequest {
  rating: number;
  comment?: string;
}
