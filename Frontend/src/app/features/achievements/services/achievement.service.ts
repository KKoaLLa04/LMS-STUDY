import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Achievement, ApiResponse, CreateAchievementRequest } from '../models/achievement.model';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AchievementService {
  private readonly baseUrl = `${environment.apiBaseUrl}/achievements`;

  constructor(private http: HttpClient) {}

  getAchievements(): Observable<ApiResponse<Achievement[]>> {
    return this.http.get<ApiResponse<Achievement[]>>(this.baseUrl);
  }

  getAchievementById(id: number): Observable<ApiResponse<Achievement>> {
    return this.http.get<ApiResponse<Achievement>>(`${this.baseUrl}/${id}`);
  }

  createAchievement(dto: CreateAchievementRequest): Observable<ApiResponse<Achievement>> {
    return this.http.post<ApiResponse<Achievement>>(this.baseUrl, dto);
  }

  updateAchievement(id: number, dto: CreateAchievementRequest): Observable<ApiResponse<Achievement>> {
    return this.http.put<ApiResponse<Achievement>>(`${this.baseUrl}/${id}`, dto);
  }

  deleteAchievement(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }

  unlockForUser(achievementId: number, userId: number): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(`${this.baseUrl}/${achievementId}/unlock/${userId}`, {});
  }
}
