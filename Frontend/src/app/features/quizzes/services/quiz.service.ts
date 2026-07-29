import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, QuizQuestionAdmin } from '../../courses/models/course.model';
import { CreateQuizRequest, QuizImportResult, QuizItem, UpdateQuizRequest } from '../models/quiz.model';
import { environment } from '../../../../environments/environment';

/** Talks to Backend/Controllers/QuizzesController.cs — quản lý kho Quiz dùng chung. */
@Injectable({ providedIn: 'root' })
export class QuizLibraryService {
  private readonly baseUrl = `${environment.apiBaseUrl}/quizzes`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<QuizItem[]>> {
    return this.http.get<ApiResponse<QuizItem[]>>(this.baseUrl);
  }

  getById(id: number): Observable<ApiResponse<QuizItem>> {
    return this.http.get<ApiResponse<QuizItem>>(`${this.baseUrl}/${id}`);
  }

  create(dto: CreateQuizRequest): Observable<ApiResponse<QuizItem>> {
    return this.http.post<ApiResponse<QuizItem>>(this.baseUrl, dto);
  }

  update(id: number, dto: UpdateQuizRequest): Observable<ApiResponse<QuizItem>> {
    return this.http.put<ApiResponse<QuizItem>>(`${this.baseUrl}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }

  getQuestions(quizId: number): Observable<ApiResponse<QuizQuestionAdmin[]>> {
    return this.http.get<ApiResponse<QuizQuestionAdmin[]>>(`${this.baseUrl}/${quizId}/questions`);
  }

  replaceQuestions(quizId: number, questions: QuizQuestionAdmin[]): Observable<ApiResponse<null>> {
    return this.http.put<ApiResponse<null>>(`${this.baseUrl}/${quizId}/questions`, { questions });
  }

  importExcel(file: File): Observable<ApiResponse<QuizImportResult>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<QuizImportResult>>(`${this.baseUrl}/import-excel`, formData);
  }

  // Cần dùng HttpClient (thay vì <a href> thô) để request đính kèm Authorization header (Admin-only).
  downloadTemplate(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/import-template`, { responseType: 'blob' });
  }
}
