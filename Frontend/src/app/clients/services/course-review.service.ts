import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, PagedResult } from '../models/course-api.model';
import {
  CourseRatingSummary,
  CourseReview,
  CreateCourseReviewRequest,
} from '../models/course-review.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CourseReviewService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/coursereviews`;

  getSummary(courseId: number): Observable<ApiResponse<CourseRatingSummary>> {
    return this.http.get<ApiResponse<CourseRatingSummary>>(`${this.baseUrl}/summary/${courseId}`);
  }

  getByCourse(courseId: number, page = 1, pageSize = 20): Observable<ApiResponse<PagedResult<CourseReview>>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<ApiResponse<PagedResult<CourseReview>>>(`${this.baseUrl}/by-course/${courseId}`, { params });
  }

  submit(courseId: number, dto: CreateCourseReviewRequest): Observable<ApiResponse<CourseReview>> {
    return this.http.post<ApiResponse<CourseReview>>(`${this.baseUrl}/${courseId}`, dto);
  }

  delete(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }
}
