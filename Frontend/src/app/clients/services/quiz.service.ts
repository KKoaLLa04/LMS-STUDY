import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, of, catchError } from 'rxjs';
import { ApiResponse } from '../models/course-api.model';
import { QuizAnswerSubmission, QuizAttemptApi, QuizAttemptResultApi, QuizQuestionApi } from '../models/quiz-api.model';
import { environment } from '../../../environments/environment';

/** Talks to Backend/Controllers/QuizController.cs — student-facing quiz taking flow. */
@Injectable({ providedIn: 'root' })
export class QuizService {
  private readonly http = inject(HttpClient);

  private baseUrl(lessonId: number): string {
    return `${environment.apiBaseUrl}/lessons/${lessonId}/quiz`;
  }

  // Không bao giờ chứa đáp án đúng — chấm điểm hoàn toàn ở server (QuizController.GetQuestions).
  getQuestions(lessonId: number): Observable<QuizQuestionApi[]> {
    return this.http.get<ApiResponse<QuizQuestionApi[]>>(`${this.baseUrl(lessonId)}/questions`).pipe(
      map((res) => res.data ?? []),
      catchError(() => of([]))
    );
  }

  submitAttempt(lessonId: number, answers: QuizAnswerSubmission[]): Observable<QuizAttemptResultApi | null> {
    return this.http.post<ApiResponse<QuizAttemptResultApi>>(`${this.baseUrl(lessonId)}/attempts`, { answers }).pipe(
      map((res) => res.data ?? null),
      catchError(() => of(null))
    );
  }

  // Lịch sử các lần làm quiz trước đó — dùng để không mất dấu vết khi rời trang rồi quay lại.
  getMyAttempts(lessonId: number): Observable<QuizAttemptApi[]> {
    return this.http.get<ApiResponse<QuizAttemptApi[]>>(`${this.baseUrl(lessonId)}/attempts`).pipe(
      map((res) => res.data ?? []),
      catchError(() => of([]))
    );
  }

  // Chi tiết đáp án đã chọn + đúng/sai từng câu của một lần làm cụ thể — dùng để khôi phục lại
  // đúng màn hình kết quả (tô đúng/sai từng câu) khi học sinh tải lại trang.
  getAttemptDetail(lessonId: number, attemptId: number): Observable<QuizAttemptResultApi | null> {
    return this.http.get<ApiResponse<QuizAttemptResultApi>>(`${this.baseUrl(lessonId)}/attempts/${attemptId}`).pipe(
      map((res) => res.data ?? null),
      catchError(() => of(null))
    );
  }

  // Làm Quiz chung trực tiếp, không qua bài học nào — Backend/Controllers/StudentQuizzesController.cs.
  private standaloneUrl(quizId: number): string {
    return `${environment.apiBaseUrl}/student/quizzes/${quizId}`;
  }

  getQuestionsStandalone(quizId: number): Observable<QuizQuestionApi[]> {
    return this.http.get<ApiResponse<QuizQuestionApi[]>>(`${this.standaloneUrl(quizId)}/questions`).pipe(
      map((res) => res.data ?? []),
      catchError(() => of([]))
    );
  }

  submitAttemptStandalone(quizId: number, answers: QuizAnswerSubmission[]): Observable<QuizAttemptResultApi | null> {
    return this.http.post<ApiResponse<QuizAttemptResultApi>>(`${this.standaloneUrl(quizId)}/attempts`, { answers }).pipe(
      map((res) => res.data ?? null),
      catchError(() => of(null))
    );
  }

  getMyAttemptsStandalone(quizId: number): Observable<QuizAttemptApi[]> {
    return this.http.get<ApiResponse<QuizAttemptApi[]>>(`${this.standaloneUrl(quizId)}/attempts`).pipe(
      map((res) => res.data ?? []),
      catchError(() => of([]))
    );
  }

  getAttemptDetailStandalone(quizId: number, attemptId: number): Observable<QuizAttemptResultApi | null> {
    return this.http.get<ApiResponse<QuizAttemptResultApi>>(`${this.standaloneUrl(quizId)}/attempts/${attemptId}`).pipe(
      map((res) => res.data ?? null),
      catchError(() => of(null))
    );
  }
}
