import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, CreateLessonRequest, LessonResponse } from '../models/course.model';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class LessonService {
  private readonly baseUrl = `${environment.apiBaseUrl}/lessons`;

  constructor(private http: HttpClient) {}

  createLesson(dto: CreateLessonRequest): Observable<ApiResponse<LessonResponse>> {
    return this.http.post<ApiResponse<LessonResponse>>(this.baseUrl, dto);
  }

  updateLesson(id: number, dto: Omit<CreateLessonRequest, 'sectionId'>): Observable<ApiResponse<LessonResponse>> {
    return this.http.put<ApiResponse<LessonResponse>>(`${this.baseUrl}/${id}`, dto);
  }

  deleteLesson(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }
}
