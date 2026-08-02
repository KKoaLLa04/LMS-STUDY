import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, AppUser, AppUserRole, CreateUserRequest, PagedResult, UpdateUserRequest } from '../models/user.model';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly baseUrl = `${environment.apiBaseUrl}/users`;

  constructor(private http: HttpClient) {}

  getUsers(role?: AppUserRole): Observable<ApiResponse<AppUser[]>> {
    let params = new HttpParams();
    if (role) params = params.set('role', role);
    return this.http.get<ApiResponse<AppUser[]>>(this.baseUrl, { params });
  }

  getUsersPaged(
    role?: AppUserRole,
    page = 1,
    pageSize = 10,
    keyword?: string
  ): Observable<ApiResponse<PagedResult<AppUser>>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (role) params = params.set('role', role);
    if (keyword) params = params.set('keyword', keyword);
    return this.http.get<ApiResponse<PagedResult<AppUser>>>(`${this.baseUrl}/paged`, { params });
  }

  getUserById(id: number): Observable<ApiResponse<AppUser>> {
    return this.http.get<ApiResponse<AppUser>>(`${this.baseUrl}/${id}`);
  }

  createUser(dto: CreateUserRequest): Observable<ApiResponse<AppUser>> {
    return this.http.post<ApiResponse<AppUser>>(this.baseUrl, dto);
  }

  updateUser(id: number, dto: UpdateUserRequest): Observable<ApiResponse<AppUser>> {
    return this.http.put<ApiResponse<AppUser>>(`${this.baseUrl}/${id}`, dto);
  }

  deleteUser(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }
}
