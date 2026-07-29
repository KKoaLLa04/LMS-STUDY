import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../courses/models/course.model';
import { CreateDocumentRequest, DocumentItem, UpdateDocumentRequest } from '../models/document.model';
import { environment } from '../../../../environments/environment';

/** Talks to Backend/Controllers/DocumentsController.cs — quản lý kho Tài liệu dùng chung. */
@Injectable({ providedIn: 'root' })
export class DocumentService {
  private readonly baseUrl = `${environment.apiBaseUrl}/documents`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<DocumentItem[]>> {
    return this.http.get<ApiResponse<DocumentItem[]>>(this.baseUrl);
  }

  getById(id: number): Observable<ApiResponse<DocumentItem>> {
    return this.http.get<ApiResponse<DocumentItem>>(`${this.baseUrl}/${id}`);
  }

  create(dto: CreateDocumentRequest): Observable<ApiResponse<DocumentItem>> {
    return this.http.post<ApiResponse<DocumentItem>>(this.baseUrl, dto);
  }

  update(id: number, dto: UpdateDocumentRequest): Observable<ApiResponse<DocumentItem>> {
    return this.http.put<ApiResponse<DocumentItem>>(`${this.baseUrl}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }
}
