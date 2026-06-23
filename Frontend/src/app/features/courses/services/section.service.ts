import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, CreateSectionRequest, SectionDetail } from '../models/course.model';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SectionService {
  private readonly baseUrl = `${environment.apiBaseUrl}/sections`;

  constructor(private http: HttpClient) {}

  createSection(dto: CreateSectionRequest): Observable<ApiResponse<SectionDetail>> {
    return this.http.post<ApiResponse<SectionDetail>>(this.baseUrl, dto);
  }

  updateSection(id: number, dto: Omit<CreateSectionRequest, 'courseId'>): Observable<ApiResponse<SectionDetail>> {
    return this.http.put<ApiResponse<SectionDetail>>(`${this.baseUrl}/${id}`, dto);
  }

  deleteSection(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }
}
