import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, of, catchError } from 'rxjs';
import { Badge } from '../models/badge.model';
import { AchievementApi } from '../models/achievement-api.model';
import { ApiResponse } from '../models/course-api.model';
import { mapAchievementDtoToBadge } from '../utils/map-achievement-dto.util';
import { MOCK_BADGES } from '../pages/achievements/achievements.data';
import { environment } from '../../../environments/environment';

/**
 * Client-facing achievements catalogue — talks to the admin API
 * (Backend/Controllers/AchievementsController.cs). Falls back to the local
 * mock list if the API is unreachable, same pattern as ClientCourseService.
 */
@Injectable({ providedIn: 'root' })
export class AchievementService {
  private readonly http = inject(HttpClient);
  private readonly achievementsUrl = `${environment.apiBaseUrl}/achievements`;

  getAchievements(): Observable<Badge[]> {
    return this.http.get<ApiResponse<AchievementApi[]>>(this.achievementsUrl).pipe(
      // An empty catalogue (fresh DB, nothing seeded yet) falls back to the
      // mock list too, so the page never renders blank.
      map((res) => (res.data ?? []).map(mapAchievementDtoToBadge)),
      map((list) => (list.length > 0 ? list : MOCK_BADGES)),
      catchError(() => of(MOCK_BADGES))
    );
  }
}
