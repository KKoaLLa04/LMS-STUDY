import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/achievement.model';
import { AchievementConditionType } from '../models/achievement-condition-type.model';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AchievementConditionService {
  private readonly baseUrl = `${environment.apiBaseUrl}/achievement-conditions`;

  constructor(private http: HttpClient) {}

  getTypes(): Observable<ApiResponse<AchievementConditionType[]>> {
    return this.http.get<ApiResponse<AchievementConditionType[]>>(`${this.baseUrl}/types`);
  }
}
