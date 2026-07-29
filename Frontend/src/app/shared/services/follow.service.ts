import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  httpStatusCode?: number;
}

export interface FollowStatus {
  isFollowing: boolean;
  followersCount: number;
  followingCount: number;
}

@Injectable({ providedIn: 'root' })
export class FollowService {
  private readonly baseUrl = `${environment.apiBaseUrl}/follows`;

  constructor(private http: HttpClient) {}

  follow(userId: number): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(`${this.baseUrl}/${userId}`, {});
  }

  unfollow(userId: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${userId}`);
  }

  getStatus(userId: number): Observable<ApiResponse<FollowStatus>> {
    return this.http.get<ApiResponse<FollowStatus>>(`${this.baseUrl}/${userId}/status`);
  }
}
