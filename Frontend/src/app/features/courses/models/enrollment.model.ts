// Backend/DTOs/EnrollmentDto.cs
export interface CourseEnrollment {
  id: number;
  userId: number;
  studentName: string;
  courseId: number;
  courseName: string;
  enrolledAt: string;
  totalLessons: number;
  completedLessons: number;
  progressPercent: number;
}
