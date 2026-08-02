import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, of } from 'rxjs';
import { ApiResponse } from '../models/course-api.model';
import { environment } from '../../../environments/environment';

/** Shape returned by Backend/DTOs/LessonProgressDto.cs */
export interface LessonProgressApi {
  lessonId: number;
  lessonTitle: string;
  watchedSeconds: number;
  isCompleted: boolean;
  completedAt?: string;
}

@Injectable({ providedIn: 'root' })
export class LessonProgressService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/lessonprogress`;

  /** Gọi định kỳ từ video player (timeupdate/pause/ended) — backend tự đánh dấu hoàn thành
   * khi watchedSeconds đạt >= 90% thời lượng bài học và cộng điểm tương ứng. */
  updateProgress(lessonId: number, watchedSeconds: number): Observable<ApiResponse<LessonProgressApi>> {
    return this.http.put<ApiResponse<LessonProgressApi>>(`${this.baseUrl}/${lessonId}`, { watchedSeconds });
  }

  getMyProgressForCourse(courseId: number): Observable<LessonProgressApi[]> {
    return this.http.get<ApiResponse<LessonProgressApi[]>>(`${this.baseUrl}/course/${courseId}`).pipe(
      map((res) => res.data ?? []),
      catchError(() => of([]))
    );
  }
}
