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

export interface UploadResult {
  url: string;
}

@Injectable({ providedIn: 'root' })
export class UploadService {
  private readonly baseUrl = `${environment.apiBaseUrl}/uploads`;

  constructor(private http: HttpClient) {}

  uploadVideo(file: File): Observable<ApiResponse<UploadResult>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<UploadResult>>(`${this.baseUrl}/video`, formData);
  }
}
