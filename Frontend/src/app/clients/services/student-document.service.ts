import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, of } from 'rxjs';
import { ApiResponse } from '../models/course-api.model';
import { StudentDocumentApi } from '../models/document-api.model';
import { environment } from '../../../environments/environment';

/** Talks to Backend/Controllers/StudentDocumentsController.cs — xem Tài liệu chung trực tiếp. */
@Injectable({ providedIn: 'root' })
export class StudentDocumentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/student/documents`;

  getAll(): Observable<StudentDocumentApi[]> {
    return this.http.get<ApiResponse<StudentDocumentApi[]>>(this.baseUrl).pipe(
      map((res) => res.data ?? []),
      catchError(() => of([]))
    );
  }

  getById(id: number): Observable<StudentDocumentApi | null> {
    return this.http.get<ApiResponse<StudentDocumentApi>>(`${this.baseUrl}/${id}`).pipe(
      map((res) => res.data ?? null),
      catchError(() => of(null))
    );
  }
}
