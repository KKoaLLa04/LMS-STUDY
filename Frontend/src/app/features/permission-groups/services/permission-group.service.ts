import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ApiResponse,
  PermissionGroup,
  PermissionGroupDetail,
  SavePermissionGroupRequest
} from '../models/permission-group.model';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PermissionGroupService {
  private readonly baseUrl = `${environment.apiBaseUrl}/permission-groups`;

  constructor(private http: HttpClient) {}

  getGroups(): Observable<ApiResponse<PermissionGroup[]>> {
    return this.http.get<ApiResponse<PermissionGroup[]>>(this.baseUrl);
  }

  getGroupById(id: number): Observable<ApiResponse<PermissionGroupDetail>> {
    return this.http.get<ApiResponse<PermissionGroupDetail>>(`${this.baseUrl}/${id}`);
  }

  createGroup(dto: SavePermissionGroupRequest): Observable<ApiResponse<PermissionGroupDetail>> {
    return this.http.post<ApiResponse<PermissionGroupDetail>>(this.baseUrl, dto);
  }

  updateGroup(id: number, dto: SavePermissionGroupRequest): Observable<ApiResponse<PermissionGroupDetail>> {
    return this.http.put<ApiResponse<PermissionGroupDetail>>(`${this.baseUrl}/${id}`, dto);
  }

  deleteGroup(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }

  setMembers(id: number, userIds: number[]): Observable<ApiResponse<null>> {
    return this.http.put<ApiResponse<null>>(`${this.baseUrl}/${id}/members`, { userIds });
  }
}
