import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, QuizQuestionAdmin } from '../models/course.model';
import { environment } from '../../../../environments/environment';

/** Talks to Backend/Controllers/QuizQuestionsController.cs (Admin-only, includes correct answers). */
@Injectable({ providedIn: 'root' })
export class QuizQuestionService {
  constructor(private http: HttpClient) {}

  private url(lessonId: number): string {
    return `${environment.apiBaseUrl}/lessons/${lessonId}/quiz-questions`;
  }

  getQuestions(lessonId: number): Observable<ApiResponse<QuizQuestionAdmin[]>> {
    return this.http.get<ApiResponse<QuizQuestionAdmin[]>>(this.url(lessonId));
  }

  replaceQuestions(lessonId: number, questions: QuizQuestionAdmin[]): Observable<ApiResponse<null>> {
    return this.http.put<ApiResponse<null>>(this.url(lessonId), { questions });
  }
}
