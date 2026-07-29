import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, of } from 'rxjs';
import { ApiResponse } from '../models/course-api.model';
import { StudentQuizApi } from '../models/quiz-api.model';
import { environment } from '../../../environments/environment';

/** Talks to Backend/Controllers/StudentQuizzesController.cs — danh sách/chi tiết Quiz chung
 * (metadata Title/Description/QuestionCount). Việc làm bài (câu hỏi/nộp bài) do QuizService
 * (quiz.service.ts) + QuizPlayerComponent đảm nhiệm. */
@Injectable({ providedIn: 'root' })
export class StudentQuizLibraryService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/student/quizzes`;

  getAll(): Observable<StudentQuizApi[]> {
    return this.http.get<ApiResponse<StudentQuizApi[]>>(this.baseUrl).pipe(
      map((res) => res.data ?? []),
      catchError(() => of([]))
    );
  }

  getById(id: number): Observable<StudentQuizApi | null> {
    return this.http.get<ApiResponse<StudentQuizApi>>(`${this.baseUrl}/${id}`).pipe(
      map((res) => res.data ?? null),
      catchError(() => of(null))
    );
  }
}
