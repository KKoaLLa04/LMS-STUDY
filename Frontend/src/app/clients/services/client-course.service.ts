import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map, of, catchError, forkJoin } from 'rxjs';
import { Course } from '../models/course.model';
import {
  ApiResponse,
  CourseCategoryApi,
  CourseDetailApi,
  CourseListItemApi,
  PagedResult,
} from '../models/course-api.model';
import { mapCourseDetailToCourse, mapCourseListItemToCourse } from '../utils/map-course-dto.util';
import { EnrollmentService } from './enrollment.service';
import { LessonProgressService } from './lesson-progress.service';
import { environment } from '../../../environments/environment';

export const CATEGORY_ALL = 'Tất cả';

/**
 * Client-facing course catalogue — talks to the same admin API
 * (Backend/Controllers/CoursesController.cs, CourseCategoriesController.cs)
 * now that GET endpoints accept any authenticated role, not just Admin.
 * `purchased` is derived by cross-referencing GET /api/enrollments/me since the
 * shared courses endpoint has no per-user context; materials aren't tracked by
 * the backend yet (Phase 2/3 of the rollout plan) so the mapper defaults those
 * to empty for now.
 */
@Injectable({ providedIn: 'root' })
export class ClientCourseService {
  private readonly http = inject(HttpClient);
  private readonly enrollmentService = inject(EnrollmentService);
  private readonly lessonProgressService = inject(LessonProgressService);
  private readonly coursesUrl = `${environment.apiBaseUrl}/courses`;
  private readonly categoriesUrl = `${environment.apiBaseUrl}/coursecategories`;

  /** Search query shared between the client shell's header and the course list page. */
  readonly searchQuery = signal('');

  getCategories(): Observable<string[]> {
    return this.http.get<ApiResponse<CourseCategoryApi[]>>(this.categoriesUrl).pipe(
      map((res) => [CATEGORY_ALL, ...(res.data ?? []).map((c) => c.name)]),
      catchError(() => of([CATEGORY_ALL]))
    );
  }

  getCourses(): Observable<Course[]> {
    const params = new HttpParams().set('page', 1).set('pageSize', 100);
    return forkJoin({
      courses: this.http.get<ApiResponse<PagedResult<CourseListItemApi>>>(this.coursesUrl, { params }).pipe(
        map((res) => res.data?.items ?? []),
        catchError(() => of([] as CourseListItemApi[]))
      ),
      // GetMyEnrollmentsAsync ở backend đã tính sẵn progressPercent thật theo LessonProgress —
      // dùng luôn thay vì chỉ lấy id để card/khay "Đang học dở" hiển thị đúng % hoàn thành.
      enrollments: this.enrollmentService.getMyEnrollments(),
    }).pipe(
      map(({ courses, enrollments }) => {
        const progressByCourseId = new Map(enrollments.map((e) => [e.courseId, e.progressPercent]));
        return courses.map((dto) => mapCourseListItemToCourse(dto, progressByCourseId));
      })
    );
  }

  getCourseById(id: number): Observable<Course | undefined> {
    return forkJoin({
      course: this.http.get<ApiResponse<CourseDetailApi>>(`${this.coursesUrl}/${id}`).pipe(
        map((res) => res.data),
        catchError(() => of(undefined))
      ),
      enrolledCourseIds: this.enrollmentService.getMyEnrolledCourseIds(),
      lessonProgress: this.lessonProgressService.getMyProgressForCourse(id),
    }).pipe(
      map(({ course, enrolledCourseIds, lessonProgress }) =>
        course ? mapCourseDetailToCourse(course, enrolledCourseIds, lessonProgress) : undefined
      )
    );
  }
}
