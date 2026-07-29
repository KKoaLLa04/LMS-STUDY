import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from './follow.service';
import {
  CreateDiscussionPostRequest,
  DiscussionPost,
  DiscussionPostListItem,
  PagedResult,
  UpdateDiscussionPostRequest,
} from '../models/discussion-post.model';

@Injectable({ providedIn: 'root' })
export class DiscussionForumService {
  private readonly baseUrl = `${environment.apiBaseUrl}/discussionforums`;

  constructor(private http: HttpClient) {}

  getByCourse(courseId: number, page = 1, pageSize = 10, keyword?: string): Observable<ApiResponse<PagedResult<DiscussionPostListItem>>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (keyword) params = params.set('keyword', keyword);
    return this.http.get<ApiResponse<PagedResult<DiscussionPostListItem>>>(`${this.baseUrl}/by-course/${courseId}`, { params });
  }

  getById(id: number): Observable<ApiResponse<DiscussionPost>> {
    return this.http.get<ApiResponse<DiscussionPost>>(`${this.baseUrl}/${id}`);
  }

  create(dto: CreateDiscussionPostRequest): Observable<ApiResponse<DiscussionPost>> {
    return this.http.post<ApiResponse<DiscussionPost>>(this.baseUrl, dto);
  }

  update(id: number, dto: UpdateDiscussionPostRequest): Observable<ApiResponse<DiscussionPost>> {
    return this.http.put<ApiResponse<DiscussionPost>>(`${this.baseUrl}/${id}`, dto);
  }

  reply(parentPostId: number, dto: CreateDiscussionPostRequest): Observable<ApiResponse<DiscussionPost>> {
    return this.http.post<ApiResponse<DiscussionPost>>(`${this.baseUrl}/${parentPostId}/reply`, dto);
  }

  delete(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }

  like(id: number): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(`${this.baseUrl}/${id}/like`, {});
  }

  unlike(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}/like`);
  }
}
