import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from './follow.service';

export interface PublicUser {
  id: number;
  fullName: string;
  avatarUrl?: string;
  role: string;
  gender?: string;
  /** Only meaningful when role === 'Teacher'. */
  subject?: string;
  experienceYears?: number;
  bio?: string;
}

@Injectable({ providedIn: 'root' })
export class PublicUserService {
  private readonly baseUrl = `${environment.apiBaseUrl}/users`;

  constructor(private http: HttpClient) {}

  getTeachers(): Observable<ApiResponse<PublicUser[]>> {
    return this.http.get<ApiResponse<PublicUser[]>>(`${this.baseUrl}/teachers/public`);
  }

  getById(id: number): Observable<ApiResponse<PublicUser>> {
    return this.http.get<ApiResponse<PublicUser>>(`${this.baseUrl}/public/${id}`);
  }
}
