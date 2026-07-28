import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, catchError, of } from 'rxjs';
import { ApiResponse } from '../models/course-api.model';
import { LeaderboardResponseApi, RankPeriod } from '../models/ranking-api.model';
import { environment } from '../../../environments/environment';

const EMPTY_RESPONSE: LeaderboardResponseApi = {
  items: { items: [], totalCount: 0, page: 1, pageSize: 0, totalPages: 0 },
};

/** Talks to Backend/Controllers/RankingController.cs — real per-student point totals. */
@Injectable({ providedIn: 'root' })
export class RankingService {
  private readonly http = inject(HttpClient);
  private readonly rankingUrl = `${environment.apiBaseUrl}/ranking`;

  getLeaderboard(period: RankPeriod, courseId?: number, page = 1, pageSize = 50): Observable<LeaderboardResponseApi> {
    const params: Record<string, string> = { period, page: String(page), pageSize: String(pageSize) };
    if (courseId != null) params['courseId'] = String(courseId);

    return this.http.get<ApiResponse<LeaderboardResponseApi>>(this.rankingUrl, { params }).pipe(
      map((res) => res.data ?? EMPTY_RESPONSE),
      catchError(() => of(EMPTY_RESPONSE))
    );
  }
}
