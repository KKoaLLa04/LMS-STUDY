import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/course.model';
import { CourseEnrollment } from '../models/enrollment.model';
import { environment } from '../../../../environments/environment';

/** [Admin/Teacher] Quản lý ghi danh học viên vào khóa học (Backend/Controllers/EnrollmentsController.cs). */
@Injectable({ providedIn: 'root' })
export class EnrollmentService {
  private readonly baseUrl = `${environment.apiBaseUrl}/enrollments`;

  constructor(private http: HttpClient) {}

  getByCourse(courseId: number): Observable<ApiResponse<CourseEnrollment[]>> {
    return this.http.get<ApiResponse<CourseEnrollment[]>>(`${this.baseUrl}/course/${courseId}`);
  }

  /** Ghi danh chỉ định một học sinh vào một khóa học — không qua luồng tự đăng ký. */
  adminEnroll(userId: number, courseId: number): Observable<ApiResponse<CourseEnrollment>> {
    return this.http.post<ApiResponse<CourseEnrollment>>(`${this.baseUrl}/admin`, { userId, courseId });
  }

  adminUnenroll(userId: number, courseId: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/admin/${courseId}/${userId}`);
  }
}
