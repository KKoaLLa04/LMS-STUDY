import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, CreateKhoiHocRequest, KhoiHoc } from '../models/khoi-hoc.model';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class KhoiHocService {
  private readonly baseUrl = `${environment.apiBaseUrl}/khoihocs`;

  constructor(private http: HttpClient) {}

  getKhoiHocs(): Observable<ApiResponse<KhoiHoc[]>> {
    return this.http.get<ApiResponse<KhoiHoc[]>>(this.baseUrl);
  }

  getKhoiHocById(id: number): Observable<ApiResponse<KhoiHoc>> {
    return this.http.get<ApiResponse<KhoiHoc>>(`${this.baseUrl}/${id}`);
  }

  createKhoiHoc(dto: CreateKhoiHocRequest): Observable<ApiResponse<KhoiHoc>> {
    return this.http.post<ApiResponse<KhoiHoc>>(this.baseUrl, dto);
  }

  updateKhoiHoc(id: number, dto: CreateKhoiHocRequest): Observable<ApiResponse<KhoiHoc>> {
    return this.http.put<ApiResponse<KhoiHoc>>(`${this.baseUrl}/${id}`, dto);
  }

  deleteKhoiHoc(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }
}
