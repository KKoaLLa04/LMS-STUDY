import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, CourseCategory, CreateCourseCategoryRequest } from '../models/course-category.model';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CourseCategoryService {
  private readonly baseUrl = `${environment.apiBaseUrl}/coursecategories`;

  constructor(private http: HttpClient) {}

  getCategories(): Observable<ApiResponse<CourseCategory[]>> {
    return this.http.get<ApiResponse<CourseCategory[]>>(this.baseUrl);
  }

  getCategoryById(id: number): Observable<ApiResponse<CourseCategory>> {
    return this.http.get<ApiResponse<CourseCategory>>(`${this.baseUrl}/${id}`);
  }

  createCategory(dto: CreateCourseCategoryRequest): Observable<ApiResponse<CourseCategory>> {
    return this.http.post<ApiResponse<CourseCategory>>(this.baseUrl, dto);
  }

  updateCategory(id: number, dto: CreateCourseCategoryRequest): Observable<ApiResponse<CourseCategory>> {
    return this.http.put<ApiResponse<CourseCategory>>(`${this.baseUrl}/${id}`, dto);
  }

  deleteCategory(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }
}
