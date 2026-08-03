import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
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

export interface PresignResult {
  uploadUrl: string;
  key: string;
}

export interface PlaybackUrlResult {
  url: string;
}

@Injectable({ providedIn: 'root' })
export class UploadService {
  private readonly baseUrl = `${environment.apiBaseUrl}/uploads`;
  private readonly playbackBaseUrl = `${environment.apiBaseUrl}/video-playback`;

  constructor(private http: HttpClient) {}

  /** Xin presigned URL rồi PUT file thẳng lên Cloudflare R2 (không qua backend). Trả về key đã lưu trên R2. */
  uploadVideo(file: File): Observable<string> {
    return this.http
      .post<ApiResponse<PresignResult>>(`${this.baseUrl}/video/presign`, {
        fileName: file.name,
        contentType: file.type,
      })
      .pipe(
        switchMap((res) => {
          const { uploadUrl, key } = res.data!;
          return this.http
            .put(uploadUrl, file, { headers: { 'Content-Type': file.type } })
            .pipe(map(() => key));
        })
      );
  }

  getLessonVideoPlaybackUrl(lessonId: number): Observable<ApiResponse<PlaybackUrlResult>> {
    return this.http.get<ApiResponse<PlaybackUrlResult>>(`${this.playbackBaseUrl}/lesson/${lessonId}`);
  }

  getCoursePreviewVideoUrl(courseId: number): Observable<ApiResponse<PlaybackUrlResult>> {
    return this.http.get<ApiResponse<PlaybackUrlResult>>(`${this.playbackBaseUrl}/course-preview/${courseId}`);
  }

  uploadImage(file: File): Observable<ApiResponse<UploadResult>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<UploadResult>>(`${this.baseUrl}/image`, formData);
  }

  uploadDocument(file: File): Observable<ApiResponse<UploadResult>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<UploadResult>>(`${this.baseUrl}/document`, formData);
  }
}
