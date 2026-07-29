import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AchievementGroup, CreateAchievementGroupRequest } from '../models/achievement-group.model';
import { ApiResponse } from '../models/achievement.model';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AchievementGroupService {
  private readonly baseUrl = `${environment.apiBaseUrl}/achievement-groups`;

  constructor(private http: HttpClient) {}

  getGroups(): Observable<ApiResponse<AchievementGroup[]>> {
    return this.http.get<ApiResponse<AchievementGroup[]>>(this.baseUrl);
  }

  createGroup(dto: CreateAchievementGroupRequest): Observable<ApiResponse<AchievementGroup>> {
    return this.http.post<ApiResponse<AchievementGroup>>(this.baseUrl, dto);
  }
}
