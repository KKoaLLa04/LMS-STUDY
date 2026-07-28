import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map, of, catchError } from 'rxjs';
import { Course } from '../models/course.model';
import {
  ApiResponse,
  CourseCategoryApi,
  CourseDetailApi,
  CourseListItemApi,
  PagedResult,
} from '../models/course-api.model';
import { mapCourseDetailToCourse, mapCourseListItemToCourse } from '../utils/map-course-dto.util';
import { environment } from '../../../environments/environment';

export const CATEGORY_ALL = 'Tất cả';

/**
 * Client-facing course catalogue — talks to the same admin API
 * (Backend/Controllers/CoursesController.cs, CourseCategoriesController.cs)
 * now that GET endpoints accept any authenticated role, not just Admin.
 * Enrollment/progress/rating/materials fields aren't tracked by the backend
 * yet (Phase 2/3 of the rollout plan), so the mapper defaults those to
 * zero/empty for now.
 */
@Injectable({ providedIn: 'root' })
export class ClientCourseService {
  private readonly http = inject(HttpClient);
  private readonly coursesUrl = `${environment.apiBaseUrl}/courses`;
  private readonly categoriesUrl = `${environment.apiBaseUrl}/coursecategories`;

  getCategories(): Observable<string[]> {
    return this.http.get<ApiResponse<CourseCategoryApi[]>>(this.categoriesUrl).pipe(
      map((res) => [CATEGORY_ALL, ...(res.data ?? []).map((c) => c.name)]),
      catchError(() => of([CATEGORY_ALL]))
    );
  }

  getCourses(): Observable<Course[]> {
    const params = new HttpParams().set('page', 1).set('pageSize', 100);
    return this.http.get<ApiResponse<PagedResult<CourseListItemApi>>>(this.coursesUrl, { params }).pipe(
      map((res) => (res.data?.items ?? []).map(mapCourseListItemToCourse)),
      catchError(() => of([]))
    );
  }

  getCourseById(id: number): Observable<Course | undefined> {
    return this.http.get<ApiResponse<CourseDetailApi>>(`${this.coursesUrl}/${id}`).pipe(
      map((res) => (res.data ? mapCourseDetailToCourse(res.data) : undefined)),
      catchError(() => of(undefined))
    );
  }
}
