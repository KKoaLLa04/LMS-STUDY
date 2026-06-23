import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, CourseDetail, CourseListItem, CreateCourseRequest, PagedResult } from '../models/course.model';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CourseService {
  private readonly baseUrl = `${environment.apiBaseUrl}/courses`;

  constructor(private http: HttpClient) {}

  getCourses(
    page = 1,
    pageSize = 10,
    keyword?: string
  ): Observable<ApiResponse<PagedResult<CourseListItem>>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (keyword) params = params.set('keyword', keyword);
    return this.http.get<ApiResponse<PagedResult<CourseListItem>>>(this.baseUrl, { params });
  }

  getCourseById(id: number): Observable<ApiResponse<CourseDetail>> {
    return this.http.get<ApiResponse<CourseDetail>>(`${this.baseUrl}/${id}`);
  }

  createCourse(dto: CreateCourseRequest): Observable<ApiResponse<CourseDetail>> {
    return this.http.post<ApiResponse<CourseDetail>>(this.baseUrl, dto);
  }

  updateCourse(id: number, dto: CreateCourseRequest): Observable<ApiResponse<CourseDetail>> {
    return this.http.put<ApiResponse<CourseDetail>>(`${this.baseUrl}/${id}`, dto);
  }

  deleteCourse(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }
}
