import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, of, catchError } from 'rxjs';
import { Badge } from '../models/badge.model';
import { AchievementApi, MyAchievementApi } from '../models/achievement-api.model';
import { ApiResponse } from '../models/course-api.model';
import { mapAchievementDtoToBadge, mapMyAchievementDtoToBadge } from '../utils/map-achievement-dto.util';
import { environment } from '../../../environments/environment';

/**
 * Client-facing achievements catalogue — talks to the admin API
 * (Backend/Controllers/AchievementsController.cs).
 */
@Injectable({ providedIn: 'root' })
export class AchievementService {
  private readonly http = inject(HttpClient);
  private readonly achievementsUrl = `${environment.apiBaseUrl}/achievements`;

  getAchievements(): Observable<Badge[]> {
    return this.http.get<ApiResponse<AchievementApi[]>>(this.achievementsUrl).pipe(
      map((res) => (res.data ?? []).map(mapAchievementDtoToBadge)),
      catchError(() => of([]))
    );
  }

  /** Real per-student unlock status (requires login) — see AchievementsController.GetMyAchievements. */
  getMyAchievements(): Observable<Badge[]> {
    return this.http.get<ApiResponse<MyAchievementApi[]>>(`${this.achievementsUrl}/me`).pipe(
      map((res) => (res.data ?? []).map(mapMyAchievementDtoToBadge)),
      catchError(() => of([]))
    );
  }
}
