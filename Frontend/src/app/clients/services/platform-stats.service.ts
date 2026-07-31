import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/course-api.model';
import { PlatformStats } from '../models/platform-stats.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PlatformStatsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/platformstats`;

  getStats(): Observable<ApiResponse<PlatformStats>> {
    return this.http.get<ApiResponse<PlatformStats>>(this.baseUrl);
  }
}
