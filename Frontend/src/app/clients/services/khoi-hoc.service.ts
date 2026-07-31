import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, of } from 'rxjs';
import { ApiResponse } from '../models/course-api.model';
import { environment } from '../../../environments/environment';

/** Shape returned by Backend/DTOs/KhoiHocDto.cs (GET is open to any authenticated role). */
export interface KhoiHocApi {
  id: number;
  name: string;
  code: string;
  orderNumber: number;
}

@Injectable({ providedIn: 'root' })
export class KhoiHocService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/khoihocs`;

  getKhoiHocs(): Observable<KhoiHocApi[]> {
    return this.http.get<ApiResponse<KhoiHocApi[]>>(this.baseUrl).pipe(
      map((res) => (res.data ?? []).slice().sort((a, b) => a.orderNumber - b.orderNumber)),
      catchError(() => of([]))
    );
  }
}
