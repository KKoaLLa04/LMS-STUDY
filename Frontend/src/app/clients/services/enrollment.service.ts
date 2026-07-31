import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, of } from 'rxjs';
import { ApiResponse } from '../models/course-api.model';
import { environment } from '../../../environments/environment';

/** Shape returned by Backend/DTOs/EnrollmentDto.cs */
export interface EnrollmentApi {
  id: number;
  userId: number;
  studentName: string;
  courseId: number;
  courseName: string;
  enrolledAt: string;
  /** Only populated by getMyEnrollments()/getMyEnrolledCourseIds() — the by-course
   * (Admin/Teacher) endpoint skips this for a lighter query. */
  totalLessons: number;
  completedLessons: number;
  progressPercent: number;
}

@Injectable({ providedIn: 'root' })
export class EnrollmentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/enrollments`;

  enroll(courseId: number): Observable<ApiResponse<EnrollmentApi>> {
    return this.http.post<ApiResponse<EnrollmentApi>>(this.baseUrl, { courseId });
  }

  unenroll(courseId: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${courseId}`);
  }

  /** Full enrollment list for the current user, including real per-course progress. */
  getMyEnrollments(): Observable<EnrollmentApi[]> {
    return this.http.get<ApiResponse<EnrollmentApi[]>>(`${this.baseUrl}/me`).pipe(
      map((res) => res.data ?? []),
      catchError(() => of([]))
    );
  }

  /** Course ids the current user is enrolled in — used to derive `purchased` on course cards/detail. */
  getMyEnrolledCourseIds(): Observable<Set<number>> {
    return this.getMyEnrollments().pipe(map((items) => new Set(items.map((e) => e.courseId))));
  }
}
